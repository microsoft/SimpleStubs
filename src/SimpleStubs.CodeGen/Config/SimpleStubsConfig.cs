using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen.Config
{
    internal class SimpleStubsConfig
    {
        public SimpleStubsConfig(IEnumerable<string> ignoredProjects,
            IEnumerable<string> ignoredInterfaces,
            bool stubInternalInterfaces,
            bool stubCurrentProject,
            string defaultMockBehavior)
        {
            IgnoredProjects = new HashSet<string>(ignoredProjects);
            IgnoredInterfaces = new HashSet<string>(ignoredInterfaces);
            StubInternalInterfaces = stubInternalInterfaces;
            StubCurrentProject = stubCurrentProject;
            DefaultMockBehavior = defaultMockBehavior;
        }

        public ISet<string> IgnoredProjects { get; }

        public ISet<string> IgnoredInterfaces { get; }

        public bool StubInternalInterfaces { get; }

        public bool StubCurrentProject { get; }

        public string DefaultMockBehavior { get; }
    }
}