using System.Net;
using FootballCatch.Updater.Application.ApiFootball;
using FootballCatch.Updater.Infrastructure.ApiFootball;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace FootballCatch.Updater.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUpdaterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ApiFootballOptions>()
            .Bind(configuration.GetSection(ApiFootballOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<IApiFootballClient, ApiFootballClient>((sp, client) =>
            {
                ApiFootballOptions options = sp.GetRequiredService<IOptions<ApiFootballOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("x-apisports-key", options.ApiKey);
            })
            .AddResilienceHandler("ApiFootball-retry", (builder, context) =>
            {
                ApiFootballOptions options = context.ServiceProvider.GetRequiredService<IOptions<ApiFootballOptions>>().Value;

                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = options.RetryCount,
                    Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(static response =>
                            (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout)
                });
            });

        return services;
    }
}
