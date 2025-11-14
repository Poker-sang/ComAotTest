using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Common;

unsafe
{
    Console.WriteLine($"Program: {RuntimeInformation.FrameworkDescription}, {(RuntimeFeature.IsDynamicCodeCompiled ? "JIT" : "AOT")}");
    Console.WriteLine();
    const string path = @"..\..\..\..\..\ComLibrary\bin\x64\Release\net10.0\publish\win-x64\ComLibrary.dll";
    var dllHandle = NativeLibrary.Load(path);
    var dllGetAllObjectPtr = NativeLibrary.GetExport(dllHandle, nameof(DllGetAllObject));
    var dllGetAllObject = Marshal.GetDelegateForFunctionPointer<DllGetAllObject>(dllGetAllObjectPtr);
    var classFactoryGuid = typeof(IServer).GUID;
    var x = dllGetAllObject(classFactoryGuid, out var pppv, out var objectCount);
    if (x is not 0)
        return x;
    var wrappers = new StrategyBasedComWrappers();
    var a = pppv[0];
    var rcw = wrappers.GetOrCreateObjectForComInstance(a, CreateObjectFlags.UniqueInstance);
    Marshal.Release(a);
    Marshal.FreeHGlobal((nint)pppv);
    var server2 = (IServer2)rcw;

    Console.WriteLine(rcw is IServer2);
    Console.WriteLine(rcw is ServerBase);

    // 普通封送测试
    var arr = server2.GetServer(out var count);
    var server = arr[0];
    CultureInfo.CurrentUICulture = new("en-US");
    server.Culture(CultureInfo.CurrentUICulture.ToString());
    Console.WriteLine(server.ComputePi());
    Console.WriteLine(server.ComputeE(out var e));
    Console.WriteLine(e);
    Console.WriteLine(server.GetEnum());
    Console.WriteLine(server.StringNullTest() is null);
    Console.WriteLine(server.StringEmptyTest() is "");

    // 流测试
    using Stream ioStream1 = new MemoryStream();
    IStream iStream1 = new NetToComStream(ioStream1);
    var ccw = wrappers.GetOrCreateComInterfaceForObject(iStream1, CreateComInterfaceFlags.None);
    var iStream2 = (IStream)wrappers.GetOrCreateObjectForComInstance(ccw, CreateObjectFlags.UniqueInstance);
    using Stream ioStream2 = new ComToNetStream(iStream2);
    Marshal.Release(ccw);
    iStream1.Write("123"u8.ToArray(), 3, out _);
    _ = ioStream2.Seek(0, SeekOrigin.Begin);
    var buffer = new byte[3];
    _ = ioStream2.Read(buffer, 0, 3);
    Console.WriteLine(Encoding.UTF8.GetString(buffer));

    using Stream readonlyIoStream = new MemoryStream("123"u8.ToArray(), false);
    IStream readonlyIStream = new NetToComStream(readonlyIoStream);
    var ccw2 = wrappers.GetOrCreateComInterfaceForObject(readonlyIStream, CreateComInterfaceFlags.None);
    var readonlyIStream2 = (IStream)wrappers.GetOrCreateObjectForComInstance(ccw2, CreateObjectFlags.UniqueInstance);
    using Stream readonlyIoStream2 = new ComToNetStream(readonlyIStream2);
    Console.WriteLine(readonlyIoStream2.CanRead);
    Console.WriteLine(readonlyIoStream2.CanSeek);
    Console.WriteLine(readonlyIoStream2.CanWrite);
    Console.WriteLine(readonlyIoStream.CanRead);
    Console.WriteLine(readonlyIoStream.CanSeek);
    Console.WriteLine(readonlyIoStream.CanWrite);
    return 0;
}

public unsafe delegate int DllGetAllObject(in Guid iId, [Out] out nint* pppv, [Out] out int count);
