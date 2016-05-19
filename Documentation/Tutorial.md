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
var stub = new StubIPhoneBook
{
    GetContactPhoneNumber_String_String = (fn, ln) =>
    {
        return 6041234567;
    }
};
```

You can also copy and verify the parameters values:
```csharp
string firstName = null;
string lastName = null;
var stub = new StubIPhoneBook
{
    GetContactPhoneNumber_String_String = (fn, ln) =>
    {
        firstName = fn;
        lastName = ln;
        return 6041234567;
    }
};

ClassUnderTest obj = new ClassUnderTest(stub);

Assert.AreEqual(expectedValue, obj.Foo());
// parameters verification
Assert.AreEqual("John", firstName);
Assert.AreEqual("Smith", lastName);
```

## Stubbing properties

```csharp
long myNumber = 6041234567;
var stub = new StubIPhoneBook
{
    MyNumber_Get = () => myNumber,
    MyNumber_Set = num =>
    {
        myNumber = num;
    }
};
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
    
var stub = new StubIPhoneBook 
{ 
    // Get the next element from the sequence every time the method is called and invoke it
    GetContactPhoneNumber_String_String = (p1, p2) => sequence.Next(p1, p2) 
};

// you can also verify how many times the sequence was called
Assert.AreEqual(5, sequence.CallCount);
```


## Full Example

Let's look at how we can unit test the following class (`LocationManager`) using SimpleStubs.

```csharp
using System.Threading.Tasks;

namespace HelloApp
{
    public class LocationManager
    {
        private readonly ILocationService _locationService;
        public LocationManager(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <returns>Current Location or null if the location could not be retrieved</returns>
        public async Task<Location> GetCurrentLocation()
        {
            try
            {
                string location = await _locationService.GetLocation();
                var ss = location.Split('/');
                return new Location(ss[0], ss[1]);
            }
            catch (LocationServiceUnavailableException)
            {
                return null;
            }
        }

        /// <returns>The current country code (e.g. US, CA) or null if the country code could not be retrieved</returns>
        public async Task<string> GetCurrentCountryCode()
        {
            try
            {
                Location location = await GetCurrentLocation();
                string loc = $"{location.Country}/{location.City}";
                return await _locationService.GetCountryCode(loc);
            }
            catch (LocationServiceUnavailableException)
            {
                return null;
            }
        }
    }
}
```

The `ILocationService` interface is as follows:

```csharp
using System;
using System.Threading.Tasks;

namespace HelloApp
{
    public interface ILocationService
    {
        /// <returns>
        /// the location in the format Country/City
        /// </returns>
        /// <exception cref="LocationServiceUnavailableException"></exception>
        Task<string> GetLocation();

        /// <returns>the country code of the given location</returns>
        /// <exception cref="LocationServiceUnavailableException"></exception>
        Task<string> GetCountryCode(string location);
    }
}
```

SimpleStubs will automatically generate a stub for `ILocationService` called StubILocationService. The following tests show how the stub can be used to unit test the `LocationManager` class:

```csharp
    [TestMethod]
    public async Task TestGetCurrentLocation()
    {
        StubILocationService locationServiceStub = new StubILocationService
        {
            GetLocation = () => Task.FromResult("Canada/Vancouver")
        };

        LocationManager locationManager = new LocationManager(locationServiceStub);
        Location location = await locationManager.GetCurrentLocation();

        Assert.AreEqual("Canada", location.Country);
        Assert.AreEqual("Vancouver", location.City);
        Assert.AreEqual(1, locationServiceStub.ILocationService_GetLocation_CallCount);
    }

    [TestMethod]
    public async Task TestThatGetCurrentLocationReturnsNullIfLocationServiceIsUnavailable()
    {
        StubILocationService locationServiceStub = new StubILocationService
        {
            GetLocation = () =>
            {
                throw new LocationServiceUnavailableException();
            }
        };

        LocationManager locationManager = new LocationManager(locationServiceStub);
        Assert.IsNull(await locationManager.GetCurrentLocation());
        Assert.AreEqual(1, locationServiceStub.ILocationService_GetLocation_CallCount);
    }

    [TestMethod]
    public async Task TestGetCurrentCountryCode()
    {
        StubILocationService locationServiceStub = new StubILocationService
        {
            GetLocation = () => Task.FromResult("Canada/Vancouver"),
            GetCountryCode_String = location => Task.FromResult("CA")
        };

        LocationManager locationManager = new LocationManager(locationServiceStub);
        Assert.AreEqual("CA", await locationManager.GetCurrentCountryCode());
        Assert.AreEqual(1, locationServiceStub.ILocationService_GetCountryCode_String_CallCount);
    }
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
* Methods signatures with pointers are not supported.
* Generic methods are not supported (but generic interfaces are).
* Only interfaces are stubbed.

## What if some stubs don't compile?

Exclude the interface that is causing the problem (using the `SimpleStubs.json` configuration file) and report the problem.

