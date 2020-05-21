using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public sealed class QuarterExpireCheck : IGameAction
    {
        public void Execute(Game game)
        {
            //remove the current play elapsed time and 
            game.CurrentQuarter.TimeRemaining -= (int)game.CurrentPlay.ElapsedTime;
            
            //see if we need to advance to the next quarter
            if(game.CurrentQuarter.TimeRemaining == 0)
            {
                switch (game.CurrentQuarter.QuarterType)
                {
                    case DomainObjects.Time.QuarterType.First:
                        game.CurrentQuarter = game.Halves[0].Quarters[1];
                        break;
                    case DomainObjects.Time.QuarterType.Second:
                        game.CurrentQuarter = game.Halves[1].Quarters[0];
                        break;
                    case DomainObjects.Time.QuarterType.Third:
                        game.CurrentQuarter = game.Halves[1].Quarters[1];
                        break;
                    case DomainObjects.Time.QuarterType.Fourth:
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
}