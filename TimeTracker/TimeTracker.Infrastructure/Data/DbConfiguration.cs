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
} 