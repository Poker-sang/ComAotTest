using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Common;

namespace ComLibrary;

public static class DllMain
{
    internal static StrategyBasedComWrappers ComWrappers { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetAllObject))]
    private static unsafe int DllGetAllObject(Guid* riid, void*** pppv, int* count)
    {
        var server = new Server();
        var ccwUnknown = (void*)ComWrappers.GetOrCreateComInterfaceForObject(server, CreateComInterfaceFlags.None);
        *count = 1;
        var x = (void**)Marshal.AllocHGlobal(sizeof(nint));
        *pppv = x;
        (*pppv)[0] = ccwUnknown;
        return 0;
    }
}

[ComVisible(true)]
[Guid("07A2382E-7A22-4912-B2D7-85F7C4F9109D")]
[GeneratedComClass]
public partial class Server : ServerBase
{
    /// <inheritdoc />
    public override double ComputePi()
    {
        var sum = 0.0;
        var sign = 1;
        for (var i = 0; i < 1024; ++i)
        {
            sum += sign / (2.0 * i + 1.0);
            sign *= -1;
        }

        Console.WriteLine("COM: " + RuntimeInformation.FrameworkDescription);

        return 4.0 * sum;
    }

    private MemoryStream _stream = new();

    /// <inheritdoc />
    public override double ComputeEProperty
    {
        get
        {
            var sum = 1.0;
            var fact = 1;
            for (var i = 1; i < 10; ++i)
            {
                fact *= i;
                sum += 1.0 / fact;
            }
            _stream.WriteByte(0);
            return sum;
        }
    }

    /// <inheritdoc />
    public override TestEnum GetEnum() => TestEnum.UniqueInstance;

    /// <inheritdoc />
    public override void Culture(string culture)
    {
        Console.WriteLine("in culture: " + culture);
        Console.WriteLine("com culture: " + CultureInfo.CurrentUICulture);
    }

    /// <inheritdoc />
    public override string StringEmptyTest() => "";

    /// <inheritdoc />
    public override string? StringNullTest() => null;

    /// <inheritdoc />
    public override int GetCount() => 1;

    /// <inheritdoc />
    public override IServer[] GetServer(int num)
    {
        return num is 1 ? [this] : [];
    }
}
