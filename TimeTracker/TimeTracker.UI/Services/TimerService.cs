using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Diagnostics;

namespace TimeTracker.UI.Services;

public class TimerService : ITimerService, IDisposable
{
    private readonly ILogger<TimerService> _logger;
    private readonly Timer _timer;
    private TimeSpan _elapsed;
    private bool _isRunning;
    private DateTime _startTime;
    private readonly Stopwatch _stopwatch;
    private bool _isPaused;
    private TimeSpan _pausedElapsedTime;

    public event EventHandler<TimeSpan>? TimerTick;

    public bool IsRunning => _isRunning;

    public TimerService(ILogger<TimerService> logger)
    {
        _logger = logger;
        _timer = new Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _elapsed = TimeSpan.Zero;
        _stopwatch = new Stopwatch();
        _pausedElapsedTime = TimeSpan.Zero;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (!_isRunning || _isPaused) return;

            // Obliczamy aktualny czas
            var actualElapsed = _elapsed.Add(_stopwatch.Elapsed);
            
            MainThread.BeginInvokeOnMainThread(() => 
            {
                TimerTick?.Invoke(this, actualElapsed);
            });

            // Co 10 sekund sprawdzamy i korygujemy ewentualne odchylenia
            if (_stopwatch.Elapsed.Seconds % 10 == 0)
            {
                var expectedTime = _startTime.Add(_elapsed).Add(_stopwatch.Elapsed);
                var actualTime = DateTime.Now;
                var drift = actualTime - expectedTime;
                
                if (Math.Abs(drift.TotalSeconds) > 1)
                {
                    _logger.LogWarning("Wykryto odchylenie czasu: {Drift}s", drift.TotalSeconds);
                    // Korygujemy czas
                    _elapsed = actualElapsed;
                    _startTime = DateTime.Now;
                    _stopwatch.Restart();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji timera");
        }
    }

    public void Start(TimeSpan startTime)
    {
        try
        {
            _logger.LogInformation("Uruchamianie timera od czasu: {StartTime}", startTime);
            
            if (_isPaused)
            {
                _logger.LogInformation("Wznawianie timera od czasu pauzy: {PausedTime}", _pausedElapsedTime);
                _elapsed = _pausedElapsedTime;
                _isPaused = false;
            }
            else
            {
                _logger.LogInformation("Rozpoczynanie nowego timera");
                _elapsed = startTime;
                _pausedElapsedTime = TimeSpan.Zero;
            }

            _startTime = DateTime.Now;
            _stopwatch.Reset();
            _stopwatch.Start();
            _timer.Start();
            _isRunning = true;
            TimerTick?.Invoke(this, _elapsed);
            _logger.LogInformation("Timer uruchomiony pomyślnie. IsRunning: {IsRunning}, IsPaused: {IsPaused}", _isRunning, _isPaused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas uruchamiania timera");
            throw;
        }
    }

    public void Stop()
    {
        try
        {
            _logger.LogInformation("Zatrzymywanie timera. Aktualny stan - IsRunning: {IsRunning}, IsPaused: {IsPaused}", _isRunning, _isPaused);
            _timer.Stop();
            _stopwatch.Stop();
            
            // Zapisujemy całkowity czas, który upłynął do momentu zatrzymania
            _pausedElapsedTime = _elapsed.Add(_stopwatch.Elapsed);
            _logger.LogInformation("Zapisano czas pauzy: {PausedTime}", _pausedElapsedTime);
            
            _isPaused = true;
            _isRunning = false;
            
            // Wywołujemy event z aktualnym czasem
            TimerTick?.Invoke(this, _pausedElapsedTime);
            _logger.LogInformation("Timer zatrzymany pomyślnie. Nowy stan - IsRunning: {IsRunning}, IsPaused: {IsPaused}", _isRunning, _isPaused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zatrzymywania timera");
            throw;
        }
    }

    public void Reset()
    {
        try
        {
            _logger.LogInformation("Resetowanie timera");
            _timer.Stop();
            _stopwatch.Stop();
            _stopwatch.Reset();
            _elapsed = TimeSpan.Zero;
            _pausedElapsedTime = TimeSpan.Zero;
            _isPaused = false;
            _isRunning = false;
            
            // Wywołujemy event z zerowym czasem i upewniamy się, że jest na głównym wątku
            MainThread.BeginInvokeOnMainThread(() => 
            {
                TimerTick?.Invoke(this, TimeSpan.Zero);
            });
            
            _logger.LogInformation("Timer zresetowany pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas resetowania timera");
            throw;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
} 