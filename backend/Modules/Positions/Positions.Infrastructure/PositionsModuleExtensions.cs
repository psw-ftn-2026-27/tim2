using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Positions.Application;
using Positions.Application.PositionSimulating;
using Positions.Application.UserPositions;
using Positions.Contracts;
using Positions.Infrastructure.Persistence;
using Positions.Infrastructure.Persistence.UserPositions;
using Shared.Infrastructure;

namespace Positions.Infrastructure;

public static class PositionsModuleExtensions
{
    public static IServiceCollection AddPositionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PositionsDbContext>(configuration, "positions");
        services.AddHostedService<PositionsModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PositionsMapperProfile>());

        services.AddScoped<IUserPositionRepository, UserPositionRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PositionsDbContext>());
        services.AddScoped<IUserPositionReadRepository, UserPositionReadRepository>();

        services.AddScoped<PositionSimulatingService>();
        services.AddScoped<PositionSimulatingQueries>();
        services.AddScoped<IPositionsApi, PositionsApi>();

        return services;
    }
}
