namespace DomainObjects.Helpers
{
    /// <summary>
    /// Helper class to convert absolute field position (0-100) to NFL-style field position notation.
    ///
    /// IMPORTANT: Field position is ABSOLUTE (does not flip on possession changes)
    /// - 0 = Home team's goal line
    /// - 50 = Midfield (50 yard line)
    /// - 100 = Away team's goal line
    ///
    /// Examples with Home=Buffalo, Away=Kansas City:
    /// - Position 20, any possession → "Buffalo 20" (in Buffalo's territory)
    /// - Position 80, any possession → "Kansas City 20" (in Kansas City's territory)
    /// - Position 50 → "50" (midfield)
    /// </summary>
    public static class FieldPositionHelper
    {
        /// <summary>
        /// Converts absolute field position to NFL-style yard line string
        /// </summary>
        /// <param name="fieldPosition">Absolute position (0-100, where 0=Home goal, 100=Away goal)</param>
        /// <param name="homeTeam">Home team</param>
        /// <param name="awayTeam">Away team</param>
        /// <returns>NFL-style field position string</returns>
        public static string FormatFieldPosition(int fieldPosition, Team? homeTeam, Team? awayTeam)
        {
            // Clamp to valid range
            if (fieldPosition < 0) fieldPosition = 0;
            if (fieldPosition > 100) fieldPosition = 100;

            // Midfield is always "50"
            if (fieldPosition == 50)
            {
                return "50";
            }

            // Position 0-49: In Home team's territory
            if (fieldPosition < 50)
            {
                if (homeTeam != null)
                {
                    return $"{homeTeam.City} {fieldPosition}";
                }
                else
                {
                    return $"{fieldPosition}";
                }
            }
            // Position 51-100: In Away team's territory
            else
            {
                int yardsFromAwayGoal = 100 - fieldPosition;

                if (awayTeam != null)
                {
                    return $"{awayTeam.City} {yardsFromAwayGoal}";
                }
                else
                {
                    return $"{yardsFromAwayGoal}";
                }
            }
        }

        /// <summary>
        /// Gets a short description of field position with "yard line" suffix
        /// </summary>
        public static string FormatFieldPositionWithYardLine(int fieldPosition, Team? homeTeam, Team? awayTeam)
        {
            string position = FormatFieldPosition(fieldPosition, homeTeam, awayTeam);

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
        /// Checks if position is in goal-to-go situation (when the first down marker would be in the end zone)
        /// </summary>
        /// <param name="fieldPosition">Current field position (0-100)</param>
        /// <param name="yardsToGo">Yards needed for first down</param>
        /// <returns>True if first down marker would be at or beyond the goal line</returns>
        public static bool IsGoalToGo(int fieldPosition, int yardsToGo)
        {
            // Calculate where the first down marker would be
            int firstDownMarker = fieldPosition + yardsToGo;

            // If the first down marker would be at or beyond the goal line (100), it's goal to go
            return firstDownMarker >= 100;
        }

        /// <summary>
        /// Checks if position is in goal-to-go situation based on position alone (opponent's 10 or closer)
        /// This is a simpler check that assumes standard 10 yards to go
        /// </summary>
        public static bool IsGoalToGo(int fieldPosition)
        {
            return IsGoalToGo(fieldPosition, 10);
        }
    }
}
