using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public sealed class HalfExpireCheck : IGameAction
    {
        public void Execute(Game game)
        {
            switch (game.CurrentQuarter.QuarterType)
            {
                case DomainObjects.Time.QuarterType.First:
                case DomainObjects.Time.QuarterType.Second:
                    game.CurrentHalf = game.Halves[0];
                    break;
                case DomainObjects.Time.QuarterType.Third:
                case DomainObjects.Time.QuarterType.Fourth:
                    game.CurrentHalf = game.Halves[1];
                    //TODO check if tied & move to OT
                    break;
                case DomainObjects.Time.QuarterType.Overtime:
                    //TODO check if tied & move to another OT
                    break;
                default:
                    break;
            }
        }
    }
}
