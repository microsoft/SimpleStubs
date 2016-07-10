using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    using SF = SyntaxFactory;

    internal class EventStubber : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface)
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
            ParameterSyntax[] parameters = GetEventParameters(eventSymbol);
            string onEventArgs = "sender";
            string eventTriggerArgs = "sender";
            if (parameters.Count() == 2)
            {
                onEventArgs += ", args";
                eventTriggerArgs += ", args";
            }
            else if (parameters.Count() == 1)
            {
                onEventArgs += ", null";
            }

            string eventType = GetEventType(eventSymbol);
            string onEventMethodName = "On_" + eventName;

            // Create OnEvent method
            MethodDeclarationSyntax onEventMethodDclr = SF.MethodDeclaration(SF.ParseTypeName("void"), onEventMethodName)
                .AddModifiers(SF.Token(SyntaxKind.ProtectedKeyword))
                .AddParameterListParameters(parameters)
                .WithBody(SF.Block(
                    SF.ParseStatement($"{eventType} handler = {eventName};\n"),
                    SF.ParseStatement($"if (handler != null) {{ handler({onEventArgs}); }}\n")
                    ));

            classDclr = classDclr.AddMembers(onEventMethodDclr);

            // Create event trigger method
            string eventTriggerMethodName = eventName + "_Raise";
            MethodDeclarationSyntax eventTriggerMethod = SF.MethodDeclaration(SF.ParseTypeName("void"),
                eventTriggerMethodName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(parameters)
                .WithBody(SF.Block(
                    SF.ParseStatement($"{onEventMethodName}({eventTriggerArgs});\n")
                    ));
            classDclr = classDclr.AddMembers(eventTriggerMethod);

            return classDclr;
        }

        private static ParameterSyntax[] GetEventParameters(IEventSymbol eventSymbol)
        {
            List<ParameterSyntax> parameters = new List<ParameterSyntax>
            {
                SF.Parameter(SF.Identifier("sender")).WithType(SF.ParseTypeName("object"))
            };
            INamedTypeSymbol type = (INamedTypeSymbol) (eventSymbol.Type);
            if (type.TypeArguments.Any())
            {
                parameters.Add(SF.Parameter(SF.Identifier("args"))
                    .WithType(SF.ParseTypeName(type.TypeArguments[0].Name)));
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
    }
}