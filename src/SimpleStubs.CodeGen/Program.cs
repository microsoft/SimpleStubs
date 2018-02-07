using System;
using System.IO;
using Etg.SimpleStubs.CodeGen;
using Etg.SimpleStubs.CodeGen.DI;

namespace Etg.SimpleStubs.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Parse(args);

            string projectPath = parser.Arguments["-ProjectPath"];
            if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
            {
                Console.WriteLine(DecorateMessage($"{projectPath} does not exist"));
                return;
            }

            string outputPath = parser.Arguments["-OutputPath"];
            if (string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine(DecorateMessage($"{outputPath} cannot be empty"));
                return;
            }

            DiModule diModule = new DiModule(projectPath, outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            Console.WriteLine(DecorateMessage($"Generating stubs for project: {projectPath}"));
            string stubsCode = diModule.StubsGenerator.GenerateStubs(projectPath).Result;
            Console.WriteLine(DecorateMessage($"Writing stubs to file: {outputPath}"));
            File.WriteAllText(outputPath, stubsCode);

            return;
        }

        private static string DecorateMessage(string message)
        {
            return "SimpleStubs: " + message;
        }
    }
}
