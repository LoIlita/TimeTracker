using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using TimeTracker.Application;
using TimeTracker.Infrastructure.Data;
using TimeTracker.UI.Services;
using TimeTracker.UI.ViewModels;
using TimeTracker.UI.Views;
using CommunityToolkit.Maui;
using TimeTracker.Application.Services;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Mapping;
using TimeTracker.UI.Converters;
using TimeTracker.Infrastructure;

namespace TimeTracker.UI;

public static class MauiProgram
{
	public static async Task<MauiApp> CreateMauiApp()
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("[APP] Rozpoczęcie inicjalizacji aplikacji");
			
			var builder = MauiApp.CreateBuilder();

			// Podstawowa konfiguracja MAUI
			builder.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

			// Konfiguracja logowania
			builder.Logging.ClearProviders();
			builder.Logging.AddDebug();
#if DEBUG
			builder.Logging.SetMinimumLevel(LogLevel.Information);
#else
			builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

			// Konfiguracja bazy danych
			var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeTracker.db");
			System.Diagnostics.Debug.WriteLine($"[APP] Ścieżka do bazy danych: {dbPath}");

			var dbDirectory = Path.GetDirectoryName(dbPath);
			if (!string.IsNullOrEmpty(dbDirectory))
			{
				Directory.CreateDirectory(dbDirectory);
			}

			// Upewnij się, że plik bazy danych jest dostępny
			if (File.Exists(dbPath))
			{
				try
				{
					// Sprawdź, czy możemy otworzyć plik
					using (var fs = File.Open(dbPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
						fs.Close();
					}
				}
				catch (IOException)
				{
					System.Diagnostics.Debug.WriteLine("[APP] Baza danych jest zablokowana, próba usunięcia pliku...");
					try
					{
						File.Delete(dbPath);
						System.Diagnostics.Debug.WriteLine("[APP] Stary plik bazy danych został usunięty");
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"[APP] Nie można usunąć pliku bazy danych: {ex.Message}");
						throw;
					}
				}
			}

			// Rejestracja serwisów
			builder.Services.AddDatabase($"Data Source={dbPath};Mode=ReadWriteCreate;Cache=Shared");
			builder.Services.AddApplication();

			// Rejestracja AutoMapper
			builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

			// Rejestracja konwerterów
			builder.Services.AddSingleton<InverseBoolConverter>();
			builder.Services.AddSingleton<TimeSpanConverter>();

			// Serwisy UI
			builder.Services.AddSingleton<ITimerService, TimerService>();
			builder.Services.AddSingleton<IDialogService, DialogService>();
			
			// Rejestracja serwisów dla predefiniowanych sesji
			builder.Services.AddScoped<IPredefinedSessionRepository, PredefinedSessionRepository>();
			builder.Services.AddScoped<IPredefinedSessionService, PredefinedSessionService>();

			// Rejestracja App i widoków
			builder.Services.AddTransient<App>();
			builder.Services.AddTransient<AppShell>();
			builder.Services.AddTransient<MainPage>();
			builder.Services.AddTransient<WorkTrackerViewModel>();

			// Rejestracja komponentów dla historii sesji
			builder.Services.AddTransient<SessionHistoryView>();
			builder.Services.AddTransient<SessionHistoryViewModel>();

			// Serwisy aplikacyjne
			builder.Services.AddScoped<IWorkSessionService, WorkSessionService>();
			builder.Services.AddScoped<IWorkSessionReader>(sp => sp.GetRequiredService<IWorkSessionService>());
			builder.Services.AddScoped<IWorkSessionWriter>(sp => sp.GetRequiredService<IWorkSessionService>());

			// Dodaj infrastrukturę
			builder.Services.AddInfrastructure();

			System.Diagnostics.Debug.WriteLine("[APP] Budowanie aplikacji...");
			var app = builder.Build();

			// Inicjalizacja LoggerProvider
			LoggerProvider.Initialize(app.Services.GetRequiredService<ILoggerFactory>());

			// Inicjalizacja bazy danych
			System.Diagnostics.Debug.WriteLine("[APP] Inicjalizacja bazy danych...");
			using (var scope = app.Services.CreateScope())
			{
				try
				{
					var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
					
					System.Diagnostics.Debug.WriteLine("[APP] Tworzenie/aktualizacja bazy danych...");
					await context.Database.EnsureCreatedAsync();
					System.Diagnostics.Debug.WriteLine("[APP] Baza danych została utworzona/zaktualizowana");

					// Inicjalizacja bazy danych
					await DbConfiguration.InitializeDatabaseAsync(scope.ServiceProvider);

					// Test połączenia
					await using (var connection = context.Database.GetDbConnection())
					{
						await connection.OpenAsync();
						System.Diagnostics.Debug.WriteLine("[APP] Test połączenia z bazą danych zakończony pomyślnie");
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[APP] Błąd podczas inicjalizacji bazy danych: {ex}");
					throw;
				}
			}

			System.Diagnostics.Debug.WriteLine("[APP] Inicjalizacja zakończona pomyślnie");
			return app;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[APP] Błąd podczas inicjalizacji: {ex}");
			throw;
		}
	}
}
