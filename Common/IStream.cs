#region Copyright

// GPL v3 License
// 
// ComAotTest/Common
// Copyright (c) 2024 Common/IStream.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;

namespace Common;

[GeneratedComInterface]
[Guid("0000000c-0000-0000-C000-000000000046")]
public partial interface IStream
{
    void Seek(long offset, SeekOrigin origin, out long newPosition);

    void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, uint bufferSize, out long bytesRead);

    void Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int bufferSize, out long bytesWritten);

    void SetSize(long libNewSize);

    void CopyTo(IStream targetStream, long bufferSize, out long buffer, out int bytesWritten);

    void Commit(uint flags);

    void Revert();

    void LockRegion(long offset, long byteCount, uint lockType);

    void UnlockRegion(long offset, long byteCount, uint lockType);

    void Stat(nint pstatstg, int grfStatFlag);

    void Clone(out IStream ppstm);
}

public class ComToNetStream : Stream
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ComToNetStream(IStream iStream)
    {
        ArgumentNullException.ThrowIfNull(iStream);
        _iStream = iStream;
    }

    public override void Flush() => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        _iStream.Read(buffer, (uint)count, out var bytesRead);
        return (int)bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _iStream.Seek(offset, origin, out var newPosition);
        return newPosition;
    }

    public override void SetLength(long value) => _iStream.SetSize(value);

    public override void Write(byte[] buffer, int offset, int count) => _iStream.Write(buffer, count, out _);

    public override bool CanSeek => true;

    public override bool CanRead => (NetToComStream.GetStat(_iStream).grfMode & 2) is not (int)NetToComStream.STGM_ACCESS.STGM_WRITE;

    public override bool CanWrite => (NetToComStream.GetStat(_iStream).grfMode & 2) is not (int)NetToComStream.STGM_ACCESS.STGM_READ;

    public override long Length => NetToComStream.GetStat(_iStream).cbSize;

    public override long Position
    {
        get => Seek(0, SeekOrigin.Current);
        set => Seek(value, SeekOrigin.Begin);
    }

    private readonly IStream _iStream;
}

