# SimpleStubs tutorial

In this tutorial we will show how to install and use SimpleStubs.

## Installation

To install SimpleStubs, simply install the `Etg.SimpleStubs` NuGet package to your unit test project (also see *Tips and Tricks* section for installation tips).

Once installed, SimpleStubs will create stubs for all interfaces (`public` interfaces and optionally `internal` interfaces) in all referenced projects. 

The generated stubs will be added to the `Properties\SimpleStubs.generated.cs` file and compiled as part of the build process. Because the stubs are generated, modifying the stubs manually has no effect (if you really want to modify a stub, copy it to a different file).

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
// for any number of calls
var stub = new StubIPhoneBook().GetContactPhoneNumber((firstName, lastName) => 6041234567);
```

or

```csharp
// For one call; an exception will be thrown if more calls occur
var stub = new StubIPhoneBook().GetContactPhoneNumber((firstName, lastName) => 6041234567, Times.Once);
```

or 

```csharp
// For a specific number of calls
var stub = new StubIPhoneBook().GetContactPhoneNumber((firstName, lastName) => 6041234567, count:5);
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

## Stubbing indexers
```csharp
var stub = new StubIGenericContainer<int>();

// stubbing indexer getter
stub.Item_Get(index =>
{
    // we're expecting the code under test to get index 5
    if (index != 5) throw new IndexOutOfRangeException();
    return 99;
});

// stubbing indexer setter
int res = -1;
stub.Item_Set((index, value) =>
{
    // we're expecting the code under test to only set index 7
    if (index != 7) throw new IndexOutOfRangeException();
    res = value;
});
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
var stub = new StubIPhoneBook()
    .GetContactPhoneNumber((p1, p2) => 12345678, Times.Once) // first call
    .GetContactPhoneNumber((p1, p2) => 11122233, Times.Twice) // next two calls
    .GetContactPhoneNumber((p1, p2) => 22233556, Times.Forever); // rest of the calls
```

## Overwriting stubs

It's possible to overwrite a stubbed method or property as follows:
```csharp
var stub = new StubIPhoneBook().GetContactPhoneNumber((p1, p2) => 12345678);

// test code

// overwrite the stub
stub.GetContactPhoneNumber((p1, p2) => 11122233, overwrite:true);

// other test code
```

## Default behaviors

In the default ```MockBehavior.Strict``` mode, stubs that are called before their behaviors have been set will throw an exception. This can be changed—so that uninitialized stubs will return ```null``` or a default value—by using ```MockBehavior.Loose```:

```csharp
var stub = new StubIPhoneBook()
    .WithDefaultBehavior(MockBehavior.Loose);
IPhoneBook phoneBook = stub;
phoneBook.GetContactPhoneNumber("John", "Smith"); // Returns 0
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
    "StubInternalInterfaces": false,
    "StubCurrentProject": false,
    "DefaultMockBehavior": "Strict"
}
```

The configuration file allows you to instruct SimpleStubs to omit creating stubs for a given project or interface. 
**Note** that this very useful to exclude interfaces that are causing SimpleStubs to generate stubs that don't compile (this can happen in some edge cases). If you encounter such a case, exclude the interface in question and report the problem so we can fix it.

It's also possible to instruct SimpleStubs to create stubs for internal interfaces (by default only public interfaces are stubbed) as shown in the configuration sample above.

It's also possible to generate stubs for interfaces from the current project (not only referenced projects) as shown in the configuration sample above. This is useful if you'd like to generate stubs for interfaces that are defined in your test project or in a shared project.

Loose mode can be turned on for all stubs by changing the DefaultMockBehavior setting to "Loose". The default setting if omitted is "Strict".

## Tips and Tricks

### Reduce compile time by adding SimpleStubs to only one project in your solution

If you have a solution composed of multiple projects, instead of installing SimpleStubs to each of your test projects,
* Create a new project that will contain the stubs, call it something like *GeneratedStubs*
* Install SimpleStubs to the *GeneratedStubs* project.
* Reference all the projects in the solution from the *GeneratedStubs* project (or at least the projects that contain interfaces to be stubbed).
* Reference the *GeneratedStubs* project in your test projects to access the stubs.

With this approach, each stub is generated only once and only when there are code changes.

### Generate stubs for external interfaces

By default, SimpleStubs ignores external interfaces because there can be too many of them to generate stubs for. One simple way of generating a stub for an external interface is to create an internal interface that inherits from it (you don't need to use the internal fake interface anywhere in the code). Because SimpleStubs handles inheritance, the generated stub can be used as a stub of the original external interface.

## Current limitations
* Only interfaces are stubbed.
* No stubs will be generated for external libraries (dlls). See *Tips and Tricks* section for a workaround.

## What if some stubs don't compile?

Exclude the interface that is causing the problem (using the `SimpleStubs.json` configuration file) and report the problem by opening an issue.
