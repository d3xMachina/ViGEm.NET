﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Nefarius.ViGEm.Client.Exceptions;

namespace Nefarius.ViGEm.Client;

using PVIGEM_TARGET = IntPtr;

/// <summary>
///     Provides a managed wrapper around a generic emulation target.
/// </summary>
internal abstract class ViGEmTarget : IDisposable, IViGEmTarget
{
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
        if (VendorId > 0 && ProductId > 0)
        {
            ViGEmClient.vigem_target_set_vid(NativeHandle, VendorId);
            ViGEmClient.vigem_target_set_pid(NativeHandle, ProductId);
        }

        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_add(Client.NativeHandle, NativeHandle);

        switch (error)
        {
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_NONE:
                break;
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_NOT_FOUND:
                throw new VigemBusNotFoundException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_TARGET_UNINITIALIZED:
                throw new VigemTargetUninitializedException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_ALREADY_CONNECTED:
                throw new VigemAlreadyConnectedException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_NO_FREE_SLOT:
                throw new VigemNoFreeSlotException();
            default:
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    ///     Takes this device offline by removing it from the bus.
    /// </summary>
    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    public virtual void Disconnect()
    {
        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_remove(Client.NativeHandle, NativeHandle);

        switch (error)
        {
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_NONE:
                break;
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_NOT_FOUND:
                throw new VigemBusNotFoundException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_TARGET_UNINITIALIZED:
                throw new VigemTargetUninitializedException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_TARGET_NOT_PLUGGED_IN:
                throw new VigemTargetNotPluggedInException();
            case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_REMOVAL_FAILED:
                throw new VigemRemovalFailedException();
            default:
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    #region IDisposable Support

    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                try
                {
                    Disconnect();
                }
                catch
                {
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            ViGEmClient.vigem_target_free(NativeHandle);

            _disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~ViGEmTarget()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion
}