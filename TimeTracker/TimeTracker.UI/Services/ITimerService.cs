namespace TimeTracker.UI.Services;

public interface ITimerService
{
    event EventHandler<TimeSpan> TimerTick;
    bool IsRunning { get; }
    void Start(TimeSpan startTime);
    void Stop();
    void Reset();
} 