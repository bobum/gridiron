using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DomainObjects;

namespace DomainObjects.Helpers
{
    public class Teams
    {
        public Team HomeTeam { get; set; }
        public Team VisitorTeam { get; set; }

        // Constructor for database-loaded or test teams
        public Teams(Team homeTeam, Team awayTeam)
        {
            HomeTeam = homeTeam;
            VisitorTeam = awayTeam;

            // Build depth charts for both teams
            HomeTeam.OffenseDepthChart = BuildOffenseDepthChart(HomeTeam.Players);
            HomeTeam.DefenseDepthChart = BuildDefenseDepthChart(HomeTeam.Players);
            HomeTeam.FieldGoalOffenseDepthChart = BuildFieldGoalOffenseDepthChart(HomeTeam.Players);
            HomeTeam.FieldGoalDefenseDepthChart = BuildFieldGoalDefenseDepthChart(HomeTeam.Players);
            HomeTeam.KickoffOffenseDepthChart = BuildKickoffOffenseDepthChart(HomeTeam.Players);
            HomeTeam.KickoffDefenseDepthChart = BuildKickoffDefenseDepthChart(HomeTeam.Players);
            HomeTeam.PuntOffenseDepthChart = BuildPuntOffenseDepthChart(HomeTeam.Players);
            HomeTeam.PuntDefenseDepthChart = BuildPuntDefenseDepthChart(HomeTeam.Players);

            VisitorTeam.OffenseDepthChart = BuildOffenseDepthChart(VisitorTeam.Players);
            VisitorTeam.DefenseDepthChart = BuildDefenseDepthChart(VisitorTeam.Players);
            VisitorTeam.FieldGoalOffenseDepthChart = BuildFieldGoalOffenseDepthChart(VisitorTeam.Players);
            VisitorTeam.FieldGoalDefenseDepthChart = BuildFieldGoalDefenseDepthChart(VisitorTeam.Players);
            VisitorTeam.KickoffOffenseDepthChart = BuildKickoffOffenseDepthChart(VisitorTeam.Players);
            VisitorTeam.KickoffDefenseDepthChart = BuildKickoffDefenseDepthChart(VisitorTeam.Players);
            VisitorTeam.PuntOffenseDepthChart = BuildPuntOffenseDepthChart(VisitorTeam.Players);
            VisitorTeam.PuntDefenseDepthChart = BuildPuntDefenseDepthChart(VisitorTeam.Players);
        }
        
        private int GetPositionSkill(Player p, Positions pos)
        {
            // Use the most relevant skill for each position
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

        // Helper methods for building depth charts

        private DepthChart BuildOffenseDepthChart(List<Player> players)
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

        private DepthChart BuildDefenseDepthChart(List<Player> players)
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

        private DepthChart BuildFieldGoalOffenseDepthChart(List<Player> players)
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

        private DepthChart BuildFieldGoalDefenseDepthChart(List<Player> players)
        {
            var chart = new DepthChart();
            chart.Chart[Positions.DE] = GetDepth(players, Positions.DE, 2);
            chart.Chart[Positions.DT] = GetDepth(players, Positions.DT, 2);
            chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 3);
            chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
            chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
            return chart;
        }

        private DepthChart BuildKickoffOffenseDepthChart(List<Player> players)
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

        private DepthChart BuildKickoffDefenseDepthChart(List<Player> players)
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

        private DepthChart BuildPuntOffenseDepthChart(List<Player> players)
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

        private DepthChart BuildPuntDefenseDepthChart(List<Player> players)
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


        private List<Player> GetDepth(List<Player> players, Positions pos, int depth = 1)
        {
            // Order players by their skill for the position, take up to 'depth' players
            return players
                .Where(p => p.Position == pos)
                .OrderByDescending(p => GetPositionSkill(p, pos))
                .Take(depth)
                .ToList();
        }
    }
}

