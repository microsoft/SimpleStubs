using System;
using System.Collections.Generic;
using System.Linq;

namespace Etg.SimpleStubs
{
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
                            $"The stub sequence has been called more than expected; Make sure the {nameof(SequenceContainer<T>)} is setup correclty");
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
}