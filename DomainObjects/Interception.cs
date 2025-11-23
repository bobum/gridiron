namespace DomainObjects
{
    public class Interception
    {
        public required Player InterceptedBy { get; set; }
        public required Player ThrownBy { get; set; }
        public int InterceptionYardLine { get; set; }
        public int ReturnYards { get; set; }
        public bool FumbledDuringReturn { get; set; }
        public Player? RecoveredBy { get; set; }
    }
}