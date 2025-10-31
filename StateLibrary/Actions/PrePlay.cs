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
            game.CurrentPlay = DetermineNextPlay(game);

            //now that we know the kind of play that is being called,
            //we sub in the right players
            SubstituteOffensivePlayers(game);
            SubstituteDefensivePlayers(game);
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
                currentPlay.Result.Add("Players are lined up for the kickoff");
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

            return currentPlay;
        }

        private void SubstituteDefensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;

            Team defenseTeam = currentPlay.Possession == Possession.Home ? game.AwayTeam : game.HomeTeam;
            var chart = defenseTeam.DefenseDepthChart.Chart;
            var playersOnField = new List<Player>();

            // Linemen
            int deCount = currentPlay.PlayType == PlayType.Run ? 2 : 1;
            int dtCount = currentPlay.PlayType == PlayType.Run ? 2 : 2; // 4 total for RUN, 3 total for PASS

            if (chart.TryGetValue(Positions.DE, out var des))
            {
                for (int i = 0; i < deCount && i < des.Count; i++)
                    playersOnField.Add(des[i]);
            }
            if (chart.TryGetValue(Positions.DT, out var dts))
            {
                int dtNeeded = currentPlay.PlayType == PlayType.Run ? 2 : 2; // 2 DTs for both
                for (int i = 0; i < dtNeeded && i < dts.Count; i++)
                    playersOnField.Add(dts[i]);
            }

            // Linebackers
            int lbCount = currentPlay.PlayType == PlayType.Run ? 3 : 4;
            if (chart.TryGetValue(Positions.LB, out var lbs))
            {
                for (int i = 0; i < lbCount && i < lbs.Count; i++)
                    playersOnField.Add(lbs[i]);
            }

            // Defensive backs (fill remaining spots to reach 11)
            int remaining = 11 - playersOnField.Count;
            var dbs = new List<Player>();
            if (chart.TryGetValue(Positions.CB, out var cbs))
                dbs.AddRange(cbs);
            if (chart.TryGetValue(Positions.S, out var ss))
                dbs.AddRange(ss);
            if (chart.TryGetValue(Positions.FS, out var fss))
                dbs.AddRange(fss);

            foreach (var db in dbs)
            {
                if (playersOnField.Count < 11)
                    playersOnField.Add(db);
                else
                    break;
            }

            // Ensure exactly 11 players
            currentPlay.DefensePlayersOnField = playersOnField.Take(11).ToList();
        }

        private void SubstituteOffensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;

            Team offenseTeam = currentPlay.Possession == Possession.Home ? game.HomeTeam : game.AwayTeam;
            var chart = offenseTeam.OffenseDepthChart.Chart;
            var playersOnField = new List<Player>();

            // Always include 1 QB, 1 RB, 1 FB, 1 C, 2 G, 2 T
            if (chart.TryGetValue(Positions.QB, out var qbs) && qbs.Count > 0)
                playersOnField.Add(qbs[0]);
            if (chart.TryGetValue(Positions.RB, out var rbs) && rbs.Count > 0)
                playersOnField.Add(rbs[0]);
            if (chart.TryGetValue(Positions.FB, out var fbs) && fbs.Count > 0)
                playersOnField.Add(fbs[0]);
            if (chart.TryGetValue(Positions.C, out var cs) && cs.Count > 0)
                playersOnField.Add(cs[0]);
            if (chart.TryGetValue(Positions.G, out var gs) && gs.Count > 1)
            {
                playersOnField.Add(gs[0]);
                playersOnField.Add(gs[1]);
            }
            if (chart.TryGetValue(Positions.T, out var ts) && ts.Count > 1)
            {
                playersOnField.Add(ts[0]);
                playersOnField.Add(ts[1]);
            }

            // WR and TE selection based on play type
            if (chart.TryGetValue(Positions.WR, out var wrs))
            {
                int wrCount = currentPlay.PlayType == PlayType.Run ? 2 : 3;
                for (int i = 0; i < wrCount && i < wrs.Count; i++)
                    playersOnField.Add(wrs[i]);
            }
            if (chart.TryGetValue(Positions.TE, out var tes))
            {
                int teCount = currentPlay.PlayType == PlayType.Run ? 1 : 0;
                for (int i = 0; i < teCount && i < tes.Count; i++)
                    playersOnField.Add(tes[i]);
            }

            // Ensure exactly 11 players
            currentPlay.OffensePlayersOnField = playersOnField.Take(11).ToList();
        }
    }
}
