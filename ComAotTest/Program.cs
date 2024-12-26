using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static Win32NativeMethods;

public class Program
{
    public static int Main()
    {
        unsafe
        {
            Console.WriteLine($"Program: {RuntimeInformation.FrameworkDescription}, {(RuntimeFeature.IsDynamicCodeCompiled ? "normal" : "AOT")}");
            const string path = @"C:\WorkSpace\ComAotTest\ComLibrary\bin\x64\Release\net8.0\publish\win-x64\ComLibrary.dll";
            var dllHandle = LoadLibrary(path);
            var dllGetClassObjectPtr = GetProcAddress(dllHandle, nameof(DllGetAllObject));
            var dllGetClassObject = Marshal.GetDelegateForFunctionPointer<DllGetAllObject>(dllGetClassObjectPtr);
            var classFactoryGuid = typeof(IServer).GUID;
            var x = dllGetClassObject(classFactoryGuid, out var pppv, out var count);
            if (x is not 0)
                return x;
            var wrappers = new StrategyBasedComWrappers();
            var a = pppv[0];
            var rcw = wrappers.GetOrCreateObjectForComInstance(a, CreateObjectFlags.UniqueInstance);
            Marshal.Release(a);
            Marshal.FreeHGlobal((nint)pppv);
            var server = (IServer)rcw;
            var pi = server.ComputePi();
            Console.WriteLine(pi);
            Console.ReadKey();
            return 0;
        }
    }
}


public static partial class Win32NativeMethods
{
    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint LoadLibrary(string libFilename);


    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint GetProcAddress(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
}

public unsafe delegate int DllGetAllObject(in Guid iId, [Out] out nint* pppv, [Out] out int count);

[GeneratedComInterface]
[Guid("3CF457CD-4383-4B36-8380-05C259F4E40F")]
public partial interface IServer
{
    double ComputePi();
}
