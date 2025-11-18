using System;
using System.Collections.Generic;

namespace DomainObjects.Helpers
{
    /// <summary>
    /// Helper methods for Player initialization and management.
    /// </summary>
    public static class PlayerHelper
    {
        /// <summary>
        /// Initializes Discipline attribute for players based on position and experience.
        /// Discipline affects penalty frequency - higher values result in fewer penalties.
        ///
        /// Discipline ranges by position:
        /// - QB: 75-95 (need to protect the ball, make smart decisions)
        /// - OL: 70-90 (veterans commit fewer holding penalties)
        /// - Skill positions (WR/RB/TE): 65-85
        /// - DL/LB: 60-80 (aggressive play can lead to more penalties)
        /// - DB: 55-75 (coverage penalties common, especially for young players)
        /// - K/P: 80-95 (rarely commit penalties)
        ///
        /// Experience modifier: +1 per year of experience (max +10)
        /// </summary>
        public static void InitializeDiscipline(Player player)
        {
            if (player == null) return;

            // Base discipline by position
            int baseDiscipline = player.Position switch
            {
                Positions.QB => 85,
                Positions.K => 90,
                Positions.P => 90,
                Positions.LS => 85,
                Positions.C => 80,
                Positions.G => 80,
                Positions.T => 80,
                Positions.TE => 75,
                Positions.WR => 75,
                Positions.RB => 75,
                Positions.FB => 75,
                Positions.DT => 70,
                Positions.DE => 70,
                Positions.LB => 70,
                Positions.OLB => 70,
                Positions.CB => 65,
                Positions.S => 65,
                Positions.FS => 65,
                _ => 70
            };

            // Add experience modifier (1 point per year, max 10)
            int experienceBonus = Math.Min(player.Exp, 10);

            // Add some random variation (-5 to +5)
            var random = new Random(player.Number + player.Age); // Seed based on player attributes for consistency
            int variation = random.Next(-5, 6);

            // Calculate final discipline (capped at 0-100)
            player.Discipline = Math.Clamp(baseDiscipline + experienceBonus + variation, 0, 100);
        }

        /// <summary>
        /// Initializes Discipline for all players on a team.
        /// </summary>
        public static void InitializeTeamDiscipline(Team team)
        {
            if (team?.Players == null) return;

            foreach (var player in team.Players)
            {
                InitializeDiscipline(player);
            }
        }

        /// <summary>
        /// Initializes Discipline for all players in a list.
        /// </summary>
        public static void InitializePlayersDiscipline(List<Player> players)
        {
            if (players == null) return;

            foreach (var player in players)
            {
                InitializeDiscipline(player);
            }
        }
    }
}
