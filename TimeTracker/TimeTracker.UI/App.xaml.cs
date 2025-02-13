using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using TimeTracker.UI.Views;
using Microsoft.Extensions.Logging;

namespace TimeTracker.UI;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<App> _logger;

	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = serviceProvider.GetRequiredService<ILogger<App>>();
		
		InitializeComponent();

		try
		{
			_logger.LogDebug("Inicjalizacja aplikacji...");
			MainPage = _serviceProvider.GetRequiredService<AppShell>();
			_logger.LogDebug("Skonfigurowano AppShell");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Błąd podczas inicjalizacji aplikacji");
			throw;
		}

#if DEBUG
		Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
		{
#if WINDOWS
			var nativeWindow = handler.PlatformView;
			nativeWindow.Activate();
#endif
		});
#endif
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);

		// Ustawiamy minimalny rozmiar okna
		window.MinimumWidth = 800;
		window.MinimumHeight = 600;

		return window;
	}
}