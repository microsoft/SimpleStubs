using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    using SF = SyntaxFactory;

    /// <summary>
    /// Generate the code needed to stub an event
    /// </summary>
    internal class EventStubber : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol, INamedTypeSymbol stubbedInterface, SemanticModel semanticModel)
        {
            // only handle EventAdd and ignore EventRemove because we only need to stub the event once
            if (!methodSymbol.IsEventAdd())
            {
                return classDclr;
            }

            // add the event implementation to the stub
            IEventSymbol eventSymbol = (IEventSymbol) methodSymbol.AssociatedSymbol;
            EventFieldDeclarationSyntax eventDclr = ToEventDclr(eventSymbol);
            classDclr = classDclr.AddMembers(eventDclr);

            string eventName = eventSymbol.Name;

            bool isCustomDelegateEvent = IsCustomDelegateBasedEvent(eventSymbol, semanticModel);
            ParameterSyntax[] parameters = GetEventParameters(eventSymbol, isCustomDelegateEvent);
            string onEventArgs;
            string eventTriggerArgs;
            string methodReturnType = "void";
            string customDelegateReturnType = "";
            bool hasReturnType = false;

            if (isCustomDelegateEvent)
            {
                IMethodSymbol delegateInvokeMethodSymbol = ((INamedTypeSymbol)(eventSymbol.OriginalDefinition).Type).DelegateInvokeMethod;
                onEventArgs = StubbingUtils.FormatParameters(delegateInvokeMethodSymbol);
                eventTriggerArgs = onEventArgs;
                if (!delegateInvokeMethodSymbol.ReturnsVoid)
                {
                    hasReturnType = true;
                    customDelegateReturnType = delegateInvokeMethodSymbol.ReturnType.GetFullyQualifiedName();
                    methodReturnType = $"global::System.Collections.Generic.IEnumerable<{customDelegateReturnType}>";
                }
            }
            else
            {
                onEventArgs = "sender";
                eventTriggerArgs = "sender";
                if (parameters.Count() == 2)
                {
                    onEventArgs += ", args";
                    eventTriggerArgs += ", args";
                }
                else if (parameters.Count() == 1)
                {
                    onEventArgs += ", null";
                }
            }

            string eventType = GetEventType(eventSymbol);
            string onEventMethodName = "On_" + eventName;

            // Create OnEvent method
            BlockSyntax onEventMethodDclrBlock;
            if (hasReturnType)
            {
                onEventMethodDclrBlock = SF.Block(
                    SF.ParseStatement($"{eventType} handler = {eventName};\n"),
                    SF.ParseStatement("if (handler == null) {{ yield break; }}\n"),
                    SF.ParseStatement($"foreach (var listener in handler.GetInvocationList()){{ if (listener.DynamicInvoke({onEventArgs}) is {customDelegateReturnType} ret){{ yield return ret; }} }}"));
            }
            else
            {
                onEventMethodDclrBlock = SF.Block(
                    SF.ParseStatement($"{eventType} handler = {eventName};\n"),
                    SF.ParseStatement($"if (handler != null) {{ handler({onEventArgs}); }}\n"));
            }

            MethodDeclarationSyntax onEventMethodDclr = SF.MethodDeclaration(SF.ParseTypeName(methodReturnType), onEventMethodName)
                .AddModifiers(SF.Token(SyntaxKind.ProtectedKeyword))
                .AddParameterListParameters(parameters)
                .WithBody(onEventMethodDclrBlock);

            classDclr = classDclr.AddMembers(onEventMethodDclr);

            // Create event trigger method
            string eventTriggerMethodName = eventName + "_Raise";

            BlockSyntax eventTriggerMethodBlock;
            if (hasReturnType)
            {
                eventTriggerMethodBlock = SF.Block(SF.ParseStatement($"return {onEventMethodName}({eventTriggerArgs});\n"));
            }
            else
            {
                eventTriggerMethodBlock = SF.Block(SF.ParseStatement($"{onEventMethodName}({eventTriggerArgs});\n"));
            }

            MethodDeclarationSyntax eventTriggerMethod = SF.MethodDeclaration(SF.ParseTypeName(methodReturnType),
                eventTriggerMethodName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(parameters)
                .WithBody(eventTriggerMethodBlock);
            classDclr = classDclr.AddMembers(eventTriggerMethod);

            return classDclr;
        }

        private static ParameterSyntax[] GetEventParameters(IEventSymbol eventSymbol, bool isCustomDelegateEvent)
        {

            var parameters = new List<ParameterSyntax>();
            INamedTypeSymbol type = (INamedTypeSymbol) (eventSymbol.Type);

            if (isCustomDelegateEvent)
            {
                IMethodSymbol delegateInvokeMethodSymbol = ((INamedTypeSymbol)(eventSymbol).Type).DelegateInvokeMethod;
                parameters.AddRange(RoslynUtils.GetMethodParameterSyntaxList(delegateInvokeMethodSymbol).ToArray());
            }
            else
            {
                parameters.Add(SF.Parameter(SF.Identifier("sender")).WithType(SF.ParseTypeName("object")));
                if (type.TypeArguments.Any())
                {
                    parameters.Add(SF.Parameter(SF.Identifier("args"))
                        .WithType(SF.ParseTypeName(type.TypeArguments[0].GetFullyQualifiedName())));
                }
            }

            return parameters.ToArray();
        }

        private static EventFieldDeclarationSyntax ToEventDclr(IEventSymbol eventSymbol)
        {
            string eventName = eventSymbol.Name;
            string eventType = GetEventType(eventSymbol);
            EventFieldDeclarationSyntax eventDclr = SF.EventFieldDeclaration(
                SF.VariableDeclaration(SF.IdentifierName(eventType),
                    SF.SeparatedList(new[] {SF.VariableDeclarator(eventName)})))
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword));
            return eventDclr;
        }

        private static string GetEventType(IEventSymbol eventSymbol)
        {
            return eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static bool IsCustomDelegateBasedEvent(IEventSymbol eventSymbol, SemanticModel semanticModel)
        {
            var genericEventType = semanticModel.Compilation.GetTypeByMetadataName("System.EventHandler`1");
            var eventType = semanticModel.Compilation.GetTypeByMetadataName("System.EventHandler");

            if (eventSymbol.Type.MetadataName.Equals(genericEventType.MetadataName) || eventSymbol.Type.MetadataName.Equals(eventType.MetadataName))
            {
                return false;
            }

            return true;
        }
    }
}