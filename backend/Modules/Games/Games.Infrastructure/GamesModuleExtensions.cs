using Games.Application;
using Games.Application.ChallengeOverview;
using Games.Application.Challenges;
using Games.Application.LocationChallengeLifecycle;
using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Games.Application.QuizLifecycle;
using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeLifecycle;
using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.Challenges;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<IChallengeReadRepository, ChallengeReadRepository>();

        services.AddScoped<SocialChallengeManagementService>();
        services.AddScoped<SocialChallengeLifecycleService>();
        services.AddScoped<QuizManagementService>();
        services.AddScoped<QuizManagementQueries>();
        services.AddScoped<QuizLifecycleService>();
        services.AddScoped<LocationChallengeManagementService>();
        services.AddScoped<LocationChallengeManagementQueries>();
        services.AddScoped<LocationChallengeLifecycleService>();
        services.AddScoped<ChallengeOverviewQueries>();

        return services;
    }
}
