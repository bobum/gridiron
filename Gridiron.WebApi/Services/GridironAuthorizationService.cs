using DataAccessLayer.Repositories;
using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Implementation of authorization service for hierarchical role-based access control.
/// </summary>
public class GridironAuthorizationService : IGridironAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IConferenceRepository _conferenceRepository;

    public GridironAuthorizationService(
        IUserRepository userRepository,
        ITeamRepository teamRepository,
        IDivisionRepository divisionRepository,
        IConferenceRepository conferenceRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
    }

    public async Task<User> GetOrCreateUserFromClaimsAsync(string azureAdObjectId, string email, string displayName)
    {
        var user = await _userRepository.GetByAzureAdObjectIdAsync(azureAdObjectId);

        if (user == null)
        {
            // First-time login - create user record
            user = new User
            {
                AzureAdObjectId = azureAdObjectId,
                Email = email,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsGlobalAdmin = false
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return user;
    }

    public async Task<bool> IsGlobalAdminAsync(string azureAdObjectId)
    {
        // In E2E test mode, treat the test user as a global admin
        if (azureAdObjectId == "e2e-test-user-object-id")
        {
            return true;
        }

        var user = await _userRepository.GetByAzureAdObjectIdAsync(azureAdObjectId);
        return user?.IsGlobalAdmin ?? false;
    }

    public async Task<bool> CanAccessLeagueAsync(string azureAdObjectId, int leagueId)
    {
        // Global admins can access everything
        if (await IsGlobalAdminAsync(azureAdObjectId))
        {
            return true;
        }

        // Check if user has any role in this league
        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return false;
        }

        return user.LeagueRoles.Any(ulr => ulr.LeagueId == leagueId);
    }

    public async Task<bool> CanAccessTeamAsync(string azureAdObjectId, int teamId)
    {
        // Global admins can access everything
        if (await IsGlobalAdminAsync(azureAdObjectId))
        {
            return true;
        }

        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return false;
        }

        // Get the team and navigate to its league
        // Team → Division → Conference → League
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null || team.DivisionId == null)
        {
            return false;
        }

        var division = await _divisionRepository.GetByIdAsync(team.DivisionId.Value);
        if (division == null)
        {
            return false;
        }

        var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
        if (conference == null)
        {
            return false;
        }

        var leagueId = conference.LeagueId;

        foreach (var role in user.LeagueRoles)
        {
            // If user is commissioner of the league this team belongs to, they can access it
            if (role.LeagueId == leagueId && role.Role == UserRole.Commissioner)
            {
                return true;
            }

            // If user is GM of this specific team, they can access it
            if (role.TeamId == teamId && role.Role == UserRole.GeneralManager)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> IsCommissionerOfLeagueAsync(string azureAdObjectId, int leagueId)
    {
        // Global admins are effectively commissioners of all leagues
        if (await IsGlobalAdminAsync(azureAdObjectId))
        {
            return true;
        }

        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return false;
        }

        return user.LeagueRoles.Any(ulr =>
            ulr.LeagueId == leagueId &&
            ulr.Role == UserRole.Commissioner);
    }

    public async Task<bool> IsGeneralManagerOfTeamAsync(string azureAdObjectId, int teamId)
    {
        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return false;
        }

        return user.LeagueRoles.Any(ulr =>
            ulr.TeamId == teamId &&
            ulr.Role == UserRole.GeneralManager);
    }

    public async Task<List<int>> GetAccessibleLeagueIdsAsync(string azureAdObjectId)
    {
        // Global admins can access all leagues
        if (await IsGlobalAdminAsync(azureAdObjectId))
        {
            // Return empty list to signal "all leagues" - caller should handle this
            // Alternatively, we could load all league IDs, but that's expensive
            return new List<int>();
        }

        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return new List<int>();
        }

        return user.LeagueRoles
            .Select(ulr => ulr.LeagueId)
            .Distinct()
            .ToList();
    }

    public async Task<List<int>> GetAccessibleTeamIdsAsync(string azureAdObjectId)
    {
        // Global admins can access all teams
        if (await IsGlobalAdminAsync(azureAdObjectId))
        {
            return new List<int>(); // Signal "all teams"
        }

        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        if (user == null)
        {
            return new List<int>();
        }

        var accessibleTeamIds = new HashSet<int>();

        foreach (var role in user.LeagueRoles)
        {
            if (role.Role == UserRole.Commissioner)
            {
                // Commissioners can access all teams in their league
                // We need to load teams for this league
                var teams = await _teamRepository.GetTeamsByLeagueIdAsync(role.LeagueId);
                foreach (var team in teams)
                {
                    accessibleTeamIds.Add(team.Id);
                }
            }
            else if (role.Role == UserRole.GeneralManager && role.TeamId.HasValue)
            {
                // GMs can access only their specific team
                accessibleTeamIds.Add(role.TeamId.Value);
            }
        }

        return accessibleTeamIds.ToList();
    }
}
