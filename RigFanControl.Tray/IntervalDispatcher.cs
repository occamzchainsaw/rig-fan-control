using System.Windows.Threading;

namespace RigFanControl.Tray;

public class IntervalDispatcher
{
    private DispatcherTimer? _timer = null;

    public void Start(int interval, Action action,
        DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
        Dispatcher? disp = null)
    {
        if (_timer != null) return;

        disp ??= Dispatcher.CurrentDispatcher;

        _timer = new DispatcherTimer(priority, disp) { Interval = TimeSpan.FromMilliseconds(interval) };
        _timer.Tick += (s, e) =>
        {
            if (_timer == null) return;

            action.Invoke();
        };
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }
}
