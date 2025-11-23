import { useTeams } from '../api';
import { Loading, ErrorMessage } from '../components';
import { Team } from '../types';

export const TeamsPage = () => {
  const { data: teams, isLoading, error } = useTeams();

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <ErrorMessage message="Failed to load teams. Make sure the API is running on http://localhost:5000" />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Teams</h1>
          <p className="text-gray-600 mt-1">
            {teams?.length || 0} teams in the league
          </p>
        </div>
      </div>

      {teams && teams.length > 0 ? (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {teams.map((team: Team) => (
            <div key={team.id} className="card hover:shadow-lg transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-xl font-bold text-gray-900">
                    {team.city} {team.name}
                  </h3>
                  <p className="text-sm text-gray-600 mt-1">
                    Division {team.divisionId || 'N/A'}
                  </p>
                </div>
                <div className="text-2xl">🏈</div>
              </div>

              <div className="space-y-2">
                <div className="flex justify-between items-center py-2 border-t">
                  <span className="text-sm text-gray-600">Record</span>
                  <span className="font-semibold text-gray-900">
                    {team.wins}-{team.losses}-{team.ties}
                  </span>
                </div>

                {team.championships > 0 && (
                  <div className="flex justify-between items-center py-2 border-t">
                    <span className="text-sm text-gray-600">Championships</span>
                    <span className="font-semibold text-yellow-600">
                      🏆 {team.championships}
                    </span>
                  </div>
                )}

                <div className="flex justify-between items-center py-2 border-t">
                  <span className="text-sm text-gray-600">Budget</span>
                  <span className="font-semibold text-gray-900">
                    ${(team.budget / 1000000).toFixed(1)}M
                  </span>
                </div>

                <div className="flex justify-between items-center py-2 border-t">
                  <span className="text-sm text-gray-600">Roster</span>
                  <span className="font-semibold text-gray-900">
                    {team.players?.length || 0} players
                  </span>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t">
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div>
                    <span className="text-gray-600">Fan Support:</span>
                    <div className="bg-gray-200 rounded-full h-2 mt-1">
                      <div
                        className="bg-gridiron-secondary h-2 rounded-full"
                        style={{ width: `${team.fanSupport}%` }}
                      ></div>
                    </div>
                  </div>
                  <div>
                    <span className="text-gray-600">Chemistry:</span>
                    <div className="bg-gray-200 rounded-full h-2 mt-1">
                      <div
                        className="bg-gridiron-primary h-2 rounded-full"
                        style={{ width: `${team.chemistry}%` }}
                      ></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="card text-center py-12">
          <p className="text-gray-600">No teams found. Create some teams to get started!</p>
        </div>
      )}
    </div>
  );
};
