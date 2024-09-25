using System.Runtime.InteropServices;

namespace ComLibrary;

[ComVisible(true)]
[Guid("3CF457CD-4383-4B36-8380-05C259F4E40F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IServer
{
    /// <summary>
    /// Compute the value of the constant Pi.
    /// </summary>
    double ComputePi();
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[Guid("07A2382E-7A22-4912-B2D7-85F7C4F9109D")]
public class Server : IServer
{
    public double ComputePi()
    {
        var sum = 0.0;
        var sign = 1;
        for (var i = 0; i < 1024; ++i)
        {
            sum += sign / (2.0 * i + 1.0);
            sign *= -1;
        }

        return 4.0 * sum;
    }
}
