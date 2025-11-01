using DomainObjects;
using Microsoft.Extensions.Logging;
using DomainObjects.Time;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions.EventChecks
{
    public sealed class QuarterExpireCheck : IGameAction
    {
        public void Execute(Game game)
        {
            //remove the current play elapsed time from the current quarter
            game.CurrentQuarter.TimeRemaining -= (int)game.CurrentPlay.ElapsedTime;

            //see if we need to advance to the next quarter
            if (game.CurrentQuarter.TimeRemaining == 0)
            {
                game.CurrentPlay.Result.LogInformation($"last play of the {game.CurrentQuarter.QuarterType} quarter");
                game.CurrentPlay.QuarterExpired = true;

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
                        game.CurrentQuarter.QuarterType = QuarterType.GameOver;
                        break;
                    case DomainObjects.Time.QuarterType.Overtime:
                        //TODO check if tied & move to another OT
                        break;
                }
            }
        }
    }
}