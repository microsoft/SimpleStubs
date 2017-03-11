using System;
using System.Collections.Generic;
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

        Task<long> GetContactPhoneNumberAsync(string firstName, string lastName);

        long MyNumber { get; set; }

        int ContactsCount { get; }

        event EventHandler<long> PhoneNumberChanged;
    }

    // just to make sure all inherited members are stubbed
    public interface IPhoneBookSpecial : IPhoneBook
    {
    }

    public interface IContainer
    {
        T GetElement<T>(int index);

        Task<T> GetElementAsync<T>(int index);

        void SetElement<T>(int index, T value);

        Task SetElementAsync<T>(int index, T value);

        bool GetElement(int index, out object value);
    }

    public interface IRefUtils
    {
        void Swap<T>(ref T v1, ref T v2);
    }

    public interface ITestInterface : IDisposable
    {
        void DoSomething();

        void DoSomething(string s, int x);

        void DoSomething(List<IEnumerable<string>> list);

        void DoSomething(string[] array);

        Task DoSomethingAsync();

        Task<List<int>> DoSomethingAsync(int parameter);

        void SetDictionary(Dictionary<string, string> dict);

        Task SetDictionaryAsync(Dictionary<string, string> dict);

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

    public interface IGenericInterface<T, A> where T : class, IDisposable, new() where A : struct
    {
        T GetX();
    }

    public interface IGenericContainer<T>
    {
        T this[int index] { get; set; }

        T this[string key, int n] { get; }
    }

    // just to make sure inherited indexers are stubbed
    public interface IGenericContainerSubInterface : IGenericContainer<int>
    {
    }

    public interface IInterfaceWithGenericMethod
    {
        T GetFoo<T>() where T : class;

        T GetBar<T>() where T : struct;

        void SetBoo<T, A>(T t, A a) where T : class, IDisposable, new() where A : new();
    }

    public delegate void CustomDelegateBasedHandler(int arg1, string arg2, object arg3);

    public interface ICustomDelegateBasedEventExample
    {
        event CustomDelegateBasedHandler CustomDelegateEventOccurred;
    }
}