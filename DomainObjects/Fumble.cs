namespace DomainObjects
{
    public class Fumble
    {
        public Player FumbledBy { get; set; }
        public Player RecoveredBy { get; set; }
    }
}