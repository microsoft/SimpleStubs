using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TestClassLibrary
{
    public interface IAppConfigPeriodicRefreshHandler : IDisposable
    {
        void Start();
    }

    public interface IPhoneBook
    {
        long GetContactPhoneNumber(string firstName, string lastName);

        long MyNumber { get; set; }

        int ContactsCount { get; }

        event EventHandler<long> PhoneNumberChanged;
    }

    public interface IContainer
    {
        T GetElement<T>(int index);
        void SetElement<T>(int index, T value);

        bool GetElement(int index, out object value);
    }

    public interface IRefUtils
    {
        void Swap<T>(ref T v1, ref T v2);
    }

    public interface ITestInterface : IDisposable
    {
        void DoSomething(string s, int x);

        void DoSomething(List<IEnumerable<string>> list);

        void DoSomething(string[] array);

        Task<List<int>> DoSomething(int parameter);

        void SetDictionary(Dictionary<string, string> dict);

        string Prop1 { get; }

        string Prop2 { set; }

        string Prop3 { get; set; }

        event EventHandler<EventArgs> Changed;

        event EventHandler OtherEvent;

        List<T> GetGenericList<T>();

        void SetGenericValue<T>(T value);
    }

    public interface IIgnoredInterface : IDisposable
    {
    }

    internal interface IInternalInterface
    {
        void DoSomethingInternal();
    }

    public interface IGenericInterface<T>
    {
        T GetX();
    }

    public interface IInterfaceWithGenericMethod
    {
        T GetFoo<T>();
    }

    public class Stub : IInterfaceWithGenericMethod
    {
        private readonly Dictionary<string, object> _stubs = new Dictionary<string, object>();

        public delegate T GetFooOfT_Delegate<T>();

        public T GetFoo<T>()
        {
            return ((GetFooOfT_Delegate<T>) _stubs[nameof(GetFooOfT_Delegate<T>)]).Invoke();
        }

        public Stub SetupGetFooOfT<T>(GetFooOfT_Delegate<T> del)
        {
            _stubs[nameof(GetFooOfT_Delegate<T>)] = del;
            return this;
        }
    }
}