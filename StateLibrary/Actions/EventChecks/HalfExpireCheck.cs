using DomainObjects;
using Microsoft.Extensions.Logging;
using DomainObjects.Time;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions.EventChecks
{
    public sealed class HalfExpireCheck : IGameAction
    {
        public void Execute(Game game, ILogger logger)
        {
            if (game.CurrentPlay.QuarterExpired)
            {
                switch (game.CurrentQuarter.QuarterType)
                {
                    case QuarterType.Third:
                        logger.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf = game.Halves[1];
                        break;
                    case QuarterType.GameOver:
                        logger.LogInformation($"last play of the {game.CurrentHalf.HalfType} half");
                        game.CurrentPlay.HalfExpired = true;
                        game.CurrentHalf.HalfType = HalfType.GameOver;
                        break;
                }
            }

            //TODO check if tied & move to OT
            //TODO check if tied & move to another OT
        }
    }
}
