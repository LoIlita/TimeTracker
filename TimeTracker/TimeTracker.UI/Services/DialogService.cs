using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace TimeTracker.UI.Services;

public class DialogService : IDialogService
{
    private readonly ILogger<DialogService> _logger;

    public DialogService(ILogger<DialogService> logger)
    {
        _logger = logger;
    }

    private Page GetCurrentPage()
    {
        try
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            _logger.LogDebug("MainPage type: {Type}", mainPage?.GetType().Name);

            if (mainPage == null)
            {
                _logger.LogWarning("Application.Current.MainPage jest null");
                throw new InvalidOperationException("Nie można znaleźć aktywnej strony");
            }

            if (mainPage is NavigationPage navigationPage)
            {
                var currentPage = navigationPage.CurrentPage;
                _logger.LogDebug("NavigationPage.CurrentPage type: {Type}", currentPage?.GetType().Name);
                if (currentPage == null)
                {
                    _logger.LogDebug("Używam NavigationPage jako fallback");
                    return navigationPage;
                }
                return currentPage;
            }

            _logger.LogDebug("Używam MainPage jako ostateczność: {Type}", mainPage.GetType().Name);
            return mainPage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania aktualnej strony");
            throw;
        }
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            _logger.LogDebug("Próba wyświetlenia błędu: {Title} - {Message}", title, message);
            
            if (!MainThread.IsMainThread)
            {
                _logger.LogDebug("Wywołanie nie jest na głównym wątku, przełączam na główny wątek");
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var page = GetCurrentPage();
                    _logger.LogDebug("Wyświetlam alert błędu na stronie typu: {PageType}", page.GetType().Name);
                    await page.DisplayAlert(title, message, "OK");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wyświetlania alertu błędu");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania komunikatu błędu. Message: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
        }
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        try
        {
            _logger.LogDebug("Próba wyświetlenia informacji: {Title} - {Message}", title, message);
            
            if (!MainThread.IsMainThread)
            {
                _logger.LogDebug("Wywołanie nie jest na głównym wątku, przełączam na główny wątek");
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var page = GetCurrentPage();
                    _logger.LogDebug("Wyświetlam alert informacyjny na stronie typu: {PageType}", page.GetType().Name);
                    await page.DisplayAlert(title, message, "OK");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wyświetlania alertu informacyjnego");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania informacji. Message: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        try
        {
            _logger.LogDebug("Próba wyświetlenia potwierdzenia: {Title} - {Message}", title, message);
            
            if (!MainThread.IsMainThread)
            {
                _logger.LogDebug("Wywołanie nie jest na głównym wątku, przełączam na główny wątek");
            }

            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var page = GetCurrentPage();
                    _logger.LogDebug("Wyświetlam dialog potwierdzenia na stronie typu: {PageType}", page.GetType().Name);
                    return await page.DisplayAlert(title, message, "Tak", "Nie");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wyświetlania dialogu potwierdzenia");
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania potwierdzenia. Message: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        try
        {
            _logger.LogDebug("Próba wyświetlenia pola wprowadzania: {Title} - {Message}", title, message);
            
            if (!MainThread.IsMainThread)
            {
                _logger.LogDebug("Wywołanie nie jest na głównym wątku, przełączam na główny wątek");
            }

            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var page = GetCurrentPage();
                    _logger.LogDebug("Wyświetlam dialog wprowadzania na stronie typu: {PageType}", page.GetType().Name);
                    return await page.DisplayPromptAsync(title, message, "OK", "Anuluj", defaultValue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wyświetlania dialogu wprowadzania");
                    return null;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyświetlania pola wprowadzania. Message: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            return null;
        }
    }
} 