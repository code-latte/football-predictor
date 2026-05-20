namespace FootballCatch.Updater.Application.ApiFootball;

public interface IApiFootballClient
{
    Task<ApiFootballCompetition?> GetCompetitionAsync(int id, CancellationToken cancellationToken = default);
}
