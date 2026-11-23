using Exploration.Application;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.TourExecution;
using Exploration.Application.TourLocationSearchRecording;
using Exploration.Application.TourLocationSearchRecords;
using Exploration.Application.TourPreferenceSetting;
using Exploration.Application.TourPreferences;
using Exploration.Application.TourPublishing;
using Exploration.Application.TourSessions;
using Exploration.Application.Tours;
using Exploration.Contracts;
using Exploration.Domain.TourSessions;
using Exploration.Infrastructure.Persistence;
using Exploration.Infrastructure.Persistence.TourLocationSearchRecords;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.TourSessions;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExplorationDbContext>(configuration, "exploration");
        services.AddHostedService<ExplorationModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<ExplorationMapperProfile>());

        AddRepositories(services);
        AddServices(services);
        AddQueries(services);

        services.AddScoped<IExplorationApi, ExplorationApi>();

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ExplorationDbContext>());
        services.AddScoped<ITourReadRepository, TourReadRepository>();
        services.AddScoped<ITourPreferenceRepository, TourPreferenceRepository>();
        services.AddScoped<ITourPreferenceReadRepository, TourPreferenceReadRepository>();
        services.AddScoped<ITourLocationSearchRecordRepository, TourLocationSearchRecordRepository>();
        services.AddScoped<ITourSessionRepository, TourSessionRepository>();
        services.AddScoped<ITourSessionReadRepository, TourSessionReadRepository>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<TourStarting>();
        services.AddScoped<KeyPointArrival>();
        services.AddScoped<TourAuthoringService>();
        services.AddScoped<TourPublishingService>();
        services.AddScoped<KeyPointAuthoringService>();
        services.AddScoped<TourPreferenceSettingService>();
        services.AddScoped<TourLocationSearchRecordingService>();
        services.AddScoped<TourExecutionService>();
    }

    private static void AddQueries(IServiceCollection services)
    {
        services.AddScoped<TourAuthoringQueries>();
        services.AddScoped<KeyPointAuthoringQueries>();
        services.AddScoped<TourBrowsingQueries>();
        services.AddScoped<TourPreferenceSettingQueries>();
        services.AddScoped<TourExecutionQueries>();
    }
}
