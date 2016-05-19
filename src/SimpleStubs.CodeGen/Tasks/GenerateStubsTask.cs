using Microsoft.Build.Framework;
using System;
using System.IO;
using Etg.SimpleStubs.CodeGen.DI;

namespace Etg.SimpleStubs.CodeGen.Tasks
{
    /// <summary>
    /// </summary>
    public class GenerateStubsTask : Microsoft.Build.Utilities.AppDomainIsolatedTask
    {
        [Required]
        public string OutputPath
        {
            get;
            set;
        }

        [Required]
        public string ProjectPath 
        {
            get;
            set;
        }

        public override bool Execute()
        {
            try
            {
                LogMessage("Generating stubs"); 
                DiModule diModule = new DiModule(ProjectPath, OutputPath);
                File.WriteAllText(OutputPath, diModule.StubsGenerator.GenerateStubs(ProjectPath).Result);
                return true;
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }

            return false;
        }

        private void LogMessage(string message)
        {
            Log.LogMessage(MessageImportance.High, DecorateMessage(message));
        }

        private static string DecorateMessage(string message)
        {
            return "SimpleStubs: " + message;
        }

        private void LogWarning(string message)
        {
            Log.LogWarning(DecorateMessage(message));
        }
    }
}