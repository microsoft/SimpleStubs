using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen.Config
{
    class SimpleStubsConfig
    {
        public SimpleStubsConfig(IEnumerable<string> ignoredProjects, 
            IEnumerable<string> ignoredInterfaces,
            bool stubInternalInterfaces)
        {
            IgnoredProjects = new HashSet<string>(ignoredProjects);
            IgnoredInterfaces = new HashSet<string>(ignoredInterfaces);
            StubInternalInterfaces = stubInternalInterfaces;
        }

        public ISet<string> IgnoredProjects
        {
            get;
        }

        public ISet<string> IgnoredInterfaces
        {
            get;
        }

        public bool StubInternalInterfaces
        {
            get;
        }
    }
}