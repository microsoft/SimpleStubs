# SimpleStubs tutorial

In this tutorial we will show how to install and use SimpleStubs.

## Installation

To install SimpleStubs, simply install the `Etg.SimpleStubs` NuGet package to your unit test project.

Once installed, SimpleStubs will create stubs for all interfaces (`public` interfaces and optionally `internal` interfaces) in all referenced projects. No stubs will be generated for external libraries (dlls) but that's a feature we're considering. 

The generated stubs will be added to the `Properties\SimpleStubs.generated.cs` file which will be regenerated every time the project is built (the stubs will be generated before the build and will be compiled as part of the build, so you don't need to worry about any of that). Because the stubs are regenerated every time, modifying the stubs manually has no effect (if you really want to modify a stub, copy it to a different file).

**Important note for UWP**: Because of a limitation in NuGet support for UWP (see discussion [here](https://github.com/NuGet/Home/wiki/Bringing-back-content-support,-September-24th,-2015)), the `Properties\SimpleStubs.generated.cs` file will not be automatically added to UWP projects and must be manually added (simply add the file to the `Properties` folder of your test project).

## Api

Let's consider the following interface and look at how we can use SimpleStubs to create stubs for it.

```csharp
public interface IPhoneBook
{
    long GetContactPhoneNumber(string firstName, string lastName);

    long MyNumber { get; set; }

    event EventHandler<long> PhoneNumberChanged;
}
```

### Stubbing methods
```csharp
var stub = new StubIPhoneBook().GetContactPhoneNumber((firstName, lastName) => 6041234567);
```

You can also copy and verify the parameters values:
```csharp
string firstName = null;
string lastName = null;
var stub = new StubIPhoneBook().GetContactPhoneNumber((fn, ln) =>
{
    firstName = fn;
    lastName = ln;
    return number;
});

ClassUnderTest obj = new ClassUnderTest(stub);

Assert.AreEqual(expectedValue, obj.Foo());
// parameters verification
Assert.AreEqual("John", firstName);
Assert.AreEqual("Smith", lastName);
```

### Out parameters
```csharp
object someObj = new Foo();
var stub = new StubIContainer()
    .GetElement((int index, out object value) =>
    {
        value = someObj;
        return true;
    });
```

### Ref parameters
```csharp
var stub = new StubIRefUtils()
    .Swap<int>((ref int v1, ref int v2) =>
    {
        int temp = v1;
        v1 = v2;
        v2 = temp;
    });
```

### Generic methods
```csharp
int value = -1;
var stub = new StubIContainer()
    .GetElement<int>(index => value)
    .SetElement<int>((i, v) => { value = v; });
```

## Stubbing properties

```csharp
long myNumber = 6041234567;
var stub = new StubIPhoneBook()
    .MyNumber_Get(() => myNumber)
    .MyNumber_Set(value => newNumber = value);
```

## Stubbing events

```csharp
var stub = new StubIPhoneBook();

// Pass the stub to the code under test
var obj = new ClassUnderTest(stub);

// Raise the event 
stub.PhoneNumberChanged_Raise(stub, 55);

// Verify the state of obj to ensure that it has reacted to the event
```

## Stubbing a sequence of calls

In some cases, it might be useful to have a stub behave differently when it's called several times. SimpleStubs offers supports for stubbing a sequence of calls.

```csharp
// Define the sequence first
var sequence = StubsUtils.Sequence<Func<string, string, int>>()
    .Once((p1, p2) => { throw new Exception(); }) // first call will throw an exception
    .Repeat((p1, p2) => 11122233, 3) // next three calls will return 11122233
    .Forever((p1, p2) => 22233556); // any subsequent call will return 22233556
    
var stub = new StubIPhoneBook().GetContactPhoneNumber((p1, p2) => sequence.Next(p1, p2));

// you can also verify how many times the sequence was called
Assert.AreEqual(5, sequence.CallCount);
```

## Configuration

SimpleStubs also supports an optional configuration file that can be added to the root of your test project. The configuration file (named `SimpleStubs.json`) has the following structure:

```json
{
    "IgnoredProjects": [
        "IgnoredProject1",
        "IgnoredProject2"
    ],
    "IgnoredInterfaces": [
        "MyNamespace.IFooInterface",
        "MyNamespace.IBarInterface"
    ],
    "StubInternalInterfaces": false    
}
```

The configuration file allows you to instruct SimpleStubs to omit creating stubs for a given project or interface. 
**Note** that this very useful to exclude interfaces that are causing SimpleStubs to generate stubs that don't compile (this can happen in some edge cases). If you encounter such a case, exclude the interface in question and report the problem so we can fix it.

It's also possible to instruct SimpleStubs to create stubs for internal interfaces (by default only public interfaces are stubbed) as shown in the configuration sample above.

## Current limitations
* Only interfaces are stubbed.
* Generic constrains are not supported.

## What if some stubs don't compile?

Exclude the interface that is causing the problem (using the `SimpleStubs.json` configuration file) and report the problem by opening an issue.
