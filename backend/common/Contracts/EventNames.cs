namespace FootballCatch.Common.Contracts;

/// <summary>
/// Strongly-typed registry of every RabbitMQ integration event name used across the FootballCatch platform.
/// Use these constants instead of raw string literals when publishing or subscribing to events so that
/// typos are caught at compile time and event names can be discovered via IDE tooling.
/// </summary>
/// <remarks>
/// One nested static class per emitting service. Each constant holds the full, lowercase dot-separated
/// event name including its version suffix (e.g. <c>"prediction.submitted.v1"</c>).
/// When a new event version is introduced, add a new constant — never change an existing one.
/// </remarks>
public static class EventNames
{
    /// <summary>Events emitted by the <b>auth</b> service (identity bounded context).</summary>
    public static class Auth
    {
        /// <summary><c>identity.user.registered.v1</c></summary>
        public const string UserRegistered = "identity.user.registered.v1";

        /// <summary><c>identity.user.disabled.v1</c></summary>
        public const string UserDisabled = "identity.user.disabled.v1";

        /// <summary><c>identity.deviceToken.added.v1</c></summary>
        public const string DeviceTokenAdded = "identity.deviceToken.added.v1";
    }

    /// <summary>Events emitted by the <b>user-profile</b> service.</summary>
    public static class UserProfile
    {
        /// <summary><c>profile.updated.v1</c></summary>
        public const string Updated = "profile.updated.v1";
    }

    /// <summary>Events emitted by the <b>catalog</b> service (competitions and teams).</summary>
    public static class Catalog
    {
        /// <summary><c>competition.created.v1</c></summary>
        public const string CompetitionCreated = "competition.created.v1";

        /// <summary><c>competition.updated.v1</c></summary>
        public const string CompetitionUpdated = "competition.updated.v1";

        /// <summary><c>team.upserted.v1</c></summary>
        public const string TeamUpserted = "team.upserted.v1";
    }

    /// <summary>Events emitted by the <b>fixtures</b> service (match calendar, live scores, results).</summary>
    public static class Fixtures
    {
        /// <summary><c>match.upserted.v1</c></summary>
        public const string MatchUpserted = "match.upserted.v1";

        /// <summary><c>match.kickoff.v1</c></summary>
        public const string Kickoff = "match.kickoff.v1";

        /// <summary><c>match.scoreChanged.v1</c></summary>
        public const string ScoreChanged = "match.scoreChanged.v1";

        /// <summary><c>match.finalized.v1</c></summary>
        public const string Finalized = "match.finalized.v1";
    }

    /// <summary>Events emitted by the <b>predictions</b> service.</summary>
    public static class Predictions
    {
        /// <summary><c>prediction.submitted.v1</c></summary>
        public const string Submitted = "prediction.submitted.v1";

        /// <summary><c>prediction.locked.v1</c></summary>
        public const string Locked = "prediction.locked.v1";

        /// <summary><c>prediction.replaced.v1</c></summary>
        public const string Replaced = "prediction.replaced.v1";
    }

    /// <summary>Events emitted by the <b>scoring-engine</b> service.</summary>
    public static class ScoringEngine
    {
        /// <summary><c>scoring.userScoreUpdated.v1</c></summary>
        public const string UserScoreUpdated = "scoring.userScoreUpdated.v1";

        /// <summary><c>scoring.matchScored.v1</c></summary>
        public const string MatchScored = "scoring.matchScored.v1";
    }

    /// <summary>Events emitted by the <b>leagues</b> service.</summary>
    public static class Leagues
    {
        /// <summary><c>league.created.v1</c></summary>
        public const string Created = "league.created.v1";

        /// <summary><c>league.joined.v1</c></summary>
        public const string Joined = "league.joined.v1";

        /// <summary><c>league.left.v1</c></summary>
        public const string Left = "league.left.v1";

        /// <summary><c>league.updated.v1</c></summary>
        public const string Updated = "league.updated.v1";

        /// <summary><c>league.tableUpdated.v1</c></summary>
        public const string TableUpdated = "league.tableUpdated.v1";
    }

    /// <summary>Events emitted by the <b>stats</b> service (global leaderboard and KPIs).</summary>
    public static class Stats
    {
        /// <summary><c>leaderboard.updated.v1</c></summary>
        public const string LeaderboardUpdated = "leaderboard.updated.v1";

        /// <summary><c>leaderboard.kpi.updated.v1</c></summary>
        public const string KpiUpdated = "leaderboard.kpi.updated.v1";
    }

    /// <summary>Events emitted by the <b>notifications</b> service (push and email).</summary>
    public static class Notifications
    {
        /// <summary><c>notification.sent.v1</c></summary>
        public const string Sent = "notification.sent.v1";
    }
}
