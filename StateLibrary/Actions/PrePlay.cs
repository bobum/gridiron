using System.Linq;
using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class PrePlay : IGameAction
    {
        private ISeedableRandom _rng;
        public PrePlay(ISeedableRandom rng)
        {
            _rng = rng;
        }
        public void Execute(Game game)
        {
            //consider this class, the huddle
            //inside here we will do things like decide the next play,
            //substitute players for the new play,
            //substitute for players that have been injured in the post play
            var currentPlay = new Play();

            //if there are 0 plays - we have a new game
            if (game.Plays.Count == 0)
            {
                currentPlay.Possession = game.WonCoinToss;
                currentPlay.Down = Downs.None;
                currentPlay.StartTime = 0;
                currentPlay.PlayType = PlayType.Kickoff;
                currentPlay.Result.Add("Players are lined up for the kickoff");
            }
            else
            {
                //possession for this play is whoever had it at the end of the last play
                currentPlay.Possession = game.Plays.Last().Possession;

                var kindOfPlay = _rng.NextDouble();

                //for now - a 50/50 shot of run or pass
                if (kindOfPlay <= 0.5)
                {
                    //run
                    currentPlay.PlayType = PlayType.Run;
                    currentPlay.ElapsedTime += 1.5;
                    currentPlay.Result.Add("The big package is in, looks like a run formation");
                }
                else
                {
                    //pass
                    currentPlay.PlayType = PlayType.Pass;
                    currentPlay.ElapsedTime += 1.5;
                    currentPlay.Result.Add("Receivers are spread wide, could be a passing down");
                }
            }

            game.CurrentPlay = currentPlay;
        }
    }
}
