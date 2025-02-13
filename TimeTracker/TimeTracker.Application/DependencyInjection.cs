using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Application.Services;
using TimeTracker.Application.Mapping;

namespace TimeTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Dodaj AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // Zarejestruj serwisy
        services.AddScoped<IWorkSessionService, WorkSessionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IPredefinedSessionService, PredefinedSessionService>();

        return services;
    }
} 