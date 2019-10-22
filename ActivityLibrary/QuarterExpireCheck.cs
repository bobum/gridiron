using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class QuarterExpireCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

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
                        //TODO check if tied & moive to OT
                        break;
                    case DomainObjects.Time.QuarterType.Overtime:
                        //TODO check if tied & moive to another OT
                        break;
                    default:
                        break;
                }
            }

            return game;
        }
    }
}