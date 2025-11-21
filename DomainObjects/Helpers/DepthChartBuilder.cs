using System.Collections.Generic;
using System.Linq;

namespace DomainObjects.Helpers
{
    /// <summary>
    /// Static helper class for building team depth charts.
    /// Single source of truth for depth chart logic used by game simulation and team management.
    /// </summary>
    public static class DepthChartBuilder
    {
        /// <summary>
        /// Assigns all 8 depth charts to a team based on their roster
        /// </summary>
        /// <param name="team">Team to assign depth charts for</param>
        public static void AssignAllDepthCharts(Team team)
        {
            if (team?.Players == null) return;

            team.OffenseDepthChart = BuildOffenseDepthChart(team.Players);
            team.DefenseDepthChart = BuildDefenseDepthChart(team.Players);
            team.FieldGoalOffenseDepthChart = BuildFieldGoalOffenseDepthChart(team.Players);
            team.FieldGoalDefenseDepthChart = BuildFieldGoalDefenseDepthChart(team.Players);
            team.KickoffOffenseDepthChart = BuildKickoffOffenseDepthChart(team.Players);
            team.KickoffDefenseDepthChart = BuildKickoffDefenseDepthChart(team.Players);
            team.PuntOffenseDepthChart = BuildPuntOffenseDepthChart(team.Players);
            team.PuntDefenseDepthChart = BuildPuntDefenseDepthChart(team.Players);
        }

        /// <summary>
        /// Builds standard offense depth chart (11 positions)
        /// </summary>
        public static DepthChart BuildOffenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.QB] = GetDepth(players, Positions.QB);
            chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
            chart.Chart[Positions.FB] = GetDepth(players, Positions.FB);
            chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 3);
            chart.Chart[Positions.TE] = GetDepth(players, Positions.TE);
            chart.Chart[Positions.C] = GetDepth(players, Positions.C);
            chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);
            chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);
            return chart;
        }

        /// <summary>
        /// Builds standard defense depth chart (11 positions)
        /// </summary>
        public static DepthChart BuildDefenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.DE] = GetDepth(players, Positions.DE, 2);
            chart.Chart[Positions.DT] = GetDepth(players, Positions.DT, 2);
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 4);
            chart.Chart[Positions.OLB] = GetDepth(players, Positions.OLB, 2);
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
            chart.Chart[Positions.FS] = GetDepth(players, Positions.FS, 1);
            return chart;
        }

        /// <summary>
        /// Builds field goal offense depth chart (kicker, holder, protection)
        /// </summary>
        public static DepthChart BuildFieldGoalOffenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.K] = GetDepth(players, Positions.K, 1);      // Kicker
            chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);    // Long Snapper
            chart.Chart[Positions.H] = GetDepth(players, Positions.QB, 1);     // Holder (often a backup QB or P)
            chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);      // Guard blocking
            chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);      // Tackle blocking
            chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 2);    // Tight end blocking
            chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);    // Running back protection
            return chart;
        }

        /// <summary>
        /// Builds field goal defense depth chart (block attempt)
        /// </summary>
        public static DepthChart BuildFieldGoalDefenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.DE] = GetDepth(players, Positions.DE, 2);
            chart.Chart[Positions.DT] = GetDepth(players, Positions.DT, 2);
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 3);
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
            return chart;
        }

        /// <summary>
        /// Builds kickoff offense depth chart (kicking team)
        /// </summary>
        public static DepthChart BuildKickoffOffenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.K] = GetDepth(players, Positions.K, 1);      // Kicker
            chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);    // Long Snapper
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);    // Coverage
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);    // Coverage
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);      // Coverage
            chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);    // Speed
            chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);    // Speed
            return chart;
        }

        /// <summary>
        /// Builds kickoff defense depth chart (return team)
        /// </summary>
        public static DepthChart BuildKickoffDefenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);    // Returners
            chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);    // Returners
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);    // Blocking
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);      // Blocking
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);    // Blocking
            chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 1);    // Blocking
            chart.Chart[Positions.G] = GetDepth(players, Positions.G, 1);      // Blocking
            chart.Chart[Positions.FB] = GetDepth(players, Positions.FB, 1);    // Blocking
            return chart;
        }

        /// <summary>
        /// Builds punt offense depth chart (punting team)
        /// </summary>
        public static DepthChart BuildPuntOffenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.P] = GetDepth(players, Positions.P, 1);      // Punter
            chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);    // Long Snapper
            chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);      // Blocking
            chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);      // Blocking
            chart.Chart[Positions.C] = GetDepth(players, Positions.C, 1);      // Center
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);    // Coverage
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);    // Coverage
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);      // Coverage
            return chart;
        }

        /// <summary>
        /// Builds punt defense depth chart (return team)
        /// </summary>
        public static DepthChart BuildPuntDefenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);    // Returners
            chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);    // Returners
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);    // Blocking
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);      // Blocking
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);    // Blocking
            chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 1);    // Blocking
            chart.Chart[Positions.G] = GetDepth(players, Positions.G, 1);      // Blocking
            chart.Chart[Positions.FB] = GetDepth(players, Positions.FB, 1);    // Blocking
            return chart;
        }

        // Private helper methods

        /// <summary>
        /// Gets the most relevant skill for a player at a given position
        /// </summary>
        private static int GetPositionSkill(Player p, Positions pos)
        {
            return pos switch
            {
                Positions.QB => p.Passing,
                Positions.RB => p.Rushing,
                Positions.FB => p.Blocking,
                Positions.WR => p.Catching,
                Positions.TE => p.Catching + p.Blocking,
                Positions.C => p.Blocking,
                Positions.G => p.Blocking,
                Positions.T => p.Blocking,
                Positions.DE => p.Tackling + p.Agility,
                Positions.DT => p.Tackling + p.Strength,
                Positions.LB => p.Tackling + p.Coverage,
                Positions.CB => p.Coverage + p.Speed,
                Positions.S => p.Coverage + p.Tackling,
                Positions.FS => p.Coverage + p.Tackling,
                Positions.K => p.Kicking,
                Positions.P => p.Kicking,
                Positions.LS => p.Blocking,
                _ => 0
            };
        }

        /// <summary>
        /// Gets the top players at a position, ordered by skill
        /// </summary>
        private static List<Player> GetDepth(List<Player> players, Positions pos, int depth = 1)
        {
            return players
                .Where(p => p.Position == pos)
                .OrderByDescending(p => GetPositionSkill(p, pos))
                .Take(depth)
                .ToList();
        }
    }
}
