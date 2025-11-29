import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const Navigation = () => {
  const { isAuthenticated, user, login, logout } = useAuth();

  return (
    <nav className="bg-gridiron-primary text-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center space-x-8">
            <Link to="/" className="flex items-center">
              <h1 className="text-2xl font-bold">Gridiron</h1>
            </Link>
            <div className="hidden md:flex space-x-4">
              <Link
                to="/"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Home
              </Link>
              <Link
                to="/teams"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Teams
              </Link>
              <Link
                to="/leagues"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Leagues
              </Link>
              <Link
                to="/simulate"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Simulate Game
              </Link>
              <Link
                to="/profile"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Profile
              </Link>
            </div>
          </div>
          <div className="flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <span className="text-sm text-blue-200">
                  {user?.name || user?.username || 'User'}
                </span>
                <button
                  onClick={logout}
                  className="px-4 py-2 rounded-md text-sm font-medium bg-blue-900 hover:bg-blue-800 transition-colors"
                >
                  Logout
                </button>
              </>
            ) : (
              <button
                onClick={login}
                className="px-4 py-2 rounded-md text-sm font-medium bg-gridiron-secondary hover:bg-green-700 transition-colors"
              >
                Login
              </button>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};
