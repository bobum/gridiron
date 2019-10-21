using DomainObjects;
using System.Activities;

namespace ActivityLibrary.SkillsCheckResult
{
    //this class will look a the two teams and when there was a fumble
    //determine possiesion based on the skills of the players
    //and the type of current play - eventually
    //for now we'll do it randomly...
    public sealed class FumblePossesionSkillsCheckResult : CodeActivity<Posession>
    {
        public InArgument<Game> Game { get; set; }

        protected override Posession Execute(CodeActivityContext context)
        {
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            var currentPosession = toss == 1 ? Posession.Away : Posession.Home;

            return currentPosession;
        }
    }
}