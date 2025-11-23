import { Link } from 'react-router-dom';
import { useTeams } from '../api';
import { Loading } from '../components';

export const HomePage = () => {
  const { data: teams, isLoading, error } = useTeams();

  const getApiStatus = () => {
    if (isLoading) return { text: 'Checking...', color: 'bg-yellow-500' };
    if (error) return { text: 'Offline', color: 'bg-red-500' };
    return { text: 'Online', color: 'bg-green-500' };
  };

  const status = getApiStatus();

  return (
    <div className="space-y-8">
      {/* Hero Section */}
      <div className="text-center">
        <h1 className="text-5xl font-bold text-gray-900 mb-4">
          Gridiron Football Manager
        </h1>
        <p className="text-xl text-gray-600 max-w-2xl mx-auto">
          Your ultimate football management simulation. Build your team, simulate games,
          and lead your franchise to championship glory.
        </p>
      </div>

      {/* API Status Card */}
      <div className="card max-w-md mx-auto">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-lg font-semibold text-gray-900">API Status</h3>
            <p className="text-sm text-gray-600 mt-1">
              {isLoading ? 'Connecting to server...' : error ? 'Cannot connect to API' : `${teams?.length || 0} teams loaded`}
            </p>
          </div>
          <div className="flex items-center space-x-2">
            <span className={`h-3 w-3 rounded-full ${status.color} animate-pulse`}></span>
            <span className="text-sm font-medium text-gray-700">{status.text}</span>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="grid md:grid-cols-3 gap-6 max-w-4xl mx-auto">
        <Link to="/teams" className="card hover:shadow-lg transition-shadow cursor-pointer">
          <div className="text-center">
            <div className="text-4xl mb-3">🏈</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">View Teams</h3>
            <p className="text-sm text-gray-600">
              Browse all teams in the league
            </p>
          </div>
        </Link>

        <Link to="/simulate" className="card hover:shadow-lg transition-shadow cursor-pointer">
          <div className="text-center">
            <div className="text-4xl mb-3">⚡</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Simulate Game</h3>
            <p className="text-sm text-gray-600">
              Run a game simulation between two teams
            </p>
          </div>
        </Link>

        <div className="card bg-gray-50">
          <div className="text-center">
            <div className="text-4xl mb-3">🏆</div>
            <h3 className="text-lg font-semibold text-gray-400 mb-2">League Mode</h3>
            <p className="text-sm text-gray-400">
              Coming soon
            </p>
          </div>
        </div>
      </div>

      {/* Stats Preview */}
      {teams && teams.length > 0 && (
        <div className="card max-w-4xl mx-auto">
          <h3 className="text-xl font-semibold text-gray-900 mb-4">League Overview</h3>
          <div className="grid grid-cols-3 gap-4 text-center">
            <div>
              <div className="text-3xl font-bold text-gridiron-primary">{teams.length}</div>
              <div className="text-sm text-gray-600 mt-1">Total Teams</div>
            </div>
            <div>
              <div className="text-3xl font-bold text-gridiron-secondary">
                {teams.reduce((sum, team) => sum + (team.players?.length || 0), 0)}
              </div>
              <div className="text-sm text-gray-600 mt-1">Total Players</div>
            </div>
            <div>
              <div className="text-3xl font-bold text-gridiron-accent">
                {teams.reduce((sum, team) => sum + team.wins, 0)}
              </div>
              <div className="text-sm text-gray-600 mt-1">Total Wins</div>
            </div>
          </div>
        </div>
      )}

      {/* Loading State */}
      {isLoading && <Loading />}

      {/* Error State */}
      {error && (
        <div className="card max-w-md mx-auto bg-red-50 border-red-200">
          <p className="text-red-800 text-center">
            <strong>API Connection Error</strong><br />
            Make sure the backend API is running on http://localhost:5000
          </p>
        </div>
      )}
    </div>
  );
};
