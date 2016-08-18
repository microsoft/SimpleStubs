using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Linq;
    using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal class PropertyStubber : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface)
        {
            if (!methodSymbol.IsPropertyAccessor())
            {
                return classDclr;
            }

            string delegateTypeName = NamingUtils.GetDelegateTypeName(methodSymbol, stubbedInterface);

            string propName = methodSymbol.AssociatedSymbol.Name;
            string propType =
                ((IPropertySymbol) methodSymbol.AssociatedSymbol).Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);
            var propDclr = GetPropDclr(classDclr, propName);
            if (propDclr == null)
            {
                propDclr = SF.PropertyDeclaration(SF.ParseTypeName(propType), SF.Identifier(propName))
                    .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(
                        SF.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));
            }

            if (methodSymbol.IsPropertyGetter())
            {
                var accessorDclr = SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, SF.Block(
                    SF.List(new[]
                    {
                        SF.ParseStatement("return " + StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, methodSymbol.Name, ""))
                    })));
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }
            else if (methodSymbol.IsPropertySetter())
            {
                var accessorDclr = SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SF.Block(
                    SF.List(new[]
                    {
                        SF.ParseStatement(StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, methodSymbol.Name, "value"))
                    })));
                propDclr = propDclr.AddAccessorListAccessors(accessorDclr);
            }

            if (propDclr != null)
            {
                PropertyDeclarationSyntax existingPropDclr = GetPropDclr(classDclr, propName);
                if (existingPropDclr != null)
                {
                    classDclr = classDclr.ReplaceNode(existingPropDclr, propDclr);
                }
                else
                {
                    classDclr = classDclr.AddMembers(propDclr);
                }
            }

            return classDclr;
        }

        private static PropertyDeclarationSyntax GetPropDclr(ClassDeclarationSyntax classDclr, string propName)
        {
            return
                classDclr.DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(p => p.Identifier.Text == propName);
        }
    }
}