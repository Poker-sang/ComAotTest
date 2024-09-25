using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static Win32NativeMethods;

Console.WriteLine("Hello, World!");

var dllHandle = LoadLibrary(@"C:\WorkSpace\ComAotTest\ComLibrary\bin\x64\Release\net8.0\ComTest.comhost.dll");
var dllGetClassObjectPtr = GetProcAddress(dllHandle, "DllGetClassObject");
var dllGetClassObject = Marshal.GetDelegateForFunctionPointer<DllGetClassObject>(dllGetClassObjectPtr);
var filterPersistGuid = new Guid("07A2382E-7A22-4912-B2D7-85F7C4F9109D");
var classFactoryGuid = typeof(IClassFactory).GUID;
var x = dllGetClassObject(filterPersistGuid, classFactoryGuid, out var unk);
if (x is not 0)
{
    Console.WriteLine($"{x:x}");
    Console.ReadKey();
    return x;
}
var wrappers = new StrategyBasedComWrappers();
var factoryObj = wrappers.GetOrCreateObjectForComInstance(unk, CreateObjectFlags.None);
var factory = (IClassFactory)factoryObj;
factory.CreateInstance(0, typeof(IServer).GUID, out var serverObj);
var server = (IServer)serverObj!;
var pi = server.ComputePi();

Console.WriteLine(pi);
Console.ReadKey();
return 0;

public static partial class Win32NativeMethods
{
    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint LoadLibrary(string libFilename);


    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint GetProcAddress(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
}

public delegate int DllGetClassObject(in Guid clsId, in Guid iId, [Out] out nint ppv);

[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
public partial interface IClassFactory
{
    void CreateInstance(nint outer, in Guid id, [MarshalAs(UnmanagedType.Interface)] out object? iFace);

    void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
}

[GeneratedComInterface]
[Guid("3CF457CD-4383-4B36-8380-05C259F4E40F")]
public partial interface IServer
{
    double ComputePi();
}

