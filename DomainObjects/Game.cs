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
    }

    public enum Positions
    {
        QB, C, G, T, TE, WR, RB, DT, DE, LB, OLB, CB, S, K, P, FB, FS, LS, H
    }

    public enum Downs
    {
        First, Second, Third, Fourth, None
    }

    public enum Possession
    {
        None, Home, Away
    }

    public enum PlayType
    {
        Kickoff, FieldGoal, Punt, Pass, Run
    }
}
