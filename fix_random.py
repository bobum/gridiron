import os

file_path = 'UnitTestProject1/Helpers/TestFluentSeedableRandom.cs'

with open(file_path, 'r') as f:
    lines = f.readlines()

# Find the end of ImmediateTackleYards
start_index = -1
for i, line in enumerate(lines):
    if 'public TestFluentSeedableRandom ImmediateTackleYards(int value)' in line:
        # Find the closing brace
        for j in range(i, len(lines)):
            if lines[j].strip() == '}':
                start_index = j + 1
                break
        break

# Find the start of KickHangTime
end_index = -1
for i, line in enumerate(lines):
    if 'public TestFluentSeedableRandom KickHangTime(double value)' in line:
        # Go back to the summary
        for j in range(i, 0, -1):
            if '/// <summary>' in lines[j]:
                end_index = j
                break
        break

if start_index != -1 and end_index != -1:
    new_content = [
        '\n',
        '        /// <summary>\n',
        '        /// Sets the big play bonus yards value (extra yards when big play occurs).\n',
        '        /// Valid range: 10 to 50\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom BigPlayBonusYards(int value)\n',
        '        {\n',
        '            ValidateYardage(value, nameof(BigPlayBonusYards), 10, 50,\n',
        '                "Extra yards awarded when big play occurs (5% chance if receiver speed > 85). " +\n',
        '                "Typical range: 10-30 yards.");\n',
        '            _intQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the sack yardage loss value (2-10 yards typically).\n',
        '        /// Valid range: 2 to 15\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom SackYards(int value)\n',
        '        {\n',
        '            ValidateYardage(value, nameof(SackYards), 2, 15,\n',
        '                "Yards lost on sack (returned as negative). " +\n',
        '                "Typical range: 2-10 yards, limited by field position (can\'t go past own goal line).");\n',
        '            _intQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        // Run Play methods (add as needed)\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the run blocking check value. Lower values mean successful blocking.\n',
        '        /// Valid range: 0.0 to 1.0\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom RunBlockingCheck(double value)\n',
        '        {\n',
        '            ValidateProbability(value, nameof(RunBlockingCheck),\n',
        '                "Determines if offensive line successfully blocks for run play. " +\n',
        '                "Lower values mean successful blocking.");\n',
        '            _doubleQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the run defense check value. Lower values mean defense fails to stop the run.\n',
        '        /// Valid range: 0.0 to 1.0\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom RunDefenseCheck(double value)\n',
        '        {\n',
        '            ValidateProbability(value, nameof(RunDefenseCheck),\n',
        '                "Determines if defense successfully stops the run. " +\n',
        '                "Lower values mean defense fails to stop the run.");\n',
        '            _doubleQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the out of bounds check value.\n',
        '        /// Run plays: < 0.15 (outside) or < 0.02 (inside) means out of bounds.\n',
        '        /// Valid range: 0.0 to 1.0\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom OutOfBoundsCheck(double value)\n',
        '        {\n',
        '            ValidateProbability(value, nameof(OutOfBoundsCheck),\n',
        '                "Determines if play goes out of bounds. " +\n',
        '                "Run plays: < 0.15 (outside) or < 0.02 (inside). Pass plays: < 0.10-0.20.");\n',
        '            _doubleQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the runoff time random factor (0.0-1.0).\n',
        '        /// Used when clock keeps running (in-bounds, no penalty/TD).\n',
        '        /// Calculation: 25.0 + (factor * 10.0) = 25.0 to 35.0 seconds.\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom RunoffTimeRandomFactor(double value)\n',
        '        {\n',
        '            ValidateRandomFactor(value, nameof(RunoffTimeRandomFactor),\n',
        '                "Random factor for runoff time (between plays). " +\n',
        '                "Used when clock keeps running. 25.0 + (factor * 10.0) = 25-35 seconds.");\n',
        '            _doubleQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the breakaway check value. Lower values trigger a breakaway run.\n',
        '        /// Valid range: 0.0 to 1.0\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom BreakawayCheck(double value)\n',
        '        {\n',
        '            ValidateProbability(value, nameof(BreakawayCheck),\n',
        '                "Determines if running back breaks free for a long run. " +\n',
        '                "Lower values (typically < ~0.05-0.10) trigger breakaway.");\n',
        '            _doubleQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the run yards value.\n',
        '        /// Valid range: -10 to 99\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom RunYards(int value)\n',
        '        {\n',
        '            ValidateYardage(value, nameof(RunYards), -10, 99,\n',
        '                "Yards gained on run play (can be negative for loss). " +\n',
        '                "Typical range: -3 to 15 yards, limited by field position.");\n',
        '            _intQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n',
        '        // Kickoff methods (add as needed)\n',
        '\n',
        '        /// <summary>\n',
        '        /// Sets the kick distance value (in yards).\n',
        '        /// Valid range: 20 to 75\n',
        '        /// </summary>\n',
        '        public TestFluentSeedableRandom KickDistance(int value)\n',
        '        {\n',
        '            ValidateYardage(value, nameof(KickDistance), 20, 75,\n',
        '                "Distance of kickoff in yards. " +\n',
        '                "Typical range: 45-70 yards for normal kickoff, 20-40 for onside kick.");\n',
        '            _intQueue.Enqueue(value);\n',
        '            return this;\n',
        '        }\n',
        '\n'
    ]
    
    final_lines = lines[:start_index] + new_content + lines[end_index:]
    
    with open(file_path, 'w') as f:
        f.writelines(final_lines)
    print("File updated successfully.")
else:
    print("Could not find insertion points.")
