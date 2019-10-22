using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class HalfExpireCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            switch (game.CurrentQuarter.QuarterType)
            {
                case DomainObjects.Time.QuarterType.First:
                case DomainObjects.Time.QuarterType.Second:
                    game.CurrentHalf = game.Halves[0];
                    break;
                case DomainObjects.Time.QuarterType.Third:
                case DomainObjects.Time.QuarterType.Fourth:
                    game.CurrentHalf = game.Halves[1];
                    //TODO check if tied & moive to OT
                    break;
                case DomainObjects.Time.QuarterType.Overtime:
                    //TODO check if tied & moive to another OT
                    break;
                default:
                    break;
            }

            return game;
        }
    }
}
