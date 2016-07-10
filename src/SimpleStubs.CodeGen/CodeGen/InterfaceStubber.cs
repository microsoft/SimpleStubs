using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    internal class InterfaceStubber : IInterfaceStubber
    {
        private readonly IEnumerable<IMethodStubber> _methodStubbers;

        public InterfaceStubber(IEnumerable<IMethodStubber> methodStubbers)
        {
            _methodStubbers = new List<IMethodStubber>(methodStubbers);
        }

        public CompilationUnitSyntax StubInterface(CompilationUnitSyntax cu, InterfaceDeclarationSyntax interfaceDclr,
            SemanticModel semanticModel)
        {
            INamedTypeSymbol interfaceType = semanticModel.GetDeclaredSymbol(interfaceDclr);
            NamespaceDeclarationSyntax namespaceNode = GetNamespaceNode(interfaceDclr);
            string interfaceName = interfaceType.GetGenericName();
            string stubName = NamingUtils.GetStubName(interfaceName);
            ClassDeclarationSyntax classDclr = SF.ClassDeclaration(SF.Identifier(stubName))
                .AddModifiers(SF.Token(RoslynUtils.GetVisibilityKeyword(interfaceType))
                //,SF.Token(SyntaxKind.PartialKeyword)
                )
                .WithBaseList(RoslynUtils.BaseList(interfaceName))
                .AddAttributeLists(AttributeListList(Attribute("CompilerGenerated")).ToArray());

            classDclr = classDclr.AddMembers(
                SF.FieldDeclaration(
                    SF.VariableDeclaration(SF.ParseTypeName("Dictionary<string, object>"),
                        SF.SeparatedList(new[]
                        {
                            SF.VariableDeclarator(SF.Identifier("_stubs"), null,
                                SF.EqualsValueClause(SF.ParseExpression("new Dictionary<string, object>()")))
                        })))
                    .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword)));


            List<IMethodSymbol> methodsToStub = RoslynUtils.GetAllMethods(interfaceType);
            foreach (IMethodSymbol methodSymbol in methodsToStub)
            {
                foreach (IMethodStubber methodStubber in _methodStubbers)
                {
                    classDclr = methodStubber.StubMethod(classDclr, methodSymbol, interfaceType);
                }
            }

            string fullNameSpace = semanticModel.GetDeclaredSymbol(namespaceNode).ToString();
            NamespaceDeclarationSyntax namespaceDclr = SF.NamespaceDeclaration(SF.IdentifierName(fullNameSpace))
                .WithUsings(namespaceNode.Usings);
            namespaceDclr = namespaceDclr.AddMembers(classDclr);
            cu = cu.AddMembers(namespaceDclr);
            return cu;
        }

        private static NamespaceDeclarationSyntax GetNamespaceNode(InterfaceDeclarationSyntax interfaceNode)
        {
            var namespaceNode = interfaceNode.Parent as NamespaceDeclarationSyntax;
            if (namespaceNode == null)
            {
                throw new Exception("A grain interface must be declared inside a namespace");
            }

            return namespaceNode;
        }

        private static SyntaxList<AttributeListSyntax> AttributeListList(params AttributeSyntax[] attributes)
        {
            var list = new SyntaxList<AttributeListSyntax>();
            foreach (AttributeSyntax attributeSyntax in attributes)
            {
                list = list.Add(AttributeList(attributeSyntax));
            }
            return list;
        }

        private static AttributeListSyntax AttributeList(params AttributeSyntax[] attributes)
        {
            SeparatedSyntaxList<AttributeSyntax> separatedList = SF.SeparatedList<AttributeSyntax>();
            foreach (var attributeSyntax in attributes)
            {
                separatedList = separatedList.Add(attributeSyntax);
            }
            return SF.AttributeList(separatedList);
        }

        private static AttributeSyntax Attribute(string attributeName)
        {
            return SF.Attribute(SF.IdentifierName(attributeName));
        }
    }
}