using Games.Application;
using Games.Application.ChallengeOverview;
using Games.Application.Challenges;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallengeLifecycle;
using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Games.Application.LocationChallengeSolving;
using Games.Application.NearbyChallenges;
using Games.Application.QuizAttempts;
using Games.Application.QuizLifecycle;
using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Games.Application.QuizSolving;
using Games.Application.SocialChallengeLifecycle;
using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallengeRuns;
using Games.Application.SocialChallenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Application.SocialChallengeSolving;
using Games.Domain.QuizAttempts;
using Games.Domain.SocialChallengeRuns;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.Challenges;
using Games.Infrastructure.Persistence.LocationChallengeAttempts;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizAttempts;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallengeRuns;
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

        AddRepositories(services);
        AddServices(services);
        AddQueries(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<ILocationChallengeAttemptRepository, LocationChallengeAttemptRepository>();
        services.AddScoped<ISocialChallengeRunRepository, SocialChallengeRunRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<IChallengeReadRepository, ChallengeReadRepository>();
        services.AddScoped<INearbyChallengeReadRepository, NearbyChallengeReadRepository>();
        services.AddScoped<IQuizAttemptReadRepository, QuizAttemptReadRepository>();
        services.AddScoped<ILocationChallengeAttemptReadRepository, LocationChallengeAttemptReadRepository>();
        services.AddScoped<ISocialChallengeRunReadRepository, SocialChallengeRunReadRepository>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<QuizSolver>();
        services.AddScoped<LocationChallengeSolver>();
        services.AddScoped<SocialChallengeSolver>();
        services.AddScoped<SocialChallengeManagementService>();
        services.AddScoped<SocialChallengeLifecycleService>();
        services.AddScoped<QuizManagementService>();
        services.AddScoped<QuizLifecycleService>();
        services.AddScoped<LocationChallengeManagementService>();
        services.AddScoped<LocationChallengeLifecycleService>();
        services.AddScoped<QuizSolvingService>();
        services.AddScoped<LocationChallengeSolvingService>();
        services.AddScoped<SocialChallengeSolvingService>();
    }

    private static void AddQueries(IServiceCollection services)
    {
        services.AddScoped<QuizManagementQueries>();
        services.AddScoped<LocationChallengeManagementQueries>();
        services.AddScoped<ChallengeOverviewQueries>();
        services.AddScoped<NearbyChallengesQueries>();
        services.AddScoped<QuizSolvingQueries>();
        services.AddScoped<LocationChallengeSolvingQueries>();
        services.AddScoped<SocialChallengeSolvingQueries>();
    }
}