/// <summary>
/// The class NetToComStream is not COM-visible. Its purpose is to be able to invoke COM interfaces
/// from managed code rather than the contrary.
/// </summary>
[GeneratedComClass]
[Guid("49BE742F-D551-48A6-A32F-6A05E85EB2CD")]
public partial class NetToComStream : IStream
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NetToComStream(Stream ioStream)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        _ioStream = ioStream;
    }

    /// <summary>
    /// Move the stream pointer to the specified position.
    /// </summary>
    /// <remarks>
    /// System.IO.stream supports searching past the end of the stream, like
    /// OLE streams.
    /// newPositionPtr is not an out parameter because the method is required
    /// to accept NULL pointers.
    /// </remarks>
    void IStream.Seek(long offset, SeekOrigin origin, out long newPosition)
    {
        // The operation will generally be I/O bound, so there is no point in
        // eliminating the following switch by playing on the fact that
        // System.IO uses the same integer values as IStream for SeekOrigin.
        // Dereference newPositionPtr and assign to the pointed location.
        newPosition = _ioStream.Seek(offset, origin);
    }

    /// <summary>
    /// Read at most bufferSize bytes into buffer and return the effective
    /// number of bytes read in bytesReadPtr (unless null).
    /// </summary>
    /// <remarks>
    /// mscorlib disassembly shows the following MarshalAs parameters
    /// void Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] pv, int cb, nint pcbRead);
    /// This means marshaling code will have found the size of the array buffer in the parameter bufferSize.
    /// </remarks>
    void IStream.Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, uint bufferSize, out long bytesRead)
    {
        bytesRead = _ioStream.Read(buffer, 0, (int)bufferSize);
    }

    /// <summary>
    /// Sets stream's size.
    /// </summary>
    void IStream.SetSize(long libNewSize) => _ioStream.SetLength(libNewSize);

    /// <summary>
    /// Write at most bufferSize bytes from buffer.
    /// </summary>
    void IStream.Write([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, int bufferSize, out long bytesWritten)
    {
        _ioStream.Write(buffer, 0, bufferSize);
        // If fewer than bufferSize bytes had been written, an exception would
        // have been thrown, so it can be assumed we wrote bufferSize bytes.
        bytesWritten = bufferSize;
    }

    unsafe void IStream.Stat(nint pstatstg, int grfStatFlag)
    {
        ref var streamStats = ref Unsafe.AsRef<STATSTG>((void*)pstatstg);
        streamStats.type = (int)STGTY.STGTY_STREAM;
        streamStats.cbSize = _ioStream.Length;

        if (_ioStream is { CanRead: true, CanWrite: true })
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_READWRITE;
        else if (_ioStream.CanRead)
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_READ;
        else if (_ioStream.CanWrite)
            streamStats.grfMode |= (int)STGM_ACCESS.STGM_WRITE;
        else
        {
            // A stream that is neither readable nor writable is a closed stream.
            // Note the use of an exception that is known to the interop marshaller
            // (unlike ObjectDisposedException).
            throw new IOException();
        }
    }

    public static unsafe STATSTG GetStat(IStream ioStream)
    {
        var stat = new STATSTG();
        ioStream.Stat((nint)Unsafe.AsPointer(ref stat), 0);
        return stat;
    }

    [Flags]
    public enum STGM_ACCESS
    {
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,

        STGM_SHARE_DENY_NONE = 0x00000040,
        STGM_SHARE_DENY_READ = 0x00000030,
        STGM_SHARE_DENY_WRITE = 0x00000020,
        STGM_SHARE_EXCLUSIVE = 0x00000010,
        STGM_PRIORITY = 0x00040000,

        STGM_CREATE = 0x00001000,
        STGM_CONVERT = 0x00020000,
        STGM_FAILIFTHERE = 0x00000000,

        STGM_DIRECT = 0x00000000,
        STGM_TRANSACTED = 0x00010000,

        STGM_NOSCRATCH = 0x00100000,
        STGM_NOSNAPSHOT = 0x00200000,

        STGM_SIMPLE = 0x08000000,
        STGM_DIRECT_SWMR = 0x00400000,

        STGM_DELETEONRELEASE = 0x04000000
    }

    public enum STGTY
    {
        STGTY_STORAGE = 1,
        STGTY_STREAM = 2,
        STGTY_LOCKBYTES = 3,
        STGTY_PROPERTY = 4
    }

    #region Unimplemented methods

    /// <summary>
    /// Create a clone.
    /// </summary>
    /// <remarks>
    /// Not implemented.
    /// </remarks>
    void IStream.Clone(out IStream streamCopy)
    {
        streamCopy = null!;
        throw new NotSupportedException();
    }

    /// <summary>
    /// Read at most bufferSize bytes from the receiver and write them to targetStream.
    /// </summary>
    /// <remarks>
    /// Not implemented.
    /// </remarks>
    void IStream.CopyTo(IStream targetStream, long bufferSize, out long buffer, out int bytesWritten)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Commit changes.
    /// </summary>
    /// <remarks>
    /// Only relevant to transacted streams.
    /// </remarks>
    void IStream.Commit(uint flags)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Lock at most byteCount bytes starting at offset.
    /// </summary>
    /// <remarks>
    /// Not supported by System.IO.Stream.
    /// </remarks>
    void IStream.LockRegion(long offset, long byteCount, uint lockType)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Undo writes performed since last Commit.
    /// </summary>
    /// <remarks>
    /// Relevant only to transacted streams.
    /// </remarks>
    void IStream.Revert()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Unlock the specified region.
    /// </summary>
    /// <remarks>
    /// Not supported by System.IO.Stream.
    /// </remarks>
    void IStream.UnlockRegion(long offset, long byteCount, uint lockType)
    {
        throw new NotSupportedException();
    }

    #endregion Unimplemented methods

    private readonly Stream _ioStream;
}
