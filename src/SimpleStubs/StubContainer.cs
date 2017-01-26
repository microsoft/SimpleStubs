using System.Collections.Generic;

namespace Etg.SimpleStubs
{
    /// <summary>
    /// Holds the stubs for a given interface.
    /// This class is intended to be used by the generated stubs code, not user code.
    /// </summary>
    public class StubContainer<TStub>
    {
        private readonly Dictionary<string, SequenceContainer<object>> _stubs = new Dictionary<string, SequenceContainer<object>>();
        private readonly string _stubTypeName;

        public StubContainer()
        {
            _stubTypeName = typeof(TStub).ToString();
        }

        public TDelegate GetMethodStub<TDelegate>(string methodName)
        {
            string key = ToUniqueId<TDelegate>();
            SequenceContainer<object> sequenceContainer;
            _stubs.TryGetValue(key, out sequenceContainer);
            if (sequenceContainer == null)
            {
                throw new SimpleStubsException(
                    $"The stub {_stubTypeName} does not contain a stub for the method {methodName}");
            }

            try
            {
                return (TDelegate)sequenceContainer.Next;
            }
            catch (SequenceContainerException)
            {
                throw new SimpleStubsException(
                    $"The stub of method {methodName} was called more than expected");
            }
        }

        public bool TryGetMethodStub<TDelegate>(string methodName, out TDelegate del)
        {
            string key = ToUniqueId<TDelegate>();
            SequenceContainer<object> sequenceContainer;
            _stubs.TryGetValue(key, out sequenceContainer);
            if (sequenceContainer == null)
            {
                del = default(TDelegate);

                return false;
            }

            try
            {
                del = (TDelegate)sequenceContainer.Next;
            }
            catch (SequenceContainerException)
            {
                throw new SimpleStubsException(
                    $"The stub of method {methodName} was called more than expected");
            }

            return true;
        }

        public void SetMethodStub<TDelegate>(TDelegate del, int count, bool overwrite)
        {
            string key = ToUniqueId<TDelegate>();

            SequenceContainer<object> sequenceContainer;
            if (!overwrite)
            {
                _stubs.TryGetValue(key, out sequenceContainer);
                if (sequenceContainer == null)
                {
                    sequenceContainer = new SequenceContainer<object>();
                }
            }
            else
            {
                sequenceContainer = new SequenceContainer<object>();
            }

            sequenceContainer.Repeat(del, count);
            _stubs[key] = sequenceContainer;
        }

        private static string ToUniqueId<T>()
        {
            return typeof(T).ToString();
        }
    }
}