using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Etg.SimpleStubs.CodeGen.Config;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    internal class InterfaceStubber : IInterfaceStubber
    {
        private readonly IEnumerable<IMethodStubber> _methodStubbers;
        private readonly IEnumerable<IPropertyStubber> _propertyStubbers;

        public InterfaceStubber(IEnumerable<IMethodStubber> methodStubbers, IEnumerable<IPropertyStubber> propertyStubbers)
        {
            _propertyStubbers = propertyStubbers;
            _methodStubbers = new List<IMethodStubber>(methodStubbers);
        }

        public CompilationUnitSyntax StubInterface(CompilationUnitSyntax cu, InterfaceDeclarationSyntax interfaceDclr, SemanticModel semanticModel, SimpleStubsConfig config)
        {
            INamedTypeSymbol interfaceType = semanticModel.GetDeclaredSymbol(interfaceDclr);
            NamespaceDeclarationSyntax namespaceNode = GetNamespaceNode(interfaceDclr);
            string interfaceName = interfaceType.GetGenericName();
            string stubName = NamingUtils.GetStubName(interfaceName);
            ClassDeclarationSyntax classDclr = SF.ClassDeclaration(SF.Identifier(stubName))
                .AddModifiers(SF.Token(RoslynUtils.GetVisibilityKeyword(interfaceType)))
                .WithBaseList(RoslynUtils.BaseList(interfaceName))
                .AddAttributeLists(AttributeListList(Attribute("CompilerGenerated")).ToArray());

            classDclr = RoslynUtils.CopyGenericConstraints(interfaceType, classDclr);
            classDclr = AddStubContainerField(classDclr, stubName);
            classDclr = AddMockBehaviorProperty(classDclr);
            classDclr = StubProperties(interfaceType, classDclr, semanticModel);
            classDclr = StubMethods(interfaceType, classDclr, semanticModel);
            classDclr = AddConstructor(interfaceType, classDclr, config);

            string fullNameSpace = semanticModel.GetDeclaredSymbol(namespaceNode).ToString();
            NamespaceDeclarationSyntax namespaceDclr = SF.NamespaceDeclaration(SF.IdentifierName(fullNameSpace))
                .WithUsings(namespaceNode.Usings);
            namespaceDclr = namespaceDclr.AddMembers(classDclr);
            cu = cu.AddMembers(namespaceDclr);
            return cu;
        }

        private ClassDeclarationSyntax AddConstructor(INamedTypeSymbol interfaceType, ClassDeclarationSyntax classDclr, SimpleStubsConfig config)
        {
            string ctorName = NamingUtils.GetStubName(interfaceType.Name);
            string defaultMockBehavior = GetValidMockBehaviorEnumValue(config.DefaultMockBehavior);

            var ctorParameter =
                SF.Parameter(SF.Identifier("mockBehavior"))
                    .WithType(SF.ParseTypeName("MockBehavior"))
                    .WithDefault(SF.EqualsValueClause(SF.ParseExpression($"MockBehavior.{defaultMockBehavior}")));

            classDclr = classDclr.AddMembers(SF.ConstructorDeclaration(ctorName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(SF.ParameterList().AddParameters(ctorParameter))
                .WithBody(SF.Block().AddStatements(SF.ExpressionStatement(
                    SF.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SF.IdentifierName("MockBehavior"),
                        SF.IdentifierName("mockBehavior")))
                )));
            return classDclr;
        }

        private ClassDeclarationSyntax StubProperties(INamedTypeSymbol interfaceType, ClassDeclarationSyntax classDclr, SemanticModel semanticModel)
        {
            IEnumerable<IPropertySymbol> propertiesToStub = RoslynUtils.GetAllMembers<IPropertySymbol>(interfaceType);
            foreach (IPropertySymbol propertySymbol in propertiesToStub)
            {
                foreach (IPropertyStubber propertyStubber in _propertyStubbers)
                {
                    classDclr = propertyStubber.StubProperty(classDclr, propertySymbol, interfaceType, semanticModel);
                }
            }
            return classDclr;
        }

        private ClassDeclarationSyntax StubMethods(INamedTypeSymbol interfaceType, ClassDeclarationSyntax classDclr, SemanticModel semanticModel)
        {
            IEnumerable<IMethodSymbol> methodsToStub = RoslynUtils.GetAllMembers<IMethodSymbol>(interfaceType);
            foreach (IMethodSymbol methodSymbol in methodsToStub)
            {
                foreach (IMethodStubber methodStubber in _methodStubbers)
                {
                    
                    classDclr = methodStubber.StubMethod(classDclr, methodSymbol, interfaceType, semanticModel);
                }
            }
            return classDclr;
        }

        private static ClassDeclarationSyntax AddStubContainerField(ClassDeclarationSyntax classDclr, string stubName)
        {
            classDclr = classDclr.AddMembers(
                SF.FieldDeclaration(
                    SF.VariableDeclaration(SF.ParseTypeName($"StubContainer<{stubName}>"),
                        SF.SeparatedList(new[]
                        {
                            SF.VariableDeclarator(SF.Identifier("_stubs"), null,
                                SF.EqualsValueClause(SF.ParseExpression($"new StubContainer<{stubName}>()")))
                        })))
                    .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword)));
            return classDclr;
        }

        private static ClassDeclarationSyntax AddMockBehaviorProperty(ClassDeclarationSyntax classDclr)
        {
            classDclr = classDclr.AddMembers(
                SF.PropertyDeclaration(SF.ParseTypeName("MockBehavior"), "MockBehavior")
                    .AddAccessorListAccessors(
                    SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)),
                    SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword))));

            return classDclr;
        }

        private static string GetValidMockBehaviorEnumValue(string suppliedValue)
        {
            if ("Strict".Equals(suppliedValue, StringComparison.OrdinalIgnoreCase))
            {
                return "Strict";
            }

            return "Loose";
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