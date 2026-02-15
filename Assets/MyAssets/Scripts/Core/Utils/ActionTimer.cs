using Cysharp.Threading.Tasks;
using System;
using System.Timers;


internal class ActionTimer
{
    private Action _actionElapsed;
    private System.Timers.Timer _timer;
    private float _interval;

    public ActionTimer(Action actionElapsed, float intervalInSeconds = 1f)
    {
        _interval = intervalInSeconds * 1000; // Convert seconds to milliseconds
        _actionElapsed = actionElapsed;
        SettingsTimer();
    }

    public void SetAction(Action newAction) => _actionElapsed = newAction;

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    private async void TimerElapsed(object sender, ElapsedEventArgs e) => await UniTask.RunOnThreadPool(_actionElapsed);

    private void SettingsTimer()
    {
        _timer = new System.Timers.Timer(_interval);
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Elapsed += TimerElapsed;
        _timer.Stop();
    }
}