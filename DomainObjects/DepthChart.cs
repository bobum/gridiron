using System.Collections.Generic;

namespace DomainObjects
{
    public class DepthChart
    {
        // For each position, a list of players ordered by depth (starter first)
        public Dictionary<Positions, List<Player>> Chart { get; set; } = new();
    }
}
