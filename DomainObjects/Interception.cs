namespace DomainObjects
{
    public class Interception
    {
        public Player InterceptedBy { get; set; }
        public Player ThrownBy { get; set; }
        public int InterceptionYardLine { get; set; }
        public int ReturnYards { get; set; }
        public bool FumbledDuringReturn { get; set; }
        public Player? RecoveredBy { get; set; }
    }
}