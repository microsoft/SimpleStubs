using Autofac;
using Etg.SimpleStubs.CodeGen.CodeGen;
using Etg.SimpleStubs.CodeGen.Config;
using System.IO;

namespace Etg.SimpleStubs.CodeGen.DI
{
    class DiModule
    {
        private readonly IContainer _container;

        public DiModule(string testProjectPath, string stubsFilePath)
        {
            _container = RegisterTypes(testProjectPath, stubsFilePath).Build();
        }

        public static ContainerBuilder RegisterTypes(string testProjectPath, string stubsFilePath)
        {
            ContainerBuilder cb = new ContainerBuilder();

            string configFilePath = Path.Combine(Path.GetDirectoryName(testProjectPath), "SimpleStubs.json");
            SimpleStubsConfig config = new ConfigLoader().LoadConfig(configFilePath);

            cb.RegisterInstance(config);

            cb.Register((c) =>
            {
                IInterfaceStubber interfaceStubber = new InterfaceStubber(
                    new IMethodStubber[] {
                    new OrdinaryMethodStubber(),
                    new EventStubber(),
                    new PropertyStubber(),
                    new StubbingPropertiesGenerator() });
                return interfaceStubber;
            }).As<IInterfaceStubber>().SingleInstance();

            cb.RegisterType<ProjectStubber>().As<IProjectStubber>().SingleInstance();
            cb.RegisterType<SimpleStubsGenerator>().AsSelf();

            return cb;
        }

        public SimpleStubsGenerator StubsGenerator => _container.Resolve<SimpleStubsGenerator>();
    }
}
