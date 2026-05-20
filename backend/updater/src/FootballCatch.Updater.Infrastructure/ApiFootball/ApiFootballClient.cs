using FootballCatch.Updater.Application.ApiFootball;

namespace FootballCatch.Updater.Infrastructure.ApiFootball;

public sealed class ApiFootballClient : IApiFootballClient
{
    private readonly HttpClient _httpClient;

    public ApiFootballClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task<ApiFootballCompetition?> GetCompetitionAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
