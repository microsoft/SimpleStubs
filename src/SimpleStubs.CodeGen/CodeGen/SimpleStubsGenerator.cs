using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Etg.SimpleStubs.CodeGen.Config;
using Etg.SimpleStubs.CodeGen.CodeGen;
using System.Text;
using Microsoft.Build.Locator;

namespace Etg.SimpleStubs.CodeGen
{
    internal class SimpleStubsGenerator
    {
        private readonly IProjectStubber _projectStubber;
        private readonly SimpleStubsConfig _config;

        public SimpleStubsGenerator(IProjectStubber projectStubber, SimpleStubsConfig config)
        {
            _projectStubber = projectStubber;
            _config = config;
        }

        public async Task<string> GenerateStubs(string testProjectPath, string configuration, string platform, string visualStudioVersion)
        {
            RegisterVisualStudioInstance(visualStudioVersion);

            using (var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> {
                { "Configuration", configuration },
                { "Platform", platform}
            }))
            {
                Project currentProject = await workspace.OpenProjectAsync(testProjectPath);

                if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var diagnostic in workspace.Diagnostics)
                    {
                        stringBuilder.AppendLine(diagnostic.ToString());
                    }
                    Console.WriteLine("Simplestubs encountered errors when opening the workspace; Stubs will be generated only for projects that successfully opened. Errors: " + stringBuilder.ToString());
                }
                else
                {
                    Console.WriteLine("MSBuildWorkspace loaded with no errors!");
                }

                if (currentProject == null)
                {
                    throw new ArgumentException("Could not open the project located at " + testProjectPath);
                }

                List<Project> projectsToStub = GetListOfProjectsToStub(workspace, currentProject);
                if (!projectsToStub.Any())
                {
                    return string.Empty;
                }

                CompilationUnitSyntax cu = SF.CompilationUnit();
                var usings = new HashSet<UsingDirectiveSyntax>(new UsingDirectiveEqualityComparer());
                usings.Add(ToUsingDirective(" System"));
                usings.Add(ToUsingDirective(" System.Runtime.CompilerServices"));
                usings.Add(ToUsingDirective(" Etg.SimpleStubs"));

                foreach (Project project in projectsToStub)
                {
                    var res = await _projectStubber.StubProject(project, cu);
                    cu = res.CompilationUnit;
                    usings.UnionWith(res.Usings);
                }

                cu = cu.AddUsings(usings.ToArray());
                return Formatter.Format(cu, workspace).ToString();
            }
        }

        private static void RegisterVisualStudioInstance(string visualStudioVersion)
        {
            string[] versionParts = visualStudioVersion.Split('.');
            if (versionParts.Length < 1) throw new Exception($"VisualStudionVersion {visualStudioVersion} is invalid");
            string versionPrefix = versionParts[0];

            IEnumerable<VisualStudioInstance> vsInstances = MSBuildLocator.QueryVisualStudioInstances();
            VisualStudioInstance currentInstance = vsInstances.FirstOrDefault(i => i.Version.ToString().StartsWith(versionPrefix));
            if (currentInstance == null)
            {
                throw new Exception($"Could not find a visual studio instance that matches the version {visualStudioVersion}");
            }
            MSBuildLocator.RegisterInstance(currentInstance);
        }

        UsingDirectiveSyntax ToUsingDirective(string nameSpace)
        {
            return SF.UsingDirective(SF.IdentifierName(nameSpace)).WithLeadingTrivia();
        }

        private List<Project> GetListOfProjectsToStub(MSBuildWorkspace workspace, Project currentProject)
        {
            var projectsToStub = new List<Project>();

            if (_config.StubCurrentProject)
            {
                projectsToStub.Add(currentProject);
            }

            foreach (ProjectReference projectRef in currentProject.ProjectReferences)
            {
                Project project = workspace.CurrentSolution.GetProject(projectRef.ProjectId);
                if (!_config.IgnoredProjects.Contains(project.Name))
                {
                    projectsToStub.Add(project);
                }
            }

            return projectsToStub;
        }
    }
}