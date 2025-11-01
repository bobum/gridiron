using System.Linq;
using DomainObjects;
using Microsoft.Extensions.Logging;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            game.CurrentPlay = DetermineNextPlay(game);

            // Assign the game's logger to this play so it can log play-by-play results
            game.CurrentPlay.Result = game.Logger;

            //now that we know the kind of play that is being called,
            //we sub in the right players
            SubstituteOffensivePlayers(game);
            SubstituteDefensivePlayers(game);

            // Log the play call
            if (game.CurrentPlay.PlayType == PlayType.Kickoff)
            {
                game.CurrentPlay.Result.LogInformation("Players are lined up for the kickoff");
            }
            else if (game.CurrentPlay.PlayType == PlayType.Run)
            {
                game.CurrentPlay.Result.LogInformation("The big package is in, looks like a run formation");
            }
            else if (game.CurrentPlay.PlayType == PlayType.Pass)
            {
                game.CurrentPlay.Result.LogInformation("Receivers are spread wide, could be a passing down");
            }
        }

        //Eventually this method will be much more complex
        //coaches will determine the next play based on situation
        //there will need to be a call by the offense that will determine the play type,
        //and a guess by the defense as to what the offense is going to do
        //for now we will just randomly pick run or pass and let both teams line up based on that
        //there could also, before the snap, be a chance for audibles determined by the QB's intelligence
        //or the defense could call a blitz or coverage change based on the offensive formation
        //if the players were smart enough
        //all of that will be modeled in future iterations here
        private Play DetermineNextPlay(Game game)
        {
            var currentPlay = new Play();
            //if there are 0 plays - we have a new game
            if (game.Plays.Count == 0)
            {
                currentPlay.Possession = game.WonCoinToss;
                currentPlay.Down = Downs.None;
                currentPlay.StartTime = 0;
                currentPlay.PlayType = PlayType.Kickoff;
            }
            else
            {
                //possession for this play is whoever had it at the end of the last play
                currentPlay.Possession = game.Plays.Last().Possession;

                //totally random for now, but later will need to add logic for determining both
                //offensive and defensive play calls here
                //coaches will decide whether to run or pass based on down, distance, time remaining, score, etc.
                var kindOfPlay = _rng.NextDouble();

                //for now - a 50/50 shot of run or pass
                if (kindOfPlay <= 0.5)
                {
                    //run
                    currentPlay.PlayType = PlayType.Run;
                    currentPlay.ElapsedTime += 1.5;
                }
                else
                {
                    //pass
                    currentPlay.PlayType = PlayType.Pass;
                    currentPlay.ElapsedTime += 1.5;
                }
            }

            return currentPlay;
        }

        private void SubstituteDefensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;

            if (currentPlay.PlayType != PlayType.Run && currentPlay.PlayType != PlayType.Pass) return;

            Team defenseTeam = currentPlay.Possession == Possession.Home ? game.AwayTeam : game.HomeTeam;
            var chart = defenseTeam.DefenseDepthChart.Chart;
            var playersOnField = new List<Player>();

            // Linemen
            AddUniquePlayers(chart, Positions.DE, currentPlay.PlayType == PlayType.Run ? 2 : 1, playersOnField, "defense");
            AddUniquePlayers(chart, Positions.DT, 2, playersOnField, "defense");

            // Linebackers
            AddUniquePlayers(chart, Positions.LB, currentPlay.PlayType == PlayType.Run ? 3 : 4, playersOnField, "defense");

            // Defensive backs (fill remaining spots to reach 11)
            int remaining = 11 - playersOnField.Count;
            var dbs = new List<Player>();
            if (chart.TryGetValue(Positions.CB, out var cbs)) dbs.AddRange(cbs);
            if (chart.TryGetValue(Positions.S, out var ss)) dbs.AddRange(ss);
            if (chart.TryGetValue(Positions.FS, out var fss)) dbs.AddRange(fss);

            int dbAdded = 0;
            for (int i = 0; i < dbs.Count && dbAdded < remaining; i++)
            {
                var candidate = dbs[i];
                if (!playersOnField.Contains(candidate))
                {
                    playersOnField.Add(candidate);
                    dbAdded++;
                }
            }
            if (playersOnField.Count < 11)
                throw new InvalidOperationException("Unable to fill 11 unique defensive players on the field.");

            currentPlay.DefensePlayersOnField = playersOnField.Take(11).ToList();
            string json = JsonConvert.SerializeObject(currentPlay.DefensePlayersOnField);

        }

        private void SubstituteOffensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;

            if (currentPlay.PlayType != PlayType.Run && currentPlay.PlayType != PlayType.Pass) return;
                        
            Team offenseTeam = currentPlay.Possession == Possession.Home ? game.HomeTeam : game.AwayTeam;
            var chart = offenseTeam.OffenseDepthChart.Chart;
            var playersOnField = new List<Player>();

            // Always include 1 QB, 1 RB, 1 FB, 1 C, 2 G, 2 T
            AddUniquePlayers(chart, Positions.QB, 1, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.RB, 1, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.FB, 1, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.C, 1, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.G, 2, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.T, 2, playersOnField, "offense");

            // WR and TE selection based on play type
            AddUniquePlayers(chart, Positions.WR, currentPlay.PlayType == PlayType.Run ? 2 : 3, playersOnField, "offense");
            AddUniquePlayers(chart, Positions.TE, currentPlay.PlayType == PlayType.Run ? 1 : 0, playersOnField, "offense");

            if (playersOnField.Count < 11)
                throw new InvalidOperationException("Unable to fill 11 unique offensive players on the field.");

            currentPlay.OffensePlayersOnField = playersOnField.Take(11).ToList();
            string json = JsonConvert.SerializeObject(currentPlay.OffensePlayersOnField);


        }

        private void AddUniquePlayers(Dictionary<Positions, List<Player>> chart, Positions position, int needed, List<Player> playersOnField, string unitName)
        {
            if (chart.TryGetValue(position, out var depthList))
            {
                int added = 0;
                for (int i = 0; i < depthList.Count && added < needed; i++)
                {
                    var candidate = depthList[i];
                    if (!playersOnField.Contains(candidate))
                    {
                        playersOnField.Add(candidate);
                        added++;
                    }
                }
                if (added < needed)
                    throw new InvalidOperationException($"Not enough unique players for position {position} on {unitName}.");
            }
            else if (needed > 0)
            {
                throw new InvalidOperationException($"No depth chart for position {position} on {unitName}.");
            }
        }        
    }
}
