# Rig Fan Control

A lightweight Windows tray app for controlling the speed of PC case fans connected to your motherboard's PWM fan headers.

I built this to control the fans I use to keep myself cool in my sim racing rig. If you're pointing PC fans at yourself for ventilation in a sim rig (or anywhere else), it might be useful to you too.

This is **not** a wind simulator — it doesn't tie fan speed to in-game telemetry. It's a simple manual speed control that lives in your system tray.

## What it does

- Runs quietly in the system tray with a click-to-open flyout.
- Lists the fan control headers exposed by your motherboard and lets you pick one.
- Sets that fan's speed with a 0–100% slider.
- Reads back the live RPM from the fan's tachometer.
- Remembers your selected fan and last speed between sessions.
- Optionally starts with Windows.
- Hands control back to the BIOS on exit, so your fans return to their normal curve.

## How it works

The **Tray** project (`RigFanControl.Tray`) is the app you run. It's a WPF application targeting .NET 10 on Windows, built with the MVVM pattern (CommunityToolkit.Mvvm) and dependency injection.

- **Fan detection and control** come from the `RigFanControl.Core` project, which wraps [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) to enumerate motherboard Super I/O sensors, drive a control sensor via software PWM, and read the paired tachometer.
- **Tray icon and flyout** are provided by H.NotifyIcon.Wpf.
- **Config** is stored as JSON in `%AppData%\RigFanControl\config.json` (selected fan and last speed).
- **Start with Windows** is registered through the Windows Task Scheduler.

Because it writes directly to hardware sensors, the app requires administrator privileges (declared in its application manifest).

## Requirements

- Windows
- .NET 10 runtime
- A motherboard with software-controllable PWM fan headers
- Administrator privileges (needed to access hardware sensors)

## Building

```
dotnet build RigFanControl.slnx
```

To publish a self-contained single-file executable:

```
dotnet publish RigFanControl.Tray -c Release
```

## Usage

> ⚠️ **The app will prompt for administrator privileges on launch.** This is required — writing to the motherboard's hardware sensors to control the fans doesn't work without it.

1. Launch the app — a fan icon appears in the system tray.
2. Open the flyout and go to settings to pick which fan header to control, then save.
3. Toggle control on and set your desired speed with the slider.
4. The live RPM readout confirms the fan is responding.

Exiting the app returns the fan to BIOS control.

## Projects

| Project | Description |
| --- | --- |
| `RigFanControl.Tray` | The Windows tray application (WPF). |
| `RigFanControl.Core` | Fan discovery, control, and config logic (LibreHardwareMonitor). |
| `RigFanControl.Ipc` | Inter-process communication (work in progress). |
| `RigFanCmdTest` | Command-line test harness. |
</content>
</invoke>
