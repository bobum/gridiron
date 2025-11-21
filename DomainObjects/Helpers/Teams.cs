using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DomainObjects;

namespace DomainObjects.Helpers
{
    /// <summary>
    /// Container for home and visitor teams with automatic depth chart building
    /// </summary>
    public class Teams
    {
        public Team HomeTeam { get; set; }
        public Team VisitorTeam { get; set; }

        // Constructor for database-loaded or test teams
        public Teams(Team homeTeam, Team awayTeam)
        {
            HomeTeam = homeTeam;
            VisitorTeam = awayTeam;

            // Build depth charts for both teams using centralized builder
            DepthChartBuilder.AssignAllDepthCharts(HomeTeam);
            DepthChartBuilder.AssignAllDepthCharts(VisitorTeam);
        }
    }
}
