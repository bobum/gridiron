import { useState } from 'react';
import { useTeams, useSimulateGame } from '../api';
import { Loading, ErrorMessage } from '../components';

export const GameSimulationPage = () => {
  const { data: teams, isLoading: teamsLoading, error: teamsError } = useTeams();
  const simulateGame = useSimulateGame();

  const [homeTeamId, setHomeTeamId] = useState<number | null>(null);
  const [awayTeamId, setAwayTeamId] = useState<number | null>(null);

  const handleSimulate = async () => {
    if (!homeTeamId || !awayTeamId) {
      alert('Please select both teams');
      return;
    }

    if (homeTeamId === awayTeamId) {
      alert('Please select different teams');
      return;
    }

    try {
      await simulateGame.mutateAsync({
        homeTeamId,
        awayTeamId,
      });
    } catch (error) {
      console.error('Simulation failed:', error);
    }
  };

  if (teamsLoading) {
    return <Loading />;
  }

  if (teamsError) {
    return (
      <ErrorMessage message="Failed to load teams. Make sure the API is running on http://localhost:5000" />
    );
  }

  const homeTeam = teams?.find(t => t.id === homeTeamId);
  const awayTeam = teams?.find(t => t.id === awayTeamId);

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Simulate Game</h1>
        <p className="text-gray-600 mt-1">
          Select two teams to run a game simulation
        </p>
      </div>

      <div className="card">
        <div className="grid md:grid-cols-2 gap-6">
          {/* Home Team Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Home Team
            </label>
            <select
              value={homeTeamId || ''}
              onChange={(e) => setHomeTeamId(Number(e.target.value))}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
            >
              <option value="">Select Home Team...</option>
              {teams?.map((team) => (
                <option key={team.id} value={team.id}>
                  {team.city} {team.name} ({team.wins}-{team.losses}-{team.ties})
                </option>
              ))}
            </select>
            {homeTeam && (
              <div className="mt-3 p-3 bg-blue-50 rounded-lg">
                <p className="text-sm text-gray-700">
                  <strong>Record:</strong> {homeTeam.wins}-{homeTeam.losses}-{homeTeam.ties}
                </p>
                <p className="text-sm text-gray-700">
                  <strong>Roster:</strong> {homeTeam.players?.length || 0} players
                </p>
              </div>
            )}
          </div>

          {/* Away Team Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Away Team
            </label>
            <select
              value={awayTeamId || ''}
              onChange={(e) => setAwayTeamId(Number(e.target.value))}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
            >
              <option value="">Select Away Team...</option>
              {teams?.map((team) => (
                <option key={team.id} value={team.id}>
                  {team.city} {team.name} ({team.wins}-{team.losses}-{team.ties})
                </option>
              ))}
            </select>
            {awayTeam && (
              <div className="mt-3 p-3 bg-red-50 rounded-lg">
                <p className="text-sm text-gray-700">
                  <strong>Record:</strong> {awayTeam.wins}-{awayTeam.losses}-{awayTeam.ties}
                </p>
                <p className="text-sm text-gray-700">
                  <strong>Roster:</strong> {awayTeam.players?.length || 0} players
                </p>
              </div>
            )}
          </div>
        </div>

        <div className="mt-6 flex justify-center">
          <button
            onClick={handleSimulate}
            disabled={!homeTeamId || !awayTeamId || simulateGame.isPending}
            className="btn-primary px-8 py-3 text-lg disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {simulateGame.isPending ? (
              <span className="flex items-center">
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Simulating...
              </span>
            ) : (
              '⚡ Simulate Game'
            )}
          </button>
        </div>
      </div>

      {/* Result Display */}
      {simulateGame.isSuccess && simulateGame.data && (
        <div className="card bg-green-50 border-green-200">
          <h3 className="text-xl font-bold text-green-900 mb-4">Game Complete!</h3>
          <div className="flex justify-around items-center">
            <div className="text-center">
              <p className="text-sm text-gray-600 mb-1">Home</p>
              <p className="text-3xl font-bold text-gray-900">{simulateGame.data.homeScore}</p>
              <p className="text-sm text-gray-700 mt-2">
                {homeTeam?.city} {homeTeam?.name}
              </p>
            </div>
            <div className="text-4xl font-bold text-gray-400">VS</div>
            <div className="text-center">
              <p className="text-sm text-gray-600 mb-1">Away</p>
              <p className="text-3xl font-bold text-gray-900">{simulateGame.data.awayScore}</p>
              <p className="text-sm text-gray-700 mt-2">
                {awayTeam?.city} {awayTeam?.name}
              </p>
            </div>
          </div>
          <div className="mt-4 text-center">
            <p className="text-sm text-gray-600">
              Game ID: {simulateGame.data.id}
            </p>
          </div>
        </div>
      )}

      {/* Error Display */}
      {simulateGame.isError && (
        <ErrorMessage message="Failed to simulate game. Please try again." />
      )}

      {/* Instructions */}
      <div className="card bg-blue-50 border-blue-200">
        <h3 className="font-semibold text-blue-900 mb-2">How it works</h3>
        <ul className="text-sm text-blue-800 space-y-1 list-disc list-inside">
          <li>Select a home team and an away team from the dropdowns</li>
          <li>Click "Simulate Game" to run a full game simulation</li>
          <li>The game engine will process all plays and return the final score</li>
          <li>Results are saved to the database for future reference</li>
        </ul>
      </div>
    </div>
  );
};
