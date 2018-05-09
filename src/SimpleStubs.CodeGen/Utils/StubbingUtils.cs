using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal static class StubbingUtils
    {
        public static BlockSyntax GetInvocationBlockSyntax(string delegateTypeName, string methodName, string parameters,
            IEnumerable<IParameterSymbol> outParameters, ITypeSymbol returnType, SemanticModel semanticModel)
        {
            var voidType = semanticModel.Compilation.GetTypeByMetadataName("System.Void");
            bool returnsVoid = returnType.Equals(voidType);
            var statements = new List<StatementSyntax>();
            string returnStatement = returnsVoid ? string.Empty : "return ";

            statements.Add(SF.ParseStatement($"{delegateTypeName} del;{System.Environment.NewLine}"));

            // Prepare if/else block to determine how uninitialized stubs are handled.
            var ifStrictExpression = SF.ParseExpression("MockBehavior == MockBehavior.Strict");
            var ifStrictTrueSyntax = SF.Block(SF.ParseStatement($"del = _stubs.GetMethodStub<{delegateTypeName}>(\"{methodName}\");{System.Environment.NewLine}"));

            var defaultReturnInvocation = outParameters.Select(p =>
                SF.ParseStatement($"{p.Name} = default ({p.Type.GetGenericName()});")).ToList();

            if (!returnsVoid)
            {
                defaultReturnInvocation.Add(SF.ParseStatement(GetDefaultReturnInvocationStatement(returnType, semanticModel)));
            }
            else
            {
                defaultReturnInvocation.Add(SF.ParseStatement($"return;{System.Environment.NewLine}"));
            }

            var ifTryGetMethodStubExpression = SF.ParseExpression($"!_stubs.TryGetMethodStub<{delegateTypeName}>(\"{methodName}\", out del)");

            var ifStrictFalseSyntax = SF.Block(SF.IfStatement(ifTryGetMethodStubExpression, SF.Block(defaultReturnInvocation)));

            statements.Add(SF.IfStatement(ifStrictExpression, ifStrictTrueSyntax, SF.ElseClause(ifStrictFalseSyntax)));

            // Add default invocation.
            statements.Add(SF.ParseStatement($"{returnStatement}del.Invoke({parameters});{System.Environment.NewLine}"));

            return SF.Block(statements.ToArray());
        }

        private static string GetDefaultReturnInvocationStatement(ITypeSymbol returnType, SemanticModel semanticModel)
        {
            var genericTaskType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var taskType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var asyncActionType = semanticModel.Compilation.GetTypeByMetadataName("Windows.Foundation.IAsyncAction");
            var asyncOperationType = semanticModel.Compilation.GetTypeByMetadataName("Windows.Foundation.IAsyncOperation`1");

            if (returnType.MetadataName.Equals(genericTaskType.MetadataName))
            {
                var namedReturnType = (INamedTypeSymbol) returnType;
                var genericReturnType = namedReturnType.TypeArguments.First();
                return $"return System.Threading.Tasks.Task.FromResult(default({genericReturnType.GetFullyQualifiedName()}));{System.Environment.NewLine}";
            }
            else if (returnType.MetadataName.Equals(taskType.MetadataName))
            {
                // do not use Task.CompletedTask to stay compatible with .Net 4.5
                return $"return System.Threading.Tasks.Task.FromResult(true);{System.Environment.NewLine}";
            }
            else if (asyncActionType != null && returnType.MetadataName.Equals(asyncActionType.MetadataName))
            {
                // do not use Task.CompletedTask to stay compatible with .Net 4.5
                return $"return System.Threading.Tasks.Task.FromResult(true).AsAsyncAction();{System.Environment.NewLine}";
            }
            else if (asyncOperationType != null && returnType.MetadataName.Equals(asyncOperationType.MetadataName))
            {
                var namedReturnType = (INamedTypeSymbol)returnType;
                var genericReturnType = namedReturnType.TypeArguments.First();
                return $"return System.Threading.Tasks.Task.FromResult(default({genericReturnType.GetFullyQualifiedName()})).AsAsyncOperation();{System.Environment.NewLine}";
            }
            else
            {
                return $"return default({returnType.GetFullyQualifiedName()});{System.Environment.NewLine}";
            }
        }

        public static string FormatParameters(IMethodSymbol methodSymbol)
        {
            return string.Join(", ", methodSymbol.Parameters.Select(p =>
            {
                if (p.RefKind == RefKind.Out)
                {
                    return $"out {p.Name}";
                }
                if (p.RefKind == RefKind.Ref)
                {
                    return $"ref {p.Name}";
                }
                return p.Name;
            }));
        }
    }
}