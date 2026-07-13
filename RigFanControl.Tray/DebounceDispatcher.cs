using System.Windows.Threading;

namespace RigFanControl.Tray;

public class DebounceDispatcher
{
    private DispatcherTimer? _timer = null;

    public void Debounce(int timeout, Action action,
        DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
        Dispatcher? disp = null)
    {
        _timer?.Stop();
        _timer = null;

        disp ??= Dispatcher.CurrentDispatcher;

        _timer = new DispatcherTimer(priority, disp)
        {
            Interval = TimeSpan.FromMilliseconds(timeout),
        };
        _timer.Tick += (s, e) =>
        {
            if (_timer == null) return;

            _timer.Stop();
            _timer = null;
            action.Invoke();
        };

        _timer.Start();
    }
}
