using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.Services;
using TimeTracker.UI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using TimeTracker.UI.Views;
using Application = Microsoft.Maui.Controls.Application;

namespace TimeTracker.UI.ViewModels;

public partial class WorkTrackerViewModel : ObservableObject, IDisposable
{
    public class SessionGroup : ObservableCollection<WorkSessionDto>
    {
        public DateTime Date { get; private set; }

        public SessionGroup(DateTime date, IEnumerable<WorkSessionDto> sessions) : base(sessions)
        {
            Date = date;
        }
    }

    private readonly IWorkSessionReader _sessionReader;
    private readonly IWorkSessionWriter _sessionWriter;
    private readonly ITimerService _timerService;
    private readonly ILogger<WorkTrackerViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;
    private TimeSpan _currentDuration;
    private TimeSpan _pausedDuration;
    private string _timerDisplay = "00:00:00";
    private string _description = string.Empty;
    private string _pauseButtonText = "Pauza";
    private string _startButtonText = "Start";
    private string _tags = string.Empty;
    private ObservableCollection<WorkSessionDto> _recentSessions = new();
    private ObservableCollection<SessionGroup> _filteredSessions = new();
    private ObservableCollection<string> _availableHashtags = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanStop))]
    [NotifyPropertyChangedFor(nameof(CanEditDescription))]
    [NotifyPropertyChangedFor(nameof(StartButtonColor))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopSessionCommand))]
    private bool isSessionActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    [NotifyCanExecuteChangedFor(nameof(TogglePauseCommand))]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isFilterVisible;

    [ObservableProperty]
    private bool _isTodaySessionsExpanded = true;

    public ObservableCollection<SessionGroup> FilteredSessions
    {
        get => _filteredSessions;
        private set => SetProperty(ref _filteredSessions, value);
    }

    public ObservableCollection<string> AvailableHashtags
    {
        get => _availableHashtags;
        private set => SetProperty(ref _availableHashtags, value);
    }

    [ObservableProperty]
    private string? _selectedHashtag;

    public WorkTrackerViewModel(
        IWorkSessionReader sessionReader,
        IWorkSessionWriter sessionWriter,
        ITimerService timerService,
        ILogger<WorkTrackerViewModel> logger,
        IDialogService dialogService,
        IServiceProvider serviceProvider)
    {
        _sessionReader = sessionReader;
        _sessionWriter = sessionWriter;
        _timerService = timerService;
        _logger = logger;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        _timerService.TimerTick += OnTimerTick;

        InitializeAsync().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        try
        {
            await CheckActiveSessionAsync();
            await LoadRecentSessionsAsync();
            await UpdateAvailableHashtags();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas inicjalizacji");
            await ShowAlertAsync("Błąd", "Wystąpił błąd podczas inicjalizacji aplikacji.");
        }
    }

    private async Task UpdateAvailableHashtags()
    {
        try
        {
            var hashtags = await _sessionReader.GetUniqueHashtags();
            AvailableHashtags = new ObservableCollection<string>(hashtags);
            _logger.LogInformation("Zaktualizowano listę dostępnych hashtagów: {Count}", AvailableHashtags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji listy hashtagów");
        }
    }

    public void Dispose()
    {
        _timerService.TimerTick -= OnTimerTick;
    }

    private void OnTimerTick(object? sender, TimeSpan duration)
    {
        try
        {
            _logger.LogDebug("Timer tick - Duration: {Duration}", duration);
            
            if (!IsSessionActive || IsPaused)
            {
                _logger.LogDebug("Timer tick zignorowany - IsSessionActive: {IsSessionActive}, IsPaused: {IsPaused}", 
                    IsSessionActive, IsPaused);
                return;
            }

        CurrentDuration = duration;
        TimerDisplay = duration.ToString(@"hh\:mm\:ss");
            _logger.LogDebug("Timer tick zaktualizowany - Display: {Display}, Duration: {Duration}", 
                TimerDisplay, CurrentDuration);
    }
        catch (Exception ex)
    {
            _logger.LogError(ex, "Błąd podczas obsługi timer tick");
        }
    }

    public TimeSpan CurrentDuration
    {
        get => _currentDuration;
        private set => SetProperty(ref _currentDuration, value);
    }

    public string TimerDisplay
    {
        get => _timerDisplay;
        private set => SetProperty(ref _timerDisplay, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Tags
    {
        get => _tags;
        set 
        {
            if (SetProperty(ref _tags, value))
            {
                OnPropertyChanged(nameof(HashtagList));
            }
        }
    }

    public IEnumerable<string> HashtagList 
    { 
        get
        {
            if (string.IsNullOrEmpty(Tags)) 
                return Enumerable.Empty<string>();
            
            return Tags.Split(' ')
                .Where(w => w.StartsWith("#"))
                .Select(w => w.Trim())
                .Distinct();
        }
    }

    public ObservableCollection<WorkSessionDto> RecentSessions
    {
        get => _recentSessions;
        private set => SetProperty(ref _recentSessions, value);
    }

    public string PauseButtonText
    {
        get => _pauseButtonText;
        private set => SetProperty(ref _pauseButtonText, value);
    }

    public TimeSpan ElapsedTime
    {
        get => CurrentDuration;
    }

    public string StartButtonText
    {
        get => _startButtonText;
        private set => SetProperty(ref _startButtonText, value);
    }

    public bool CanStart => !IsSessionActive;

    public bool CanPause => IsSessionActive;

    public bool CanStop => IsSessionActive;

    public bool CanEditDescription => !IsSessionActive;
    public string StartButtonColor => IsSessionActive ? "Gray" : "Green";

    private async Task ShowAlertAsync(string title, string message)
    {
        try
        {
            await _dialogService.ShowInfoAsync(title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania alertu");
        }
    }

    private async Task CheckActiveSessionAsync()
    {
        try
        {
            _logger.LogInformation("Sprawdzanie aktywnej sesji...");
            var activeSession = await _sessionReader.GetActiveSessionAsync();
            if (activeSession != null)
            {
                _logger.LogWarning("Znaleziono niezakończoną sesję z ID: {Id}", activeSession.Id);
                await _sessionWriter.CompleteCurrentSessionAsync();
                _logger.LogInformation("Automatycznie zakończono poprzednią sesję");
            }
            IsSessionActive = false;
            _logger.LogInformation("Stan IsSessionActive ustawiony na: {IsSessionActive}", IsSessionActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania aktywnej sesji");
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                await _dialogService.ShowErrorAsync("Błąd", "Opis sesji nie może być pusty");
                return;
            }

            var dto = new CreateWorkSessionDto
            {
                Description = Description,
                Tags = Tags
            };

            _logger.LogInformation("Tworzenie nowej sesji z tagami: {Tags}", dto.Tags);

            var session = await _sessionWriter.StartSessionAsync(dto);
            _logger.LogInformation("Rozpoczęto nową sesję o ID: {Id} z tagami: {Tags}", session.Id, session.Tags);

            // Reset timera przed rozpoczęciem nowej sesji
            _timerService.Reset();
            CurrentDuration = TimeSpan.Zero;
            TimerDisplay = "00:00:00";
            
            IsSessionActive = true;
            _timerService.Start(TimeSpan.Zero);

            // Wyczyść opis i tagi po rozpoczęciu sesji
            Description = string.Empty;
            Tags = string.Empty;
            
            // Odśwież listę dostępnych hashtagów
            await UpdateAvailableHashtags();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas rozpoczynania sesji");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się rozpocząć sesji");
        }
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void TogglePause()
    {
        try
        {
            _logger.LogInformation("TogglePause został wywołany");
            
            if (IsSessionActive)
            {
                if (IsPaused)
                {
                    _logger.LogInformation("Wznawianie sesji...");
                    _timerService.Start(_pausedDuration);
                    IsPaused = false;
                    PauseButtonText = "Pauza";
                    _logger.LogInformation("Sesja wznowiona od czasu: {Duration}", _pausedDuration);
                }
                else
                {
                    _logger.LogInformation("Wstrzymywanie sesji...");
                    _timerService.Stop();
                    IsPaused = true;
                    _pausedDuration = CurrentDuration;
                    PauseButtonText = "Wznów";
                    _logger.LogInformation("Sesja wstrzymana na czasie: {Duration}", _pausedDuration);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przełączania pauzy");
            // Próba przywrócenia spójnego stanu
            IsPaused = false;
            PauseButtonText = "Pauza";
        }
    }

    private bool CanExecuteStop()
    {
        var canStop = IsSessionActive;
        _logger.LogDebug("CanExecuteStop wywołane: {CanStop}", canStop);
        return canStop;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStop))]
    private async Task StopSession()
    {
        try
        {
            _logger.LogInformation("StopSession został wywołany");
            
            if (!IsSessionActive)
            {
                _logger.LogWarning("Próba zatrzymania nieaktywnej sesji");
                return;
            }

            var activeSession = await _sessionReader.GetActiveSessionAsync();
            _logger.LogInformation("Aktywna sesja: {SessionId}", activeSession?.Id);

            var result = await _dialogService.ShowConfirmationAsync(
                "Potwierdzenie",
                "Czy na pewno chcesz zakończyć sesję?"
            );

            if (!result)
            {
                _logger.LogInformation("Użytkownik anulował zakończenie sesji");
                return;
            }

            _timerService.Stop();
            await _sessionWriter.CompleteCurrentSessionWithDurationAsync(CurrentDuration);
            
            IsSessionActive = false;
            IsPaused = false;
            CurrentDuration = TimeSpan.Zero;
            TimerDisplay = "00:00:00";
            
            await LoadRecentSessionsAsync();
            
            _logger.LogInformation("Sesja została zakończona");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zatrzymywania sesji");
            await ShowAlertAsync("Błąd", "Nie udało się zakończyć sesji.");
        }
    }

    private async Task LoadRecentSessionsAsync()
    {
        try
        {
            _logger.LogInformation("Ładowanie dzisiejszych sesji");
            var allSessions = await _sessionReader.GetAllAsync();
            var todaySessions = allSessions.Where(s => s.StartTime.Date == DateTime.Today);
            RecentSessions = new ObservableCollection<WorkSessionDto>(todaySessions);
            await UpdateFilteredSessions();
            _logger.LogInformation("Załadowano {Count} dzisiejszych sesji", todaySessions.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania dzisiejszych sesji");
        }
    }

    private async Task UpdateFilteredSessions()
    {
        try
        {
            if (_sessionReader == null)
            {
                _logger.LogError("_sessionReader jest null");
                await _dialogService.ShowErrorAsync("Błąd", "Nie można połączyć się z bazą danych");
                return;
            }

            var sessions = await _sessionReader.GetRecentSessionsAsync();
            if (sessions == null)
            {
                _logger.LogError("Otrzymano null zamiast listy sesji");
                await _dialogService.ShowErrorAsync("Błąd", "Nie udało się pobrać listy sesji");
                return;
            }

            _logger.LogInformation("Pobrano {Count} sesji z bazy", sessions.Count());
            var filteredSessions = sessions.ToList();
            _logger.LogInformation("Przekonwertowano do listy: {Count} sesji", filteredSessions.Count);

            // Logowanie hashtagów dla wszystkich sesji
            foreach (var session in filteredSessions)
            {
                _logger.LogInformation("Sesja {Id}: Tags={Tags}, HashtagList=[{Hashtags}]", 
                    session.Id, session.Tags, string.Join(", ", session.HashtagList));
            }

            if (SelectedHashtag != null)
            {
                _logger.LogInformation("Filtrowanie po hashtagu: {Hashtag}", SelectedHashtag);
                var beforeFilter = filteredSessions.Count;
                filteredSessions = filteredSessions.Where(s => 
                {
                    var hasHashtag = s.HashtagList.Contains(SelectedHashtag);
                    _logger.LogInformation("Sesja {Id}: Tags={Tags}, HashtagList=[{Hashtags}], Contains({Hashtag})={Result}", 
                        s.Id, s.Tags, string.Join(", ", s.HashtagList), SelectedHashtag, hasHashtag);
                    return hasHashtag;
                }).ToList();
                _logger.LogInformation("Po filtrowaniu: z {Before} sesji zostało {After}", beforeFilter, filteredSessions.Count);
            }

            var groupedSessions = filteredSessions
                .GroupBy(s => s.StartTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new SessionGroup(g.Key, g.OrderByDescending(s => s.StartTime)))
                .ToList();

            FilteredSessions = new ObservableCollection<SessionGroup>(groupedSessions);
            _logger.LogInformation("Zaktualizowano przefiltrowane sesje. Liczba grup: {Count}, łączna liczba sesji: {SessionCount}", 
                groupedSessions.Count, filteredSessions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji przefiltrowanych sesji");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się zaktualizować listy sesji");
        }
    }

    [RelayCommand]
    private async Task ShowHistory()
    {
        try
        {
            _logger.LogInformation("Otwieranie historii sesji");
            await Shell.Current.GoToAsync("sessionhistory");
            _logger.LogInformation("Historia sesji otwarta");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas otwierania historii sesji");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się otworzyć historii sesji");
        }
    }

    [RelayCommand]
    private void ToggleFilter()
    {
        IsFilterVisible = !IsFilterVisible;
        _logger.LogInformation("Przełączono widoczność filtra na: {IsFilterVisible}", IsFilterVisible);
    }

    partial void OnSelectedHashtagChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Dodaj wybrany hashtag do pola Tags
            if (string.IsNullOrEmpty(Tags))
            {
                Tags = value;
            }
            else if (!Tags.Contains(value))
            {
                Tags += " " + value;
            }
            // Wyczyść wybór
            SelectedHashtag = null;
        }
        UpdateFilteredSessions().ConfigureAwait(false);
    }

    [RelayCommand]
    private void ToggleTodaySessions()
    {
        IsTodaySessionsExpanded = !IsTodaySessionsExpanded;
        _logger.LogInformation("Przełączono widoczność dzisiejszych sesji: {IsExpanded}", IsTodaySessionsExpanded);
    }

    [RelayCommand]
    private async Task AddHashtag()
    {
        try
        {
            var hashtag = await _dialogService.ShowInputAsync(
                "Nowy hashtag",
                "Wprowadź nowy hashtag (bez znaku #):");

            if (!string.IsNullOrWhiteSpace(hashtag))
            {
                hashtag = "#" + hashtag.Trim();
                if (string.IsNullOrEmpty(Tags))
                {
                    Tags = hashtag;
                }
                else
                {
                    Tags += " " + hashtag;
                }
                
                // Odśwież listę dostępnych hashtagów
                await UpdateAvailableHashtags();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania hashtagu");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się dodać hashtagu");
        }
    }

    [RelayCommand]
    private async Task ShowStatistics()
    {
        try
        {
            await Shell.Current.GoToAsync("statistics");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przechodzenia do widoku statystyk");
            await ShowAlertAsync("Błąd", "Nie udało się otworzyć statystyk");
        }
    }
}
