namespace DomainObjects
{
    public class Game : SoftDeletableEntity
    {
        public int Id { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

        public Team HomeTeam { get; set; } = null!;

        public Team AwayTeam { get; set; } = null!;

        public int? RandomSeed { get; set; }

        public PlayByPlay? PlayByPlay { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        /// <summary>
        /// Gets or sets foreign key to the season week this game belongs to (nullable for standalone games).
        /// </summary>
        public int? SeasonWeekId { get; set; }

        /// <summary>
        /// Gets or sets navigation property to the season week.
        /// </summary>
        public SeasonWeek? SeasonWeek { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether this game has been completed (score is final).
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Gets or sets date and time when the game was played.
        /// </summary>
        public DateTime? PlayedAt { get; set; }
    }

    public enum Positions
    {
        QB,
        C,
        G,
        T,
        TE,
        WR,
        RB,
        DT,
        DE,
        LB,
        OLB,
        CB,
        S,
        K,
        P,
        FB,
        FS,
        LS,
        H
    }

    public enum Downs
    {
        First,
        Second,
        Third,
        Fourth,
        None
    }

    public enum Possession
    {
        None,
        Home,
        Away
    }

    public enum PlayType
    {
        Kickoff,
        FieldGoal,
        Punt,
        Pass,
        Run
    }
}
