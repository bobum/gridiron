using System.Linq;
using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.SkillsCheckResults
{
    public class PenaltySkillsCheckResult : IGameAction
    {
        private readonly Penalty _penalty;

        public PenaltySkillsCheckResult(Penalty penalty)
        {
            _penalty = penalty;
        }

        public void Execute(Game game, ILogger logger)
        {
            //once we've determined that a penalty has been called on a player in the PenaltySkillsCheck,
            //here we will determine what penalty it was that was called on that player and add that to the game object
            switch (game.CurrentPlay.PlayType)
            {
                case PlayType.Kickoff:
                    _penalty.Name = Penalties.List.Single(p =>
                        p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Name;
                    break;
                case PlayType.FieldGoal:
                    _penalty.Name = Penalties.List.Single(p =>
                        p.Name == PenaltyNames.RoughingtheKicker).Name;
                    break;
                case PlayType.Pass:
                    _penalty.Name = Penalties.List.Single(p =>
                        p.Name == PenaltyNames.OffensiveHolding).Name;
                    break;
                case PlayType.Punt:
                    _penalty.Name = Penalties.List.Single(p =>
                        p.Name == PenaltyNames.RoughingtheKicker).Name;
                    break;
                case PlayType.Run:
                    _penalty.Name = Penalties.List.Single(p =>
                        p.Name == PenaltyNames.OffensiveHolding).Name;
                    break;
            }

            game.CurrentPlay.Penalties.Add(_penalty);
            logger.LogInformation("Flag on the play");
        }
    }
}
