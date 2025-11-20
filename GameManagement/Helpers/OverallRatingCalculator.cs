using DomainObjects;

namespace GameManagement.Helpers;

/// <summary>
/// Helper class for calculating player overall ratings based on position
/// </summary>
public static class OverallRatingCalculator
{
    /// <summary>
    /// Calculates overall rating based on position-relevant attributes
    /// </summary>
    public static int Calculate(Player player)
    {
        return player.Position switch
        {
            // Quarterback: Passing, Awareness, Agility
            Positions.QB => (int)((player.Passing * 0.5) + (player.Awareness * 0.3) + (player.Agility * 0.2)),

            // Running Back: Rushing, Speed, Agility, Catching
            Positions.RB => (int)((player.Rushing * 0.4) + (player.Speed * 0.25) + (player.Agility * 0.2) + (player.Catching * 0.15)),

            // Fullback: Blocking, Strength, Rushing
            Positions.FB => (int)((player.Blocking * 0.5) + (player.Strength * 0.3) + (player.Rushing * 0.2)),

            // Wide Receiver: Catching, Speed, Agility
            Positions.WR => (int)((player.Catching * 0.5) + (player.Speed * 0.3) + (player.Agility * 0.2)),

            // Tight End: Catching, Blocking, Strength
            Positions.TE => (int)((player.Catching * 0.4) + (player.Blocking * 0.35) + (player.Strength * 0.25)),

            // Offensive Line (C, G, T): Blocking, Strength, Awareness
            Positions.C or Positions.G or Positions.T => 
                (int)((player.Blocking * 0.5) + (player.Strength * 0.3) + (player.Awareness * 0.2)),

            // Defensive Line (DE, DT): Tackling, Strength, Speed
            Positions.DE or Positions.DT => 
                (int)((player.Tackling * 0.4) + (player.Strength * 0.35) + (player.Speed * 0.25)),

            // Linebacker: Tackling, Coverage, Awareness
            Positions.LB or Positions.OLB => 
                (int)((player.Tackling * 0.4) + (player.Coverage * 0.3) + (player.Awareness * 0.3)),

            // Cornerback: Coverage, Speed, Agility
            Positions.CB => (int)((player.Coverage * 0.5) + (player.Speed * 0.3) + (player.Agility * 0.2)),

            // Safety: Coverage, Tackling, Awareness
            Positions.S or Positions.FS => 
                (int)((player.Coverage * 0.4) + (player.Tackling * 0.3) + (player.Awareness * 0.3)),

            // Kicker/Punter: Kicking primarily
            Positions.K or Positions.P => player.Kicking,

            // Long Snapper: Blocking, Awareness
            Positions.LS => (int)((player.Blocking * 0.6) + (player.Awareness * 0.4)),

            // Holder: Catching, Awareness (usually a backup QB or P)
            Positions.H => (int)((player.Catching * 0.5) + (player.Awareness * 0.5)),

            // Default
            _ => (player.Speed + player.Strength + player.Agility + player.Awareness) / 4
        };
    }

    /// <summary>
    /// Calculates salary based on overall rating and position market value
    /// </summary>
    public static int CalculateSalary(Player player, int overallRating)
    {
        // Base salary by position (in thousands)
        int positionBaseSalary = player.Position switch
        {
            Positions.QB => 8000,      // QBs are highest paid
            Positions.DE or Positions.DT or Positions.CB => 6000,  // Premium defensive positions
            Positions.WR or Positions.T => 5000,  // Premium skill/protection positions
            Positions.LB or Positions.S or Positions.TE => 4000,
            Positions.RB or Positions.G or Positions.C => 3000,
            Positions.K or Positions.P => 2000,
            _ => 1500
        };

        // Multiply by overall rating factor (60 rating = 0.6x, 90 rating = 1.5x)
        double ratingMultiplier = overallRating / 60.0;
        
        return (int)(positionBaseSalary * ratingMultiplier);
    }
}
