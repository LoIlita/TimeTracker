using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.Services;
using TimeTracker.UI.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TimeTracker.UI.ViewModels;

public partial class SessionHistoryViewModel : ObservableObject
{
    private readonly IWorkSessionReader _sessionReader;
    private readonly IWorkSessionService _sessionService;
    private readonly ILogger<SessionHistoryViewModel> _logger;
    private readonly IDialogService _dialogService;
    private ObservableCollection<WorkTrackerViewModel.SessionGroup> _sessions = new();
    private ObservableCollection<string> _availableHashtags = new();

    [ObservableProperty]
    private string? _selectedHashtag;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<WorkTrackerViewModel.SessionGroup> Sessions
    {
        get => _sessions;
        private set => SetProperty(ref _sessions, value);
    }

    public ObservableCollection<string> AvailableHashtags
    {
        get => _availableHashtags;
        private set => SetProperty(ref _availableHashtags, value);
    }

    public SessionHistoryViewModel(
        IWorkSessionReader sessionReader,
        IWorkSessionService sessionService,
        ILogger<SessionHistoryViewModel> logger,
        IDialogService dialogService)
    {
        _sessionReader = sessionReader;
        _sessionService = sessionService;
        _logger = logger;
        _dialogService = dialogService;

        LoadHistoryAsync().ConfigureAwait(false);
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Ładowanie historii sesji - rozpoczęcie");

            var allSessions = await _sessionReader.GetAllAsync();
            _logger.LogInformation("Pobrano {Count} sesji z bazy danych", allSessions.Count());
            
            var hashtags = await _sessionReader.GetUniqueHashtags();
            AvailableHashtags = new ObservableCollection<string>(hashtags);
            _logger.LogInformation("Załadowano {Count} hashtagów", AvailableHashtags.Count);
            
            UpdateSessionGroups(allSessions);

            _logger.LogInformation("Załadowano historię sesji - zakończenie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania historii sesji");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateSessionGroups(IEnumerable<WorkSessionDto> sessions)
    {
        _logger.LogInformation("UpdateSessionGroups - rozpoczęcie z {Count} sesjami", sessions.Count());
        _logger.LogInformation("Aktualnie wybrany hashtag: {SelectedHashtag}", SelectedHashtag ?? "brak");

        var filteredSessions = sessions;
        if (!string.IsNullOrEmpty(SelectedHashtag))
        {
            _logger.LogInformation("Filtrowanie po hashtagu: {Hashtag}", SelectedHashtag);
            var beforeFilter = filteredSessions.Count();
            filteredSessions = sessions.Where(s => 
            {
                var hasHashtag = !string.IsNullOrEmpty(s.Tags) && s.HashtagList.Contains(SelectedHashtag);
                _logger.LogInformation("Sprawdzanie sesji {Id}: Tags={Tags}, HashtagList=[{Hashtags}], Contains({Hashtag})={Result}", 
                    s.Id, s.Tags, string.Join(", ", s.HashtagList), SelectedHashtag, hasHashtag);
                return hasHashtag;
            });
            _logger.LogInformation("Po filtrowaniu: z {Before} sesji zostało {After}", beforeFilter, filteredSessions.Count());
        }

        _logger.LogInformation("Grupowanie sesji...");
        var groupedSessions = filteredSessions
            .OrderByDescending(s => s.StartTime)
            .GroupBy(s => s.StartTime.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new WorkTrackerViewModel.SessionGroup(g.Key, g.ToList()));

        var groupedList = groupedSessions.ToList();
        _logger.LogInformation("Utworzono {GroupCount} grup sesji", groupedList.Count);

        Sessions = new ObservableCollection<WorkTrackerViewModel.SessionGroup>(groupedList);
        _logger.LogInformation("UpdateSessionGroups - zakończenie. Liczba grup w kolekcji: {Count}", Sessions.Count);
    }

    [RelayCommand]
    private async Task RefreshHistory()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task ClearFilter()
    {
        SelectedHashtag = null;
        await LoadHistoryAsync();
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
                _logger.LogInformation("Próba dodania nowego hashtagu: {Hashtag}", hashtag);
                
                // Odśwież całą historię, aby pobrać aktualne hashtagi
                await LoadHistoryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania hashtagu");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się dodać hashtagu");
        }
    }

    partial void OnSelectedHashtagChanged(string? value)
    {
        _logger.LogInformation("Zmiana wybranego hashtagu na: {Value}", value ?? "brak");
        LoadHistoryAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void SelectHashtag(string hashtag)
    {
        _logger.LogInformation("Wybrano hashtag: {Hashtag}", hashtag);
        SelectedHashtag = hashtag;
    }

    [RelayCommand]
    private async Task DeleteSession(WorkSessionDto session)
    {
        try
        {
            if (session == null) return;

            var result = await _dialogService.ShowConfirmationAsync(
                "Usuwanie sesji",
                "Czy na pewno chcesz usunąć tę sesję?");

            if (!result) return;

            await _sessionService.DeleteSessionAsync(session.Id);
            await LoadHistoryAsync();
            
            _logger.LogInformation("Sesja została usunięta pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania sesji");
            await _dialogService.ShowErrorAsync("Błąd", "Nie udało się usunąć sesji");
        }
    }
} 