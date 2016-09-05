using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    using SF = SyntaxFactory;

    internal class PropertyStubber : IPropertyStubber
    {
        public ClassDeclarationSyntax StubProperty(ClassDeclarationSyntax classDclr, IPropertySymbol propertySymbol,
            INamedTypeSymbol stubbedInterface)
        {
            string indexerType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            BasePropertyDeclarationSyntax propDclr = null;

            if (propertySymbol.GetMethod != null)
            {
                IMethodSymbol getMethodSymbol = propertySymbol.GetMethod;
                string parameters = StubbingUtils.FormatParameters(getMethodSymbol);

                string delegateTypeName = NamingUtils.GetDelegateTypeName(getMethodSymbol, stubbedInterface);
                var accessorDclr = SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SF.Block(
                    SF.List(new[]
                    {
                        SF.ParseStatement("return " + StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, getMethodSymbol.Name, parameters))
                    })));

                propDclr = CreatePropertyDclr(getMethodSymbol, indexerType);
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
                
            }
            if (propertySymbol.SetMethod != null)
            {
                IMethodSymbol setMethodSymbol = propertySymbol.SetMethod;
                string parameters = $"{StubbingUtils.FormatParameters(setMethodSymbol)}";
                string delegateTypeName = NamingUtils.GetDelegateTypeName(setMethodSymbol, stubbedInterface);
                var accessorDclr = SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SF.Block(
                    SF.List(new[]
                    {
                        SF.ParseStatement(StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, setMethodSymbol.Name, parameters))
                    })));
                if (propDclr == null)
                {
                    propDclr = CreatePropertyDclr(setMethodSymbol, indexerType);
                }
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }

            classDclr = classDclr.AddMembers(propDclr);
            return classDclr;
        }

        private BasePropertyDeclarationSyntax CreatePropertyDclr(IMethodSymbol methodSymbol, string propType)
        {
            if (methodSymbol.IsIndexerAccessor())
            {
                IndexerDeclarationSyntax indexerDclr = SF.IndexerDeclaration(
                    SF.ParseTypeName(propType))
                    .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(
                        SF.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));
                indexerDclr = indexerDclr.AddParameterListParameters(
                        RoslynUtils.GetMethodParameterSyntaxList(methodSymbol).ToArray());
                return indexerDclr;
            }

            string propName = methodSymbol.AssociatedSymbol.Name;
            PropertyDeclarationSyntax propDclr = SF.PropertyDeclaration(SF.ParseTypeName(propType), SF.Identifier(propName))
            .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(
                SF.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));
            return propDclr;
        }
    }
}