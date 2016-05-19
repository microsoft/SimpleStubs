using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.IO;

namespace Etg.SimpleStubs.CodeGen.Config
{
    class ConfigLoader
    {
        public SimpleStubsConfig LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                return JsonConvert.DeserializeObject<SimpleStubsConfig>(File.ReadAllText(configFilePath));
            }

            return new SimpleStubsConfig(new string[]{}, new string[]{}, false);
        }
    }
}