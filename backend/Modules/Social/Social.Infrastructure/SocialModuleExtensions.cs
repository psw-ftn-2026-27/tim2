using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Social.Application;
using Social.Application.BlogAuthoring;
using Social.Application.BlogCommenting;
using Social.Application.BlogReading;
using Social.Application.Blogs;
using Social.Application.ClubAuthoring;
using Social.Application.Clubs;
using Social.Application.FollowRequestSending;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.FollowRequests;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SocialDbContext>(configuration, "social");
        services.AddHostedService<SocialModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<SocialMapperProfile>());

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IBlogReadRepository, BlogReadRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubReadRepository, ClubReadRepository>();
        services.AddScoped<IFollowRequestRepository, FollowRequestRepository>();
        services.AddScoped<IFollowRequestReadRepository, FollowRequestReadRepository>();

        services.AddScoped<FollowRequestService>();

        services.AddScoped<BlogAuthoringService>();
        services.AddScoped<BlogAuthoringQueries>();
        services.AddScoped<BlogReadingQueries>();
        services.AddScoped<BlogCommentingService>();
        services.AddScoped<ClubAuthoringService>();
        services.AddScoped<ClubAuthoringQueries>();
        services.AddScoped<FollowRequestSendingService>();
        services.AddScoped<FollowRequestSendingQueries>();

        return services;
    }
}
