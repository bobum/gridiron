using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Interfaces;
using StateLibrary.SkillsCheckResults;
using StateLibrary.SkillsChecks;

namespace StateLibrary.BaseClasses
{
    public abstract class PlayBookend :  IGameAction
    {
        protected readonly ICryptoRandom _rng;

        public abstract void Execute(Game game);

        protected PlayBookend(ICryptoRandom rng)
        {
            _rng = rng;
        }

        protected void PenaltyCheck(PenaltyOccuredWhen penaltyOccuredWhen, Game game)
        {
            var penaltyOccurredSkillsCheck = new PenaltyOccurredSkillsCheck(penaltyOccuredWhen, _rng);
            penaltyOccurredSkillsCheck.Execute(game);

            if (penaltyOccurredSkillsCheck.Occurred)
            {
                var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penaltyOccurredSkillsCheck.Penalty);
                penaltySkillsCheckResult.Execute(game);
            }
        }
    }
}