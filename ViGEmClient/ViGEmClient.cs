using System;
using System.Diagnostics.CodeAnalysis;

using Nefarius.ViGEm.Client.Exceptions;

namespace Nefarius.ViGEm.Client;

using PVIGEM_CLIENT = IntPtr;

/// <summary>
///     Represents a managed gateway to a compatible emulation bus.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed partial class ViGEmClient : IDisposable
{
    public ViGEmClient()
    {
        NativeHandle = vigem_alloc();
        if (NativeHandle == IntPtr.Zero)
        {
            throw new VigemAllocFailedException();
        }

        VIGEM_ERROR error = vigem_connect(NativeHandle);
        CheckError(error);
    }

    /// <summary>
    ///     Gets the <see cref="PVIGEM_CLIENT" /> identifying the bus connection.
    /// </summary>
    internal PVIGEM_CLIENT NativeHandle { get; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    
    private void ReleaseUnmanagedResources()
    {
        vigem_disconnect(NativeHandle);
        vigem_free(NativeHandle);
    }

    ~ViGEmClient()
    {
        ReleaseUnmanagedResources();
    }
}