using System;

namespace Nefarius.ViGEm.Client;

using PVIGEM_TARGET = IntPtr;

/// <summary>
///     Provides a managed wrapper around a generic emulation target.
/// </summary>
internal abstract class ViGEmTarget : IDisposable, IViGEmTarget
{
    private bool _isConnected;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViGEmTarget" /> bound to a <see cref="ViGEmClient" />.
    /// </summary>
    /// <param name="client">The <see cref="ViGEmClient" /> this device is attached to.</param>
    protected ViGEmTarget(ViGEmClient client)
    {
        Client = client;
    }

    /// <summary>
    ///     Gets the <see cref="ViGEmClient" /> this <see cref="ViGEmTarget" /> is bound to.
    /// </summary>
    protected ViGEmClient Client { get; }

    protected PVIGEM_TARGET NativeHandle { get; init; }

    /// <inheritdoc />
    /// <summary>
    ///     Gets the Vendor ID this device will present to the system.
    /// </summary>
    public ushort VendorId { get; protected init; }

    /// <inheritdoc />
    /// <summary>
    ///     Gets the Product ID this device will present to the system.
    /// </summary>
    public ushort ProductId { get; protected init; }

    /// <summary>
    ///     Brings this device online by attaching it to the bus.
    /// </summary>
    public virtual void Connect()
    {
        if (_isConnected)
        {
            return;
        }

        if (VendorId > 0 && ProductId > 0)
        {
            ViGEmClient.vigem_target_set_vid(NativeHandle, VendorId);
            ViGEmClient.vigem_target_set_pid(NativeHandle, ProductId);
        }

        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_add(Client.NativeHandle, NativeHandle);
        ViGEmClient.CheckError(error);

        _isConnected = true;
    }

    /// <summary>
    ///     Takes this device offline by removing it from the bus.
    /// </summary>
    public virtual void Disconnect()
    {
        if (!_isConnected)
        {
            return;
        }

        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_remove(Client.NativeHandle, NativeHandle);
        ViGEmClient.CheckError(error);

        _isConnected = false;
    }

    private void ReleaseUnmanagedResources()
    {
        ViGEmClient.vigem_target_free(NativeHandle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ViGEmTarget()
    {
        ReleaseUnmanagedResources();
    }
}