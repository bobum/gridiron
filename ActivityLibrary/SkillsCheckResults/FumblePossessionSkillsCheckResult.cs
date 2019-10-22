using DomainObjects;
using System.Activities;

namespace ActivityLibrary.SkillsCheckResults
{
    //this class will look a the two teams and when there was a fumble
    //determine possession based on the skills of the players
    //and the type of current play - eventually
    //for now we'll do it randomly...
    public sealed class FumblePossessionSkillsCheckResult : CodeActivity<Possession>
    {
        public InArgument<Game> Game { get; set; }

        protected override Possession Execute(CodeActivityContext context)
        {
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            var possession = toss == 1 ? Possession.Away : Possession.Home;

            return possession;
        }
    }
}