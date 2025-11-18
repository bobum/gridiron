namespace DomainObjects.Helpers
{
    /// <summary>
    /// Helper class to convert absolute field position (0-100) to NFL-style field position notation.
    ///
    /// Internal representation:
    /// - 0 = Offense's own goal line
    /// - 50 = Midfield (50 yard line)
    /// - 100 = Opponent's goal line (offense trying to score here)
    ///
    /// NFL-style notation examples:
    /// - Position 20 → "Own 20" or "[TeamName] 20"
    /// - Position 60 → "Opp 40" or "[OpponentName] 40"
    /// - Position 50 → "50" (midfield)
    /// </summary>
    public static class FieldPositionHelper
    {
        /// <summary>
        /// Converts absolute field position to NFL-style yard line string
        /// </summary>
        /// <param name="fieldPosition">Absolute position (0-100)</param>
        /// <param name="offenseTeam">Team with possession (optional, for team name display)</param>
        /// <param name="defenseTeam">Team defending (optional, for team name display)</param>
        /// <returns>NFL-style field position string</returns>
        public static string FormatFieldPosition(int fieldPosition, Team? offenseTeam = null, Team? defenseTeam = null)
        {
            // Clamp to valid range
            if (fieldPosition < 0) fieldPosition = 0;
            if (fieldPosition > 100) fieldPosition = 100;

            // Midfield is always "50"
            if (fieldPosition == 50)
            {
                return "50";
            }

            // Own side of field (0-49)
            if (fieldPosition < 50)
            {
                if (offenseTeam != null)
                {
                    return $"{offenseTeam.City} {fieldPosition}";
                }
                else
                {
                    return $"Own {fieldPosition}";
                }
            }
            // Opponent's side of field (51-100)
            else
            {
                int yardsFromOpponentGoal = 100 - fieldPosition;

                if (defenseTeam != null)
                {
                    return $"{defenseTeam.City} {yardsFromOpponentGoal}";
                }
                else
                {
                    return $"Opp {yardsFromOpponentGoal}";
                }
            }
        }

        /// <summary>
        /// Gets a short description of field position with "yard line" suffix
        /// </summary>
        public static string FormatFieldPositionWithYardLine(int fieldPosition, Team? offenseTeam = null, Team? defenseTeam = null)
        {
            string position = FormatFieldPosition(fieldPosition, offenseTeam, defenseTeam);

            if (position == "50")
            {
                return "50 yard line (midfield)";
            }

            return $"{position} yard line";
        }

        /// <summary>
        /// Gets which side of the field the ball is on
        /// </summary>
        public static string GetFieldSide(int fieldPosition)
        {
            if (fieldPosition < 50) return "Own";
            if (fieldPosition > 50) return "Opponent";
            return "Midfield";
        }

        /// <summary>
        /// Gets the yard line number only (no team name)
        /// </summary>
        public static int GetYardLine(int fieldPosition)
        {
            if (fieldPosition <= 50)
            {
                return fieldPosition;
            }
            else
            {
                return 100 - fieldPosition;
            }
        }

        /// <summary>
        /// Checks if position is in the red zone (opponent's 20 or closer)
        /// </summary>
        public static bool IsInRedZone(int fieldPosition)
        {
            return fieldPosition >= 80; // 80-100 is opponent's 20-0
        }

        /// <summary>
        /// Checks if position is in goal-to-go situation (opponent's 10 or closer)
        /// </summary>
        public static bool IsGoalToGo(int fieldPosition)
        {
            return fieldPosition >= 90; // 90-100 is opponent's 10-0
        }
    }
}
