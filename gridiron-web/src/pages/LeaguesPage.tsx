import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useLeagues, useCreateLeague, useCurrentUser } from '../api';
import { Loading, ErrorMessage } from '../components';
import type { CreateLeagueRequest } from '../types/League';

export const LeaguesPage = () => {
  const { data: leagues, isLoading, error } = useLeagues();
  const { data: user } = useCurrentUser();
  const createLeague = useCreateLeague();
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [formData, setFormData] = useState<CreateLeagueRequest>({
    name: '',
    numberOfConferences: 2,
    divisionsPerConference: 2,
    teamsPerDivision: 4,
  });
  const [formError, setFormError] = useState<string | null>(null);

  // Get user's role in a specific league
  const getUserRoleInLeague = (leagueId: number) => {
    if (!user?.leagueRoles) return null;
    const roles = user.leagueRoles.filter(r => r.leagueId === leagueId);
    if (roles.some(r => r.role === 'Commissioner')) return 'Commissioner';
    if (roles.some(r => r.role === 'GeneralManager')) return 'GeneralManager';
    return null;
  };

  const handleCreateLeague = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);

    // Validation
    if (!formData.name.trim()) {
      setFormError('League name is required');
      return;
    }
    if (formData.numberOfConferences < 1 || formData.numberOfConferences > 8) {
      setFormError('Number of conferences must be between 1 and 8');
      return;
    }
    if (formData.divisionsPerConference < 1 || formData.divisionsPerConference > 8) {
      setFormError('Divisions per conference must be between 1 and 8');
      return;
    }
    if (formData.teamsPerDivision < 1 || formData.teamsPerDivision > 8) {
      setFormError('Teams per division must be between 1 and 8');
      return;
    }

    try {
      await createLeague.mutateAsync(formData);
      setShowCreateModal(false);
      setFormData({
        name: '',
        numberOfConferences: 2,
        divisionsPerConference: 2,
        teamsPerDivision: 4,
      });
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Failed to create league');
    }
  };

  const totalTeams = formData.numberOfConferences * formData.divisionsPerConference * formData.teamsPerDivision;

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="Failed to load leagues" />;

  return (
    <div className="space-y-8">
      {/* Page Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-4xl font-bold text-gray-900 mb-2">My Leagues</h1>
          <p className="text-lg text-gray-600">
            Manage your leagues or create a new one
          </p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="btn-primary text-lg px-6 py-3"
          data-testid="create-league-button"
        >
          + Create League
        </button>
      </div>

      {/* Leagues Grid */}
      {!leagues || leagues.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">🏈</div>
          <h2 className="text-2xl font-semibold text-gray-900 mb-2">No Leagues Yet</h2>
          <p className="text-gray-600 mb-6">
            Create your first league to get started, or ask a commissioner to add you to an existing league.
          </p>
          <button
            onClick={() => setShowCreateModal(true)}
            className="btn-primary"
            data-testid="create-first-league-button"
          >
            Create Your First League
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {leagues.map(league => {
            const role = getUserRoleInLeague(league.id);
            return (
              <Link
                key={league.id}
                to={`/leagues/${league.id}`}
                className="card border-2 border-gray-200 hover:border-gridiron-primary hover:shadow-lg transition-all"
                data-testid={`league-card-${league.id}`}
              >
                <div className="flex justify-between items-start mb-3">
                  <h3 className="text-xl font-semibold text-gray-900">{league.name}</h3>
                  {role && (
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        role === 'Commissioner'
                          ? 'bg-purple-100 text-purple-800'
                          : 'bg-blue-100 text-blue-800'
                      }`}
                    >
                      {role === 'Commissioner' ? '👑 Commissioner' : '📋 GM'}
                    </span>
                  )}
                </div>
                <div className="space-y-2 text-sm text-gray-600">
                  <div className="flex justify-between">
                    <span>Season:</span>
                    <span className="font-medium text-gray-900">{league.season}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Conferences:</span>
                    <span className="font-medium text-gray-900">{league.totalConferences}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Teams:</span>
                    <span className="font-medium text-gray-900">{league.totalTeams}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Status:</span>
                    <span className={`font-medium ${league.isActive ? 'text-green-600' : 'text-gray-500'}`}>
                      {league.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                </div>
                <div className="mt-4 pt-3 border-t border-gray-200">
                  <span className="text-gridiron-primary font-medium text-sm">
                    View League →
                  </span>
                </div>
              </Link>
            );
          })}
        </div>
      )}

      {/* Create League Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" data-testid="create-league-modal">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-2xl font-bold text-gray-900">Create New League</h2>
                <button
                  onClick={() => setShowCreateModal(false)}
                  className="text-gray-400 hover:text-gray-600 text-2xl"
                  data-testid="close-modal-button"
                >
                  ×
                </button>
              </div>

              <form onSubmit={handleCreateLeague} className="space-y-6">
                {formError && (
                  <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded" data-testid="form-error">
                    {formError}
                  </div>
                )}

                {/* League Name */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    League Name *
                  </label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
                    placeholder="My Awesome League"
                    data-testid="league-name-input"
                  />
                </div>

                {/* Structure Configuration */}
                <div className="bg-gray-50 rounded-lg p-4 space-y-4">
                  <h3 className="font-medium text-gray-900">League Structure</h3>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Number of Conferences
                    </label>
                    <input
                      type="number"
                      min="1"
                      max="8"
                      value={formData.numberOfConferences}
                      onChange={(e) => setFormData({ ...formData, numberOfConferences: parseInt(e.target.value) || 1 })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
                      data-testid="conferences-input"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Divisions per Conference
                    </label>
                    <input
                      type="number"
                      min="1"
                      max="8"
                      value={formData.divisionsPerConference}
                      onChange={(e) => setFormData({ ...formData, divisionsPerConference: parseInt(e.target.value) || 1 })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
                      data-testid="divisions-input"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Teams per Division
                    </label>
                    <input
                      type="number"
                      min="1"
                      max="8"
                      value={formData.teamsPerDivision}
                      onChange={(e) => setFormData({ ...formData, teamsPerDivision: parseInt(e.target.value) || 1 })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-gridiron-primary focus:border-transparent"
                      data-testid="teams-input"
                    />
                  </div>

                  {/* Summary */}
                  <div className="bg-gridiron-primary bg-opacity-10 rounded p-3 text-center">
                    <span className="text-sm text-gray-700">Total Teams: </span>
                    <span className="text-lg font-bold text-gridiron-primary" data-testid="total-teams">
                      {totalTeams}
                    </span>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex space-x-4">
                  <button
                    type="button"
                    onClick={() => setShowCreateModal(false)}
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={createLeague.isPending}
                    className="flex-1 btn-primary disabled:opacity-50"
                    data-testid="submit-create-league"
                  >
                    {createLeague.isPending ? 'Creating...' : 'Create League'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
