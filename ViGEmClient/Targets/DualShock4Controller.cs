﻿using System;
using System.Collections.Generic;

using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Utilities;

namespace Nefarius.ViGEm.Client.Targets;

/// <inheritdoc cref="ViGEmTarget" />
/// <summary>
///     Represents an emulated wired Sony DualShock 4 Controller.
/// </summary>
internal partial class DualShock4Controller : ViGEmTarget, IDualShock4Controller
{
    private static readonly List<DualShock4Button> ButtonMap = new()
    {
        DualShock4Button.ThumbRight,
        DualShock4Button.ThumbLeft,
        DualShock4Button.Options,
        DualShock4Button.Share,
        DualShock4Button.TriggerRight,
        DualShock4Button.TriggerLeft,
        DualShock4Button.ShoulderRight,
        DualShock4Button.ShoulderLeft,
        DualShock4Button.Triangle,
        DualShock4Button.Circle,
        DualShock4Button.Cross,
        DualShock4Button.Square,
        DualShock4SpecialButton.Ps,
        DualShock4SpecialButton.Touchpad
    };

    private static readonly List<DualShock4Axis> AxisMap = new()
    {
        DualShock4Axis.LeftThumbX, DualShock4Axis.LeftThumbY, DualShock4Axis.RightThumbX, DualShock4Axis.RightThumbY
    };

    private static readonly List<DualShock4Slider> SliderMap = new()
    {
        DualShock4Slider.LeftTrigger, DualShock4Slider.RightTrigger
    };

    private ViGEmClient.DS4_REPORT _nativeReport;

    private ViGEmClient.DS4_REPORT_EX _nativeReportEx;

    private ViGEmClient.PVIGEM_DS4_NOTIFICATION _notificationCallback;

    private ViGEmClient.DS4_AWAIT_OUTPUT_BUFFER _outputBuffer;

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Nefarius.ViGEm.Client.Targets.DualShock4Controller" /> class bound
    ///     to a <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" />.
    /// </summary>
    /// <param name="client">The <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> this device is attached to.</param>
    public DualShock4Controller(ViGEmClient client) : base(client)
    {
        NativeHandle = ViGEmClient.vigem_target_ds4_alloc();
        if (NativeHandle == IntPtr.Zero)
        {
            throw new VigemAllocFailedException();
        }

        ResetReport();
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Nefarius.ViGEm.Client.Targets.DualShock4Controller" /> class bound
    ///     to a <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> overriding the default Vendor and Product IDs with the
    ///     provided values.
    /// </summary>
    /// <param name="client">The <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> this device is attached to.</param>
    /// <param name="vendorId">The Vendor ID to use.</param>
    /// <param name="productId">The Product ID to use.</param>
    public DualShock4Controller(ViGEmClient client, ushort vendorId, ushort productId) : this(client)
    {
        VendorId = vendorId;
        ProductId = productId;
    }

    public override void Connect()
    {
        base.Connect();

        //
        // Callback to event
        // 
        _notificationCallback = (client, target, motor, smallMotor, color, userData) => FeedbackReceived?.Invoke(this,
            new DualShock4FeedbackReceivedEventArgs(motor, smallMotor,
                new LightbarColor(color.Red, color.Green, color.Blue)));

        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_ds4_register_notification(Client.NativeHandle,
            NativeHandle,
            _notificationCallback);

        ViGEmClient.CheckError(error);
    }

    public override void Disconnect()
    {
        ViGEmClient.vigem_target_ds4_unregister_notification(NativeHandle);

        base.Disconnect();
    }

    public int ButtonCount => ButtonMap.Count;

    public int AxisCount => AxisMap.Count;

    public int SliderCount => SliderMap.Count;

    public void SetButtonState(int index, bool pressed)
    {
        SetButtonState(ButtonMap[index], pressed);
    }

    public void SetAxisValue(int index, short value)
    {
        SetAxisValue(AxisMap[index],
            (byte)MathUtil.ConvertRange(
                short.MinValue,
                short.MaxValue,
                byte.MinValue,
                byte.MaxValue,
                value
            )
        );
    }

    public void SetSliderValue(int index, byte value)
    {
        SetSliderValue(SliderMap[index], value);
    }

    public bool AutoSubmitReport { get; set; } = true;

    public void ResetReport()
    {
        _nativeReport = default;

        _nativeReport.wButtons &= unchecked((ushort)~0xF);
        _nativeReport.wButtons |= 0x08; // resting HAT switch position
        _nativeReport.bThumbLX = 0x80; // centered axis value
        _nativeReport.bThumbLY = 0x80; // centered axis value
        _nativeReport.bThumbRX = 0x80; // centered axis value
        _nativeReport.bThumbRY = 0x80; // centered axis value
    }

    public void SubmitReport()
    {
        SubmitNativeReport(_nativeReport);
    }

    [Obsolete("This event might not behave as expected and has been deprecated. Use AwaitRawOutputReport() instead.")]
    public event DualShock4FeedbackReceivedEventHandler FeedbackReceived;

    private void SubmitNativeReport(ViGEmClient.DS4_REPORT report)
    {
        ViGEmClient.VIGEM_ERROR error = ViGEmClient.vigem_target_ds4_update(Client.NativeHandle, NativeHandle, report);
        ViGEmClient.CheckError(error);
    }
}

[Obsolete("This event might not behave as expected and has been deprecated. Use AwaitRawOutputReport() instead.")]
public delegate void DualShock4FeedbackReceivedEventHandler(object sender, DualShock4FeedbackReceivedEventArgs e);