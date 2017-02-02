using System.Linq;
using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    using SF = SyntaxFactory;

    internal class PropertyStubber : IPropertyStubber
    {
        public ClassDeclarationSyntax StubProperty(ClassDeclarationSyntax classDclr, IPropertySymbol propertySymbol, INamedTypeSymbol stubbedInterface, SemanticModel semanticModel)
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
                        StubbingUtils.GetInvocationBlockSyntax(delegateTypeName, getMethodSymbol.Name, parameters, 
                        Enumerable.Empty<IParameterSymbol>(), getMethodSymbol.ReturnType, semanticModel)
                    })));

                propDclr = CreatePropertyDclr(getMethodSymbol, indexerType);
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }
                
            if (propertySymbol.SetMethod != null)
            {
                var voidType = semanticModel.Compilation.GetTypeByMetadataName("System.Void");
                IMethodSymbol setMethodSymbol = propertySymbol.SetMethod;
                string parameters = $"{StubbingUtils.FormatParameters(setMethodSymbol)}";
                string delegateTypeName = NamingUtils.GetDelegateTypeName(setMethodSymbol, stubbedInterface);
                var accessorDclr = SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SF.Block(
                    SF.List(new[]
                    {
                        StubbingUtils.GetInvocationBlockSyntax(delegateTypeName, setMethodSymbol.Name, parameters, 
                        Enumerable.Empty<IParameterSymbol>(), voidType, semanticModel)
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