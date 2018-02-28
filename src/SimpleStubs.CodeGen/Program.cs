using System;
using System.IO;
using System.Reflection;
using System.Text;
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
                Console.WriteLine(DecorateMessage($"ProjectPath cannot be empty"));
                return;
            }
            if (!File.Exists(projectPath))
            {
                Console.WriteLine(DecorateMessage($"{projectPath} does not exist"));
                return;
            }

            string outputPath = parser.Arguments["-OutputPath"];
            if (string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine(DecorateMessage($"OutputPath cannot be empty"));
                return;
            }

            string configuration = parser.Arguments["-Configuration"];
            if (string.IsNullOrEmpty(configuration))
            {
                Console.WriteLine(DecorateMessage($"Configuration cannot be empty"));
                return;
            }

            DiModule diModule = new DiModule(projectPath, outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            Console.WriteLine(DecorateMessage($"Generating stubs for project: {projectPath}"));

            try
            {
                string stubsCode = diModule.StubsGenerator.GenerateStubs(projectPath, configuration).Result;
                Console.WriteLine(DecorateMessage($"Writing stubs to file: {outputPath}"));
                File.WriteAllText(outputPath, stubsCode);
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(DecorateMessage($"Failed to generate stubs: {errorMessage}"));
            }
            catch(Exception e)
            {
                Console.WriteLine(DecorateMessage($"Failed to generate stubs: {e.ToString()}"));
            }
        }

        private static string DecorateMessage(string message)
        {
            return "SimpleStubs: " + message;
        }
    }
}
