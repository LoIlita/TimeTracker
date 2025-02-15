using TimeTracker.UI.Views;
using Microsoft.Extensions.Logging;

namespace TimeTracker.UI;

public partial class AppShell : Shell
{
    private readonly ILogger<AppShell> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AppShell(IServiceProvider serviceProvider, ILogger<AppShell> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        InitializeComponent();

        try
        {
            _logger.LogInformation("Rejestracja tras nawigacyjnych");
            
            // Rejestracja tras nawigacyjnych
            Routing.RegisterRoute("sessionhistory", typeof(SessionHistoryView));
            Routing.RegisterRoute("statistics", typeof(StatisticsView));
            
            _logger.LogInformation("Trasy nawigacyjne zarejestrowane");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas rejestracji tras nawigacyjnych");
            throw;
        }
    }
} 