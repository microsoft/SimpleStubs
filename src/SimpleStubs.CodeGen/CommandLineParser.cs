using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etg.SimpleStubs.CodeGen
{
    class CommandLineParser
    {
        public CommandLineParser()
        {
            Arguments = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Arguments { get; private set; }

        public void Parse(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    var colon = arg.IndexOf(':');

                    if (colon != -1)
                    {
                        this.Arguments.Add(arg.Substring(0, colon), arg.Substring(colon + 1).Trim('\'', '\"'));
                    }
                    else
                    {
                        this.Arguments.Add(arg, string.Empty);
                    }
                }
            }
        }
    }
}
