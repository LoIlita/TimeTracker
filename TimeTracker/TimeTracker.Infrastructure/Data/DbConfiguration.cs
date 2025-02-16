using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Repositories;
using System.Linq;

namespace TimeTracker.Infrastructure.Data;

public static class DbConfiguration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        // Dodanie kontekstu bazy danych
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            // Włączamy szczegółowe logowanie dla debugowania
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(message => System.Diagnostics.Debug.WriteLine($"[EF Core] {message}"));
        });

        // Rejestracja repozytorium
        services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            System.Diagnostics.Debug.WriteLine("[DB] Sprawdzanie bazy danych...");
            
            // Zawsze tworzymy bazę danych, jeśli nie istnieje
            await context.Database.EnsureCreatedAsync();
            
            // Sprawdzamy połączenie
            if (await context.Database.CanConnectAsync())
            {
                System.Diagnostics.Debug.WriteLine("[DB] Połączono z bazą danych");
                
                // Sprawdzamy, czy tabele są puste
                var hasAnyProjects = await context.Projects.AnyAsync();
                var hasAnySessions = await context.WorkSessions.AnyAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DB] Projekty: {(hasAnyProjects ? "istnieją" : "brak")}");
                System.Diagnostics.Debug.WriteLine($"[DB] Sesje: {(hasAnySessions ? "istnieją" : "brak")}");
            }
            else
            {
                throw new Exception("Nie można połączyć się z bazą danych po jej utworzeniu");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] BŁĄD: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public static async Task CreateBackupAsync(string dbPath)
    {
        try
        {
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Nie znaleziono pliku bazy danych", dbPath);
            }

            var backupDirectory = Path.Combine(
                Path.GetDirectoryName(dbPath)!,
                "Backups"
            );

            // Tworzenie katalogu kopii zapasowych, jeśli nie istnieje
            Directory.CreateDirectory(backupDirectory);

            // Tworzenie nazwy pliku kopii zapasowej z datą i czasem
            var backupFileName = $"TimeTracker_backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
            var backupPath = Path.Combine(backupDirectory, backupFileName);

            // Kopiowanie pliku bazy danych
            await Task.Run(() => File.Copy(dbPath, backupPath, true));

            System.Diagnostics.Debug.WriteLine($"[DB] Utworzono kopię zapasową w: {backupPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] Błąd podczas tworzenia kopii zapasowej: {ex.Message}");
            throw;
        }
    }

    public static async Task RestoreFromBackupAsync(string backupPath, string currentDbPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Nie znaleziono pliku kopii zapasowej", backupPath);
            }

            // Zatrzymujemy wszystkie połączenia z bazą danych
            using (var fs = File.Open(currentDbPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                fs.Close();
            }

            // Przywracamy kopię zapasową
            await Task.Run(() => File.Copy(backupPath, currentDbPath, true));

            System.Diagnostics.Debug.WriteLine($"[DB] Przywrócono bazę danych z kopii: {backupPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] Błąd podczas przywracania kopii zapasowej: {ex.Message}");
            throw;
        }
    }

    public static string[] GetAvailableBackups(string dbPath)
    {
        var backupDirectory = Path.Combine(
            Path.GetDirectoryName(dbPath)!,
            "Backups"
        );

        if (!Directory.Exists(backupDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(backupDirectory, "TimeTracker_backup_*.db")
                       .OrderByDescending(f => f)
                       .ToArray();
    }
} 