using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

            statements.Add(SF.ParseStatement($"{delegateTypeName} del;\n"));
            statements.Add(SF.ParseStatement("if (_mockBehavior == MockBehavior.Strict)"));
            statements.Add(SF.Block(SF.ParseStatement($"del = _stubs.GetMethodStub<{delegateTypeName}>(\"{methodName}\");")));
            statements.Add(SF.ParseStatement("else"));

            var defaultReturnInvocation = outParameters.Select(p =>
                SF.ParseStatement($"{p.Name} = default ({p.Type.GetGenericName()});")).ToList();

            if (!returnsVoid)
            {
                defaultReturnInvocation.Add(SF.ParseStatement(GetDefaultReturnInvocationStatement(returnType, semanticModel)));
            }
            else
            {
                defaultReturnInvocation.Add(SF.ParseStatement("return;"));
            }

            statements.Add(SF.Block(
                SF.ParseStatement($"if (!_stubs.TryGetMethodStub<{delegateTypeName}>(\"{methodName}\", out del))"),
                SF.Block(defaultReturnInvocation)
            ));

            statements.Add(SF.ParseStatement($"{returnStatement}del.Invoke({parameters});"));

            return SF.Block(statements.ToArray());
        }

        private static string GetDefaultReturnInvocationStatement(ITypeSymbol returnType, SemanticModel semanticModel)
        {
            var genericTaskType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var taskType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            if (returnType.MetadataName.Equals(genericTaskType.MetadataName))
            {
                var namedReturnType = (INamedTypeSymbol) returnType;
                var genericReturnType = namedReturnType.TypeArguments.First();
                return $"return Task.FromResult(default({genericReturnType.GetGenericName()}));";
            }
            else if (returnType.MetadataName.Equals(taskType.MetadataName))
            {
                return "return Task.CompletedTask;";
            }
            else
            {
                return $"return default({returnType.GetMinimallyQualifiedName()});";
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