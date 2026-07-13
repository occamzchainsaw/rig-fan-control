using Microsoft.Win32.TaskScheduler;
using System.Security.Principal;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace RigFanControl.Tray;

public static class StartupManager
{
    private const string TaskName = "RigFanControl";

    public static bool IsEnabled()
    {
        using var ts = new TaskService();
        return ts.GetTask(TaskName) is not null;
    }

    public static void Enable()
    {
        if (IsEnabled())
            return;

        using var ts = new TaskService();
        var td = ts.NewTask();

        td.RegistrationInfo.Description = "Starts RigFanControl elevated at logon";
        td.Principal.RunLevel = TaskRunLevel.Highest;
        td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name });
        td.Actions.Add(new ExecAction(Environment.ProcessPath!));

        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.StopIfGoingOnBatteries = false;

        ts.RootFolder.RegisterTaskDefinition(TaskName, td);
    }

    public static void Disable()
    {
        using var ts = new TaskService();
        ts.RootFolder.DeleteTask(TaskName, exceptionOnNotExists: false);
    }
}
