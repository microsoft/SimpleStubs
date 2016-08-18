using System;
using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen
{
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
                    $"The stub {_stubTypeName} does not contain a stub for the method {methodName}");
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
}