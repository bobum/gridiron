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

            // Check for touchdown or two-point conversion
            if (newFieldPosition >= 100)
            {
                play.IsTouchdown = true;
                play.EndFieldPosition = 100;
                game.FieldPosition = 100;

                // Update score using centralized method
                if (play.IsTwoPointConversion)
                {
                    game.AddTwoPointConversion(play.Possession);
                }
                else
                {
                    game.AddTouchdown(play.Possession);
                }

                // After touchdown/2pt conversion, possession changes (kickoff will follow)
                play.PossessionChange = true;
            }
            else if (newFieldPosition <= 0)
            {
                // Safety - QB sacked in own end zone
                play.EndFieldPosition = 0;
                game.FieldPosition = 0;

                // Award 2 points to defense using centralized method
                var defendingTeam = play.Possession == Possession.Home ? Possession.Away : Possession.Home;
                game.AddSafety(defendingTeam);

                play.PossessionChange = true;
            }
            else
            {
                // Normal play result
                play.EndFieldPosition = newFieldPosition;
                game.FieldPosition = newFieldPosition;

                // Two-point conversion failed - possession changes
                if (play.IsTwoPointConversion)
                {
                    play.PossessionChange = true;
                    play.Result.LogInformation($"Two-point conversion FAILED. No points scored.");
                }
                // Check for first down
                else if (play.YardsGained >= game.YardsToGo)
                {
                    // First down!
                    game.CurrentDown = Downs.First;
                    game.YardsToGo = 10;
                    play.Result.LogInformation($"First down! Ball at the {game.FieldPosition} yard line.");
                }
                else
                {
                    // Advance the down
                    var nextDown = AdvanceDown(game.CurrentDown);
                    game.YardsToGo -= play.YardsGained;

                    if (nextDown == Downs.None)
                    {
                        // Turnover on downs - other team gets 1st and 10
                        play.PossessionChange = true;
                        game.CurrentDown = Downs.First;
                        game.YardsToGo = 10;
                        play.Result.LogInformation($"Turnover on downs! Ball at the {game.FieldPosition} yard line.");
                    }
                    else
                    {
                        game.CurrentDown = nextDown;

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