using System;
using System.Collections.Generic;
using System.Linq;

namespace Etg.SimpleStubs
{
    /// <summary>
    /// Defines the types of behavior that can be applied to stub members that have not had a behavior applied.
    /// </summary>
    public enum MockBehavior
    {
        /// <summary>
        /// Methods and properties on a mock instance in strict mode throw an exception when called before any behaviors have been assigned.
        /// </summary>
        Strict,
        /// <summary>
        /// Methods and properties on a mock instance in loose mode use a default behavior when called before any behaviors have been assigned.
        /// </summary>
        Loose
    }

    public class Times
    {
        public const int Once = 1;
        public const int Twice = 2;
        public const int Forever = int.MaxValue;
    }

    public class SequenceContainer<T>
    {
        private readonly List<SequenceElement<T>> _sequence = new List<SequenceElement<T>>();

        public virtual SequenceContainer<T> Repeat(T element, int count)
        {
            _sequence.Add(new SequenceElement<T>{Element = element, Count = count});
            return this;
        }

        public int CallCount { get; private set; } = 0;

        public SequenceContainer<T> Forever(T element)
        {
            return Repeat(element, int.MaxValue);
        }

        public SequenceContainer<T> Once(T element)
        {
            return Repeat(element, 1);
        }

        public SequenceContainer<T> Twice(T element)
        {
            return Repeat(element, 2);
        }

        public virtual T Next
        {
            get
            {
                lock (_sequence)
                {
                    if (!_sequence.Any())
                    {
                        throw new SequenceContainerException(
                            "The stub sequence has been called more than expected");
                    }

                    var se = _sequence[0];
                    T e = se.Element;
                    if (--se.Count == 0)
                    {
                        _sequence.RemoveAt(0);
                    }

                    CallCount++;
                    return e;
                }
            }
        }

        private class SequenceElement<TElement>
        {
            public TElement Element
            {
                get;
                set;
            }

            public int Count
            {
                get;
                set;
            }
        }
    }

    public class SequenceContainerException : Exception
    {
        public SequenceContainerException(string msg) : base(msg)
        {
        }
    }

    public class SimpleStubsException : Exception
    {
        public SimpleStubsException(string message) : base(message)
        {
        }
    }

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
            TDelegate del;
            if (!TryGetMethodStub(methodName, out del))
            {
                throw new SimpleStubsException(
                    $"The stub {_stubTypeName} does not contain a stub for the method {methodName}");
            }

            return del;
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

    public static class StubsUtils
    {
        public static SequenceContainer<T> Sequence<T>()
        {
            return new SequenceContainer<T>();
        }
    }
}