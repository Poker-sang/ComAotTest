using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace Common;

public static class StreamHelper
{
    public static IStream ToIStream(this Stream stream) => new NetToComStream(stream);

    public static Stream ToStream(this IStream iStream) => new ComToNetStream(iStream);

    public static unsafe STATSTG GetStat(this IStream ioStream)
    {
        var stat = new STATSTG();
        ioStream.Stat((nint)Unsafe.AsPointer(ref stat), 0);
        return stat;
    }
}
