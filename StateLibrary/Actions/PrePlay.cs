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

            // Log the current game situation
            var downText = game.CurrentDown switch
            {
                Downs.First => "1st",
                Downs.Second => "2nd",
                Downs.Third => "3rd",
                Downs.Fourth => "4th",
                _ => "Kickoff"
            };
            var fieldPos = game.FormatFieldPosition(game.CurrentPlay.Possession);
            if (game.CurrentDown == Downs.None)
            {
                game.Logger.LogInformation($"Kickoff at {fieldPos}");
            }
            else
            {
                game.Logger.LogInformation($"{downText} & {game.YardsToGo} at {fieldPos}");
            }

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
        private IPlay DetermineNextPlay(Game game)
        {
            IPlay currentPlay;

            //if there are 0 plays - we have a new game
            if (game.Plays.Count == 0)
            {
                currentPlay = new KickoffPlay
                {
                    Possession = game.WonCoinToss,
                    Down = Downs.None,
                    StartTime = 0
                };
            }
            else
            {
                var lastPlay = game.Plays.Last();

                // Determine possession for this play
                var possession = lastPlay.PossessionChange
                    ? FlipPossession(lastPlay.Possession)
                    : lastPlay.Possession;

                // Set down and yards to go for this play
                // If possession changed (turnover, touchdown, etc.), reset to 1st and 10
                if (lastPlay.PossessionChange)
                {
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                    // Note: Field position is an absolute value (0-100 on field)
                    // It does NOT flip on possession changes
                    // Display helpers (FormatFieldPosition) interpret it based on possession
                }
                // Otherwise, down and yards were already set by PlayResult

                //totally random for now, but later will need to add logic for determining both
                //offensive and defensive play calls here
                //coaches will decide whether to run or pass based on down, distance, time remaining, score, etc.
                var kindOfPlay = _rng.NextDouble();

                //for now - a 50/50 shot of run or pass
                if (kindOfPlay <= 0.5)
                {
                    //run
                    currentPlay = new RunPlay
                    {
                        Possession = possession,
                        ElapsedTime = 1.5,
                        Down = game.CurrentDown
                    };
                }
                else
                {
                    //pass
                    currentPlay = new PassPlay
                    {
                        Possession = possession,
                        ElapsedTime = 1.5,
                        Down = game.CurrentDown
                    };
                }
            }

            return currentPlay;
        }

        private Possession FlipPossession(Possession currentPossession)
        {
            return currentPossession == Possession.Home ? Possession.Away : Possession.Home;
        }

        private void SubstituteDefensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;
            Team defenseTeam = currentPlay.Possession == Possession.Home ? game.AwayTeam : game.HomeTeam;
            var playersOnField = new List<Player>();

            // Handle special teams plays
            if (currentPlay.PlayType == PlayType.Kickoff)
            {
                var chart = defenseTeam.KickoffDefenseDepthChart.Chart;
                SubstituteKickoffDefense(chart, playersOnField);
            }
            else if (currentPlay.PlayType == PlayType.Punt)
            {
                var chart = defenseTeam.PuntDefenseDepthChart.Chart;
                SubstitutePuntDefense(chart, playersOnField);
            }
            else if (currentPlay.PlayType == PlayType.FieldGoal)
            {
                var chart = defenseTeam.FieldGoalDefenseDepthChart.Chart;
                SubstituteFieldGoalDefense(chart, playersOnField);
            }
            else if (currentPlay.PlayType == PlayType.Run || currentPlay.PlayType == PlayType.Pass)
            {
                // Regular defense
                var chart = defenseTeam.DefenseDepthChart.Chart;

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
            }
            else
            {
                return; // Unknown play type
            }

            if (playersOnField.Count < 11)
                throw new InvalidOperationException("Unable to fill 11 unique defensive players on the field.");

            currentPlay.DefensePlayersOnField = playersOnField.Take(11).ToList();
            string json = JsonConvert.SerializeObject(currentPlay.DefensePlayersOnField);
        }

        private void SubstituteKickoffDefense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField)
        {
            // Kickoff return team: Fast returners and blockers (flexible)
            TryAddUniquePlayers(chart, Positions.WR, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.RB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.CB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.S, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.LB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.TE, 1, playersOnField);

            // Fill any remaining spots
            FillRemainingPlayers(chart, playersOnField, 11);
        }

        private void SubstitutePuntDefense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField)
        {
            // Punt return team: Returner and block/coverage players (flexible)
            TryAddUniquePlayers(chart, Positions.WR, 3, playersOnField);
            TryAddUniquePlayers(chart, Positions.CB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.S, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.LB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.RB, 1, playersOnField);
            TryAddUniquePlayers(chart, Positions.DE, 1, playersOnField);

            // Fill any remaining spots
            FillRemainingPlayers(chart, playersOnField, 11);
        }

        private void SubstituteFieldGoalDefense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField)
        {
            // Field goal block unit: DL for rush, DBs for fake coverage (flexible)
            TryAddUniquePlayers(chart, Positions.DE, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.DT, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.LB, 3, playersOnField);
            TryAddUniquePlayers(chart, Positions.CB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.S, 2, playersOnField);

            // Fill any remaining spots
            FillRemainingPlayers(chart, playersOnField, 11);
        }

        private void SubstituteOffensivePlayers(Game game)
        {
            var currentPlay = game.CurrentPlay;
            Team offenseTeam = currentPlay.Possession == Possession.Home ? game.HomeTeam : game.AwayTeam;
            var playersOnField = new List<Player>();

            // Handle special teams plays
            if (currentPlay.PlayType == PlayType.Kickoff)
            {
                var chart = offenseTeam.KickoffOffenseDepthChart.Chart;
                SubstituteKickoffOffense(chart, playersOnField, currentPlay as KickoffPlay);
            }
            else if (currentPlay.PlayType == PlayType.Punt)
            {
                var chart = offenseTeam.PuntOffenseDepthChart.Chart;
                SubstitutePuntOffense(chart, playersOnField, currentPlay as PuntPlay);
            }
            else if (currentPlay.PlayType == PlayType.FieldGoal)
            {
                var chart = offenseTeam.FieldGoalOffenseDepthChart.Chart;
                SubstituteFieldGoalOffense(chart, playersOnField, currentPlay as FieldGoalPlay);
            }
            else if (currentPlay.PlayType == PlayType.Run || currentPlay.PlayType == PlayType.Pass)
            {
                // Regular offense
                var chart = offenseTeam.OffenseDepthChart.Chart;

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
            }
            else
            {
                return; // Unknown play type
            }

            if (playersOnField.Count < 11)
                throw new InvalidOperationException("Unable to fill 11 unique offensive players on the field.");

            currentPlay.OffensePlayersOnField = playersOnField.Take(11).ToList();
            string json = JsonConvert.SerializeObject(currentPlay.OffensePlayersOnField);
        }

        private void SubstituteKickoffOffense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField, KickoffPlay? play)
        {
            // Kickoff team needs: K, mix of LB/CB/S for coverage
            // Must have a kicker - use P as fallback
            Player? kicker = null;
            if (chart.TryGetValue(Positions.K, out var kickers) && kickers.Count > 0)
            {
                kicker = kickers[0];
                playersOnField.Add(kicker);
            }
            else if (chart.TryGetValue(Positions.P, out var punters) && punters.Count > 0)
            {
                kicker = punters[0];
                playersOnField.Add(kicker);
            }

            // Assign the kicker to the play object so execution knows who kicks
            if (play != null && kicker != null)
            {
                play.Kicker = kicker;
            }

            // Fill remaining 10 spots with fast coverage players (flexible)
            TryAddUniquePlayers(chart, Positions.CB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.S, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.LB, 3, playersOnField);
            TryAddUniquePlayers(chart, Positions.WR, 3, playersOnField);

            // Fill any remaining spots with whatever is available
            FillRemainingPlayers(chart, playersOnField, 11);
        }

        private void SubstitutePuntOffense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField, PuntPlay? play)
        {
            // Punt team needs: P, LS (long snapper), coverage players
            Player? punter = null;
            if (chart.TryGetValue(Positions.P, out var punters) && punters.Count > 0)
            {
                punter = punters[0];
                playersOnField.Add(punter);
            }

            // Assign the punter to the play object so execution knows who punts
            if (play != null && punter != null)
            {
                play.Punter = punter;
            }

            TryAddUniquePlayers(chart, Positions.LS, 1, playersOnField); // Long snapper

            // Fill remaining spots with coverage team (flexible)
            TryAddUniquePlayers(chart, Positions.LB, 3, playersOnField);
            TryAddUniquePlayers(chart, Positions.CB, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.S, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.WR, 2, playersOnField);

            // Fill any remaining spots
            FillRemainingPlayers(chart, playersOnField, 11);
        }

        private void SubstituteFieldGoalOffense(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField, FieldGoalPlay? play)
        {
            // Field goal unit needs: K, holder (H), LS (long snapper), linemen to protect
            Player? kicker = null;
            if (chart.TryGetValue(Positions.K, out var kickers) && kickers.Count > 0)
            {
                kicker = kickers[0];
                playersOnField.Add(kicker);
            }

            Player? holder = null;
            if (chart.TryGetValue(Positions.H, out var holders) && holders.Count > 0)
            {
                holder = holders[0];
                if (!playersOnField.Contains(holder))
                {
                    playersOnField.Add(holder);
                }
            }

            // Assign kicker and holder to the play object so execution knows who does what
            if (play != null)
            {
                if (kicker != null) play.Kicker = kicker;
                if (holder != null) play.Holder = holder;
            }

            TryAddUniquePlayers(chart, Positions.LS, 1, playersOnField); // Long snapper

            // Protection (flexible)
            TryAddUniquePlayers(chart, Positions.G, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.T, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.TE, 2, playersOnField);
            TryAddUniquePlayers(chart, Positions.FB, 2, playersOnField);

            // Fill any remaining spots
            FillRemainingPlayers(chart, playersOnField, 11);
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

        /// <summary>
        /// Tries to add unique players from a position. Returns true if at least one was added.
        /// Does not throw if fewer than needed are available.
        /// </summary>
        private bool TryAddUniquePlayers(Dictionary<Positions, List<Player>> chart, Positions position, int needed, List<Player> playersOnField)
        {
            if (!chart.TryGetValue(position, out var depthList))
                return false;

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

            return added > 0;
        }

        /// <summary>
        /// Fills remaining player slots with any available unique players from the depth chart
        /// </summary>
        private void FillRemainingPlayers(Dictionary<Positions, List<Player>> chart, List<Player> playersOnField, int targetCount)
        {
            if (playersOnField.Count >= targetCount)
                return;

            // Iterate through all positions in the depth chart
            foreach (var kvp in chart)
            {
                foreach (var player in kvp.Value)
                {
                    if (!playersOnField.Contains(player))
                    {
                        playersOnField.Add(player);
                        if (playersOnField.Count >= targetCount)
                            return;
                    }
                }
            }
        }
    }
}
