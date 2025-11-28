import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useCurrentUser } from '../api';
import { Loading, ErrorMessage } from '../components';

export const ProfilePage = () => {
  const { data: user, isLoading, error } = useCurrentUser();
  const [copiedUserId, setCopiedUserId] = useState(false);

  const copyUserId = () => {
    if (user) {
      navigator.clipboard.writeText(user.id.toString());
      setCopiedUserId(true);
      setTimeout(() => setCopiedUserId(false), 2000);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (isLoading) return <Loading />;
  if (error) return <ErrorMessage message="Failed to load user profile" />;
  if (!user) return <ErrorMessage message="User not found" />;

  // Group roles by league
  const leagueRolesMap = new Map<number, {
    leagueId: number;
    leagueName: string;
    roles: typeof user.leagueRoles;
  }>();

  user.leagueRoles.forEach(role => {
    if (!leagueRolesMap.has(role.leagueId)) {
      leagueRolesMap.set(role.leagueId, {
        leagueId: role.leagueId,
        leagueName: role.leagueName,
        roles: [],
      });
    }
    leagueRolesMap.get(role.leagueId)?.roles.push(role);
  });

  const leagues = Array.from(leagueRolesMap.values());

  // Get all teams the user manages
  const teams = user.leagueRoles
    .filter(role => role.role === 'GeneralManager' && role.teamId)
    .map(role => ({
      teamId: role.teamId!,
      teamName: role.teamName!,
      leagueId: role.leagueId,
      leagueName: role.leagueName,
    }));

  return (
    <div className="space-y-8">
      {/* Page Header */}
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">
          User Profile
        </h1>
        <p className="text-lg text-gray-600">
          Manage your account and view your leagues
        </p>
      </div>

      {/* User Info Card */}
      <div className="card max-w-2xl mx-auto">
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Account Information</h2>

        <div className="space-y-4">
          {/* User ID - Shareable Key */}
          <div className="bg-gridiron-primary bg-opacity-5 border-2 border-gridiron-primary rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <label className="text-sm font-medium text-gray-700 block mb-1">
                  Your User ID
                </label>
                <div className="flex items-center space-x-3">
                  <code className="text-2xl font-bold text-gridiron-primary">
                    {user.id}
                  </code>
                  <button
                    onClick={copyUserId}
                    className="btn-primary text-sm px-3 py-1"
                  >
                    {copiedUserId ? '✓ Copied!' : 'Copy'}
                  </button>
                </div>
                <p className="text-xs text-gray-600 mt-2">
                  Share this ID with league commissioners to join a league
                </p>
              </div>
            </div>
          </div>

          {/* Display Name */}
          <div>
            <label className="text-sm font-medium text-gray-700 block mb-1">
              Display Name
            </label>
            <div className="text-lg text-gray-900">{user.displayName}</div>
          </div>

          {/* Email */}
          <div>
            <label className="text-sm font-medium text-gray-700 block mb-1">
              Email
            </label>
            <div className="text-lg text-gray-900">{user.email}</div>
          </div>

          {/* Global Admin Badge */}
          {user.isGlobalAdmin && (
            <div className="inline-flex items-center px-3 py-1 rounded-full bg-purple-100 text-purple-800 text-sm font-medium">
              🔧 Global Administrator
            </div>
          )}

          {/* Account Activity */}
          <div className="grid grid-cols-2 gap-4 pt-4 border-t border-gray-200">
            <div>
              <label className="text-sm font-medium text-gray-700 block mb-1">
                Member Since
              </label>
              <div className="text-sm text-gray-900">{formatDate(user.createdAt)}</div>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-700 block mb-1">
                Last Login
              </label>
              <div className="text-sm text-gray-900">{formatDate(user.lastLoginAt)}</div>
            </div>
          </div>
        </div>
      </div>

      {/* Leagues Section */}
      <div className="card max-w-2xl mx-auto">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-2xl font-semibold text-gray-900">
            My Leagues ({leagues.length})
          </h2>
        </div>

        {leagues.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <div className="text-5xl mb-3">🏈</div>
            <p className="text-lg font-medium mb-2">No leagues yet</p>
            <p className="text-sm">
              Join a league by sharing your User ID with a commissioner, or create your own league
            </p>
          </div>
        ) : (
          <div className="space-y-3">
            {leagues.map(league => (
              <Link
                key={league.leagueId}
                to={`/leagues/${league.leagueId}`}
                className="block card border-2 border-gray-200 hover:border-gridiron-primary hover:shadow-md transition-all cursor-pointer"
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {league.leagueName}
                    </h3>
                    <div className="flex flex-wrap gap-2 mt-2">
                      {league.roles.map(role => (
                        <span
                          key={role.id}
                          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                            role.role === 'Commissioner'
                              ? 'bg-purple-100 text-purple-800'
                              : 'bg-blue-100 text-blue-800'
                          }`}
                        >
                          {role.role === 'Commissioner' ? '👑 Commissioner' : '📋 GM'}
                          {role.teamName && ` - ${role.teamName}`}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="text-gridiron-primary text-xl">→</div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      {/* Teams Section */}
      {teams.length > 0 && (
        <div className="card max-w-2xl mx-auto">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-semibold text-gray-900">
              My Teams ({teams.length})
            </h2>
          </div>

          <div className="space-y-3">
            {teams.map(team => (
              <Link
                key={team.teamId}
                to={`/teams/${team.teamId}`}
                className="block card border-2 border-gray-200 hover:border-gridiron-secondary hover:shadow-md transition-all cursor-pointer"
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {team.teamName}
                    </h3>
                    <p className="text-sm text-gray-600 mt-1">
                      {team.leagueName}
                    </p>
                  </div>
                  <div className="text-gridiron-secondary text-xl">→</div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
