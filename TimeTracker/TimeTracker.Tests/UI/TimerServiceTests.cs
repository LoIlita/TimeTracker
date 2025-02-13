using TimeTracker.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace TimeTracker.Tests.UI;

public class TimerServiceTests : IDisposable
{
    private readonly TimerService _timerService;
    private readonly Mock<ILogger<TimerService>> _loggerMock;
    private TimeSpan _lastTick;
    private bool _tickReceived;

    public TimerServiceTests()
    {
        _loggerMock = new Mock<ILogger<TimerService>>();
        _timerService = new TimerService(_loggerMock.Object);
        _timerService.TimerTick += OnTimerTick;
        _tickReceived = false;
    }

    private void OnTimerTick(object? sender, TimeSpan duration)
    {
        _lastTick = duration;
        _tickReceived = true;
    }

    public void Dispose()
    {
        _timerService.TimerTick -= OnTimerTick;
    }

    [Fact]
    public void Start_ShouldStartTimer()
    {
        // Act
        _timerService.Start(TimeSpan.Zero);

        // Assert
        Assert.True(_timerService.IsRunning);
    }

    [Fact]
    public void Stop_ShouldStopTimer()
    {
        // Arrange
        _timerService.Start(TimeSpan.Zero);

        // Act
        _timerService.Stop();

        // Assert
        Assert.False(_timerService.IsRunning);
    }

    [Fact]
    public void Reset_ShouldResetTimer()
    {
        // Arrange
        _timerService.Start(TimeSpan.Zero);
        Thread.Sleep(1100); // Wait for at least one tick

        // Act
        _timerService.Reset();

        // Assert
        Assert.False(_timerService.IsRunning);
        Assert.Equal(TimeSpan.Zero, _lastTick);
    }

    [Fact]
    public async Task Timer_ShouldTickEverySecond()
    {
        // Arrange
        _tickReceived = false;

        // Act
        _timerService.Start(TimeSpan.Zero);
        await Task.Delay(1100); // Wait for at least one tick

        // Assert
        Assert.True(_tickReceived);
        Assert.True(_lastTick.TotalSeconds >= 1);
    }

    [Fact]
    public async Task Timer_ShouldNotTickWhenStopped()
    {
        // Arrange
        _timerService.Start(TimeSpan.Zero);
        await Task.Delay(1100); // Wait for at least one tick
        var lastTickBeforeStop = _lastTick;
        _tickReceived = false;

        // Act
        _timerService.Stop();
        await Task.Delay(1100); // Wait to ensure no more ticks

        // Assert
        Assert.False(_tickReceived);
        Assert.Equal(lastTickBeforeStop, _lastTick);
    }
} 