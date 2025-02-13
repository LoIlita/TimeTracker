using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Repositories;

namespace TimeTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Rejestracja repozytoriów
        services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPredefinedSessionRepository, PredefinedSessionRepository>();

        return services;
    }
} 