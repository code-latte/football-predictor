using System.ComponentModel.DataAnnotations;

namespace FootballCatch.Updater.Infrastructure.ApiFootball;

public sealed class ApiFootballOptions
{
    public const string SectionName = "ApiFootball";

    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    public int RetryCount { get; init; } = 3;
    public double BaseDelaySeconds { get; init; } = 1.0;
    public int[] ActiveCompetitions { get; init; } = Array.Empty<int>();
}
