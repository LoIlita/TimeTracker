using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application.Services;
using Microsoft.Extensions.Logging;
using TimeTracker.UI.Services;
using TimeTracker.Application.DTOs;

namespace TimeTracker.UI.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IWorkSessionReader _sessionReader;
    private readonly ILogger<StatisticsViewModel> _logger;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _selectedHashtag;

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private string _totalTime = "00:00:00";

    [ObservableProperty]
    private string _averageSessionTime = "00:00:00";

    [ObservableProperty]
    private string _longestSessionTime = "00:00:00";

    [ObservableProperty]
    private int _todaySessions;

    [ObservableProperty]
    private string _todayTime = "00:00:00";

    [ObservableProperty]
    private string _thisWeekTime = "00:00:00";

    [ObservableProperty]
    private int _thisWeekSessions;

    [ObservableProperty]
    private string _thisMonthTime = "00:00:00";

    [ObservableProperty]
    private int _thisMonthSessions;

    [ObservableProperty]
    private string _mostActiveDay = "Brak danych";

    [ObservableProperty]
    private string _mostActiveTimeOfDay = "Brak danych";

    [ObservableProperty]
    private string _mostUsedHashtag = "Brak danych";

    [ObservableProperty]
    private ObservableCollection<string> _availableHashtags = new();

    [ObservableProperty]
    private bool _isBasicStatisticsExpanded = true;

    [ObservableProperty]
    private bool _isTimeStatisticsExpanded = true;

    [ObservableProperty]
    private bool _isActivityStatisticsExpanded = true;

    public string BasicStatisticsIcon => IsBasicStatisticsExpanded ? "▼" : "▶";
    public string TimeStatisticsIcon => IsTimeStatisticsExpanded ? "▼" : "▶";
    public string ActivityStatisticsIcon => IsActivityStatisticsExpanded ? "▼" : "▶";

    [RelayCommand]
    private void ToggleBasicStatistics()
    {
        IsBasicStatisticsExpanded = !IsBasicStatisticsExpanded;
        OnPropertyChanged(nameof(BasicStatisticsIcon));
    }

    [RelayCommand]
    private void ToggleTimeStatistics()
    {
        IsTimeStatisticsExpanded = !IsTimeStatisticsExpanded;
        OnPropertyChanged(nameof(TimeStatisticsIcon));
    }

    [RelayCommand]
    private void ToggleActivityStatistics()
    {
        IsActivityStatisticsExpanded = !IsActivityStatisticsExpanded;
        OnPropertyChanged(nameof(ActivityStatisticsIcon));
    }

    public StatisticsViewModel(
        IWorkSessionReader sessionReader,
        ILogger<StatisticsViewModel> logger,
        IDialogService dialogService)
    {
        _sessionReader = sessionReader;
        _logger = logger;
        _dialogService = dialogService;

        LoadStatisticsAsync().ConfigureAwait(false);
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Ładowanie statystyk - rozpoczęcie");

            // Pobierz wszystkie sesje
            var allSessions = await _sessionReader.GetAllAsync();
            var sessions = allSessions.ToList();

            // Filtruj po hashtagu jeśli wybrany
            if (!string.IsNullOrEmpty(SelectedHashtag))
            {
                sessions = sessions.Where(s => s.HashtagList.Contains(SelectedHashtag)).ToList();
            }

            // Oblicz podstawowe statystyki
            CalculateBasicStatistics(sessions);

            // Oblicz statystyki czasowe
            CalculateTimeBasedStatistics(sessions);

            // Oblicz statystyki aktywności
            CalculateActivityStatistics(sessions);

            // Załaduj dostępne hashtagi
            var hashtags = await _sessionReader.GetUniqueHashtags();
            AvailableHashtags = new ObservableCollection<string>(hashtags);

            _logger.LogInformation("Załadowano statystyki - zakończenie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania statystyk");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się załadować statystyk");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateBasicStatistics(List<WorkSessionDto> sessions)
    {
        TotalSessions = sessions.Count;
        
        var totalTime = TimeSpan.FromTicks(sessions.Sum(s => s.Duration?.Ticks ?? 0));
        TotalTime = totalTime.ToString(@"hh\:mm\:ss");

        if (TotalSessions > 0)
        {
            var averageTime = TimeSpan.FromTicks((long)sessions.Average(s => s.Duration?.Ticks ?? 0));
            AverageSessionTime = averageTime.ToString(@"hh\:mm\:ss");

            var longestSession = sessions.Max(s => s.Duration ?? TimeSpan.Zero);
            LongestSessionTime = longestSession.ToString(@"hh\:mm\:ss");
        }
    }

    private void CalculateTimeBasedStatistics(List<WorkSessionDto> sessions)
    {
        // Dzisiaj
        var todaySessions = sessions.Where(s => s.StartTime.Date == DateTime.Today).ToList();
        TodaySessions = todaySessions.Count;
        var todayTime = TimeSpan.FromTicks(todaySessions.Sum(s => s.Duration?.Ticks ?? 0));
        TodayTime = todayTime.ToString(@"hh\:mm\:ss");

        // Ten tydzień
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var thisWeekSessions = sessions.Where(s => s.StartTime.Date >= startOfWeek).ToList();
        ThisWeekSessions = thisWeekSessions.Count;
        var thisWeekTime = TimeSpan.FromTicks(thisWeekSessions.Sum(s => s.Duration?.Ticks ?? 0));
        ThisWeekTime = thisWeekTime.ToString(@"hh\:mm\:ss");

        // Ten miesiąc
        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var thisMonthSessions = sessions.Where(s => s.StartTime.Date >= startOfMonth).ToList();
        ThisMonthSessions = thisMonthSessions.Count;
        var thisMonthTime = TimeSpan.FromTicks(thisMonthSessions.Sum(s => s.Duration?.Ticks ?? 0));
        ThisMonthTime = thisMonthTime.ToString(@"hh\:mm\:ss");
    }

    private void CalculateActivityStatistics(List<WorkSessionDto> sessions)
    {
        if (!sessions.Any()) return;

        // Najbardziej aktywny dzień
        var mostActiveDay = sessions
            .GroupBy(s => s.StartTime.Date)
            .OrderByDescending(g => g.Sum(s => s.Duration?.Ticks ?? 0))
            .First();
        MostActiveDay = $"{mostActiveDay.Key:d MMMM yyyy} ({TimeSpan.FromTicks(mostActiveDay.Sum(s => s.Duration?.Ticks ?? 0)):hh\\:mm\\:ss})";

        // Najbardziej aktywna pora dnia
        var mostActiveHour = sessions
            .GroupBy(s => s.StartTime.Hour)
            .OrderByDescending(g => g.Count())
            .First();
        MostActiveTimeOfDay = $"{mostActiveHour.Key:00}:00 - {(mostActiveHour.Key + 1):00}:00 ({mostActiveHour.Count()} sesji)";

        // Najczęściej używany hashtag
        var allHashtags = sessions
            .SelectMany(s => s.HashtagList)
            .GroupBy(h => h)
            .OrderByDescending(g => g.Count());

        if (allHashtags.Any())
        {
            var topHashtag = allHashtags.First();
            MostUsedHashtag = $"{topHashtag.Key} ({topHashtag.Count()} sesji)";
        }
    }

    partial void OnSelectedHashtagChanged(string? value)
    {
        _logger.LogInformation("Zmiana wybranego hashtagu na: {Value}", value ?? "brak");
        LoadStatisticsAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task RefreshStatistics()
    {
        await LoadStatisticsAsync();
    }

    [RelayCommand]
    private async Task ResetStatistics()
    {
        try
        {
            _logger.LogInformation("Resetowanie statystyk");
            SelectedHashtag = null;
            await LoadStatisticsAsync();
            _logger.LogInformation("Statystyki zresetowane");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas resetowania statystyk");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się zresetować statystyk");
        }
    }
} 