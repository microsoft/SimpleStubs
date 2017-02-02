# Developer Documentation

This document is targeted for developers who would like to understand and potentially contribute to the SimpleStubs source code.

## Build & Code Generation
SimpleStubs used Roslyn to generate stubs. The code is generated during the build (right before the build actually). 

When SimpleStubs NuGet package is installed to a given project, the [Etg.SimpleStubs.targets](https://github.com/Microsoft/SimpleStubs/blob/master/Targets/Etg.SimpleStubs.targets) file is installed and imported in the `ProjectName.nuget.targets` file (which gets added to the target project). The [Etg.SimpleStubs.targets](https://github.com/Microsoft/SimpleStubs/blob/master/Targets/Etg.SimpleStubs.targets) file contains a `BeforeBuild` task that executes the [GenerateStubsTask](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/Tasks/GenerateStubsTask.cs) visual studio Task and writes the output to the `"$(ProjectDir)Properties\SimpleStubs.generated.cs"` file. This Task calls the [SimpleStubsGenerator](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/CodeGen/SimpleStubsGenerator.cs) which walks through all dependent projects and generate corresponding stubs.

## Method and Property Stubbers

If you follow the source code starting at [SimpleStubsGenerator](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/CodeGen/SimpleStubsGenerator.cs), you'll notice that the [InterfaceStubber](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/CodeGen/InterfaceStubber.cs) class is used to generate a stub for a given interface. The constructor of [InterfaceStubber](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/CodeGen/InterfaceStubber.cs) is as follows:

```csharp
        public InterfaceStubber(IEnumerable<IMethodStubber> methodStubbers, IEnumerable<IPropertyStubber> propertyStubbers)
        {
            _propertyStubbers = propertyStubbers;
            _methodStubbers = new List<IMethodStubber>(methodStubbers);
        }
```

When an interface is stubbed, the `InterfaceStubber` walks through all the method/property stubbers and execute them all on each method/property in the interface. This means that you can easily add your own implementation of `IMethodStubber`/`IPropertyStubber` and it will be invoked as part of the build. To inject your own `IMethodStubber`/`IPropertyStubber`, simply update the [DIModule](https://github.com/Microsoft/SimpleStubs/blob/master/src/SimpleStubs.CodeGen/DI/DIModule.cs) code and add an additional entry where the constructor of `InterfaceStubber` is being invoked. You can also modify existing implementations of `IMethodStubber`/`IPropertyStubber` to fix bugs or add behaviour.

## Testing

The [test](https://github.com/Microsoft/SimpleStubs/tree/master/test) folder contains a class library project and a corresponding test project. The test project is setup in a way that it will invoke SimpleStubs with the current version of the `Etg.SimpleStubs.CodeGen.dll` that is available in the build directory (see [Etg.SimpleStubs.targets](https://github.com/Microsoft/SimpleStubs/blob/master/test/TestClassLibraryTest/Etg.SimpleStubs.targets)). This will allow developers to modify the SimpleStubs code and run the unit tests without having to re-generate the NuGet every time. One issue I noticed with this approach is that Visual Studio ends up locking the `Etg.SimpleStubs.CodeGen.dll` and it won't be updated during the build. When that happens, the only solution I am currently aware of is to restart visual studio.

If you want to avoid dealing with dlls being locked or to debug SimpleStubs code generation, there is a test that has been added specifically for these purposes (see in `TestGenerateStubs` the [test class](https://github.com/Microsoft/SimpleStubs/blob/master/test/TestClassLibraryTest/StubGeneratorTest.cs)). Enable this test when needed (by removing the `[Ignore]` attribute) and it'll generate the stubs and write them to the same `SimpleStubs.generated.cs` file.


