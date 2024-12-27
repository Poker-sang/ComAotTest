using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System;

namespace Common;

[ComVisible(true)]
[Guid("3FE29B79-550D-48A3-800F-F884145FA514")]
[GeneratedComInterface]
public partial interface IServer2
{
    /// <summary>
    /// 获取数组和COM复杂类型示例
    /// </summary>
    [return: MarshalUsing(CountElementName = nameof(num))]
    IServer[] GetServer(int num);

    /// <summary>
    /// 获取数组长度
    /// </summary>
#pragma warning disable SYSLIB1092
    int GetCount();
#pragma warning restore SYSLIB1092
}

[ComVisible(true)]
[Guid("3CF457CD-4383-4B36-8380-05C259F4E40F")]
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
public partial interface IServer
{
    /// <summary>
    /// 方法示例 Compute the value of the constant Pi.
    /// </summary>
    double ComputePi();

    /// <summary>
    /// 布尔类型和out参数和属性示例（COM接口中不能用属性）
    /// </summary>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool ComputeE(out double e);

    /// <summary>
    /// 枚举示例
    /// </summary>
#pragma warning disable SYSLIB1092
    TestEnum GetEnum();
#pragma warning restore SYSLIB1092

    /// <summary>
    /// 字符串和全局变量示例
    /// </summary>
    void Culture(string culture);

    /// <summary>
    /// 空字符串示例
    /// </summary>
    string StringEmptyTest();

    /// <summary>
    /// 字符串null示例
    /// </summary>
    string? StringNullTest();
}

[ComVisible(true)]
[Guid("2AEA338C-F19F-474C-9D21-787DC577412F")]
[GeneratedComClass]
public abstract partial class ServerBase : IServer, IServer2
{
    /// <inheritdoc />
    public abstract double ComputePi();

    /// <inheritdoc />
    public bool ComputeE(out double e)
    {
        Console.WriteLine($"{nameof(ComputeE)} from {nameof(ServerBase)}");
        e = ComputeEProperty;
        return true;
    }

    /// <inheritdoc cref="ComputeE" />
    public abstract double ComputeEProperty { get; }

    /// <inheritdoc />
    public abstract TestEnum GetEnum();

    /// <inheritdoc />
    public abstract void Culture(string culture);

    /// <inheritdoc />
    public abstract string StringEmptyTest();

    /// <inheritdoc />
    public abstract string? StringNullTest();

    /// <inheritdoc />
    public abstract int GetCount();

    /// <inheritdoc />
    public abstract IServer[] GetServer(int num);
}

public enum TestEnum
{
    None = 0,
    UniqueInstance = 1,
}
