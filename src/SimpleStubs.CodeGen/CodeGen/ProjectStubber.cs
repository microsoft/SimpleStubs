using Etg.SimpleStubs.CodeGen.Config;
using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Etg.SimpleStubs.CodeGen.CodeGen
{
    internal class ProjectStubber : IProjectStubber
    {
        private readonly IInterfaceStubber _interfaceStubber;
        private readonly SimpleStubsConfig _config;

        public ProjectStubber(IInterfaceStubber interfaceStubber, SimpleStubsConfig config)
        {
            _interfaceStubber = interfaceStubber;
            _config = config;
        }

        public async Task<StubProjectResult> StubProject(Project project, CompilationUnitSyntax cu)
        {
            var usings = new List<string>();
            foreach (Document document in project.Documents)
            {
                SyntaxTree syntaxTree = await document.GetSyntaxTreeAsync();
                SemanticModel semanticModel = await document.GetSemanticModelAsync();
                IEnumerable<InterfaceDeclarationSyntax> interfaces =
                    syntaxTree.GetRoot()
                        .DescendantNodes()
                        .OfType<InterfaceDeclarationSyntax>()
                        .Where(SatisfiesVisibilityConstraints);
                if (!interfaces.Any())
                {
                    continue;
                }

                cu = AddStubContainerDefinition(cu);

                foreach (var interfaceDclr in interfaces)
                {
                    try
                    {
                        INamedTypeSymbol interfaceType = semanticModel.GetDeclaredSymbol(interfaceDclr);
                        if (!_config.IgnoredInterfaces.Contains(interfaceType.GetQualifiedName()))
                        {
                            LogWarningsIfAny(semanticModel);
                            cu = _interfaceStubber.StubInterface(cu, interfaceDclr, semanticModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Could not generate stubs for interface {interfaceDclr}, Exception: {e}");
                    }
                }
                usings.AddRange(syntaxTree.GetCompilationUnitRoot().Usings.Select(@using => @using.Name.ToString()));
            }

            return new StubProjectResult(cu, usings);
        }

        private CompilationUnitSyntax AddStubContainerDefinition(CompilationUnitSyntax cu)
        {
            string text = @"
    /// <summary>
    /// Holds the stubs for a given interface
    /// </summary>
    public class StubContainer<TStub>
    {
        private readonly Dictionary<string, object> _stubs = new Dictionary<string, object>();
        private readonly string _stubTypeName;

        public StubContainer()
        {
            _stubTypeName = typeof(TStub).ToString();
        }

        public TDelegate GetMethodStub<TDelegate>(string methodName)
        {
            string key = ToUniqueId<TDelegate>();
            object value;
            _stubs.TryGetValue(key, out value);
            if (value == null)
            {
                throw new InvalidOperationException(
                    $""The stub { _stubTypeName} does not contain a stub for the method { methodName}"");
            }
            return (TDelegate) value;
        }

    public void SetMethodStub<TDelegate>(TDelegate del)
    {
        string key = ToUniqueId<TDelegate>();
        _stubs[key] = del;
    }

    private static string ToUniqueId<T>()
    {
        return typeof(T).ToString();
    }
}
";
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            ClassDeclarationSyntax classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            return cu.AddMembers(classDeclaration);
        }

        private bool SatisfiesVisibilityConstraints(InterfaceDeclarationSyntax i)
        {
            return i.IsPublic() || (_config.StubInternalInterfaces && i.IsInternal());
        }

        private void LogWarningsIfAny(SemanticModel semanticModel)
        {
            foreach (var diagnostic in semanticModel.GetDiagnostics())
            {
                Trace.TraceInformation(diagnostic.ToString());
            }
        }
    }
}