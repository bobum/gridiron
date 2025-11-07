using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;
using System.Linq;

namespace StateLibrary.PlayResults
{
    public class PassResult : IGameAction
    {
        public void Execute(Game game)
        {
            var play = (PassPlay)game.CurrentPlay;

            // Set start field position
            play.StartFieldPosition = game.FieldPosition;

            // Calculate new field position
            var newFieldPosition = game.FieldPosition + play.YardsGained;

            // Check for touchdown
            if (newFieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                // Update score
                if (play.Possession == Possession.Home)
                {
                    game.HomeScore += 6;
                    play.Result.LogInformation($"TOUCHDOWN! Home team scores! Home {game.HomeScore}, Away {game.AwayScore}");
                }
                else
                {
                    game.AwayScore += 6;
                    play.Result.LogInformation($"TOUCHDOWN! Away team scores! Home {game.HomeScore}, Away {game.AwayScore}");
                }

                // After touchdown, possession changes (kickoff will follow)
                play.PossessionChange = true;
            }
            else if (newFieldPosition <= 0)
            {
                // Safety - QB sacked in own end zone
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Award 2 points to defense
                if (play.Possession == Possession.Home)
                {
                    game.AwayScore += 2;
                    play.Result.LogInformation($"SAFETY! Away team gets 2 points! Home {game.HomeScore}, Away {game.AwayScore}");
                }
                else
                {
                    game.HomeScore += 2;
                    play.Result.LogInformation($"SAFETY! Home team gets 2 points! Home {game.HomeScore}, Away {game.AwayScore}");
                }

                play.PossessionChange = true;
            }
            else
            {
                // Normal play result
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;

                // Check for first down
                if (play.YardsGained >= game.YardsToGo)
                {
                    // First down!
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                    play.Result.LogInformation($"First down! Ball at the {game.FieldPosition} yard line.");
                }
                else
                {
                    // Advance the down
                    game.CurrentDown = AdvanceDown(game.CurrentDown);
                    game.YardsToGo -= play.YardsGained;

                    if (game.CurrentDown == Downs.None)
                    {
                        // Turnover on downs
                        play.PossessionChange = true;
                        play.Result.LogInformation($"Turnover on downs! Ball at the {game.FieldPosition} yard line.");
                    }
                    else
                    {
                        // Check if this was an incomplete pass or sack
                        var lastSegment = play.PassSegments.LastOrDefault();
                        if (lastSegment != null && !lastSegment.IsComplete && play.YardsGained >= 0)
                        {
                            // Incomplete pass
                            play.Result.LogInformation($"Incomplete pass. {FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FieldPosition}.");
                        }
                        else
                        {
                            // Completed pass or sack
                            play.Result.LogInformation($"{FormatDown(game.CurrentDown)} and {game.YardsToGo} at the {game.FieldPosition}.");
                        }
                    }
                }
            }

            // Log final passer and receiver stats
            var lastPassSegment = play.PassSegments.LastOrDefault();
            if (lastPassSegment != null && lastPassSegment.Passer != null)
            {
                var passer = lastPassSegment.Passer;
                var receiver = lastPassSegment.Receiver;

                var completions = play.PassSegments.Count(s => s.IsComplete);
                var attempts = play.PassSegments.Count;
                var totalYards = play.YardsGained;

                if (lastPassSegment.IsComplete && receiver != null)
                {
                    play.Result.LogInformation($"{passer.LastName}: {completions}/{attempts} for {totalYards} yards to {receiver.LastName}.");
                }
                else if (play.YardsGained < 0)
                {
                    // Sack
                    play.Result.LogInformation($"{passer.LastName}: Sacked for {Math.Abs(totalYards)} yards.");
                }
                else
                {
                    // Incomplete
                    play.Result.LogInformation($"{passer.LastName}: {completions}/{attempts} passing.");
                }
            }
        }

        private Downs AdvanceDown(Downs currentDown)
        {
            return currentDown switch
            {
                Downs.First => Downs.Second,
                Downs.Second => Downs.Third,
                Downs.Third => Downs.Fourth,
                Downs.Fourth => Downs.None, // Turnover on downs
                _ => Downs.None
            };
        }

        private string FormatDown(Downs down)
        {
            return down switch
            {
                Downs.First => "1st",
                Downs.Second => "2nd",
                Downs.Third => "3rd",
                Downs.Fourth => "4th",
                _ => "?"
            };
        }
    }
}