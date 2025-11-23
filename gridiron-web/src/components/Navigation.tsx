import { Link } from 'react-router-dom';

export const Navigation = () => {
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
                to="/simulate"
                className="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-900 transition-colors"
              >
                Simulate Game
              </Link>
            </div>
          </div>
          <div className="flex items-center">
            <span className="text-sm text-blue-200">Football Manager</span>
          </div>
        </div>
      </div>
    </nav>
  );
};
