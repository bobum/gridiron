# Authorization and User Management in Gridiron

## Table of Contents
1. [Overview](#overview)
2. [Authentication System](#authentication-system)
3. [Authorization Roles](#authorization-roles)
4. [User Lifecycle](#user-lifecycle)
5. [Role Assignment](#role-assignment)
6. [Access Control Examples](#access-control-examples)
7. [API Endpoints](#api-endpoints)
8. [Frontend Integration](#frontend-integration)
9. [Database Schema](#database-schema)
10. [Security Best Practices](#security-best-practices)

---

## Overview

Gridiron uses a **hierarchical Role-Based Access Control (RBAC)** system with three distinct roles:

1. **God (Global Admin)** - System administrators with full access
2. **Commissioner** - League administrators with full access to their league(s)
3. **General Manager (GM)** - Team managers with access only to their team(s)

This system enables multi-tenant league management where:
- Multiple leagues can exist independently
- Each league has its own Commissioner(s)
- Each team has its own General Manager
- Users can have different roles in different leagues

---

## Authentication System

### Azure Entra ID External CIAM

Gridiron uses **Azure Entra ID (formerly Azure AD B2C)** for authentication with an External Customer Identity Access Management (CIAM) setup.

#### Configuration

**Tenant:** `gtggridiron.ciamlogin.com`
**Client ID:** `29348959-a014-4550-b3c3-044585c83f0a`
**Authority:** `https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com`

#### Supported Identity Providers

- **Azure AD accounts** (gtggridiron.onmicrosoft.com)
- **Google** (social login configured)
- **Email + Password** (local accounts)

#### Authentication Flow

```
User clicks "Sign In"
    ↓
Redirect to Azure Entra ID login page
    ↓
User authenticates (Azure AD, Google, or Email)
    ↓
Azure Entra ID returns JWT token with claims
    ↓
Frontend stores token in MSAL cache
    ↓
Backend validates JWT signature on each request
    ↓
User identity extracted from "oid" claim
```

#### JWT Token Claims

The JWT token contains these critical claims:

| Claim | Description | Example |
|-------|-------------|---------|
| `oid` | Azure AD Object ID (unique user identifier) | `"12345678-1234-1234-1234-123456789abc"` |
| `email` | User's email address | `"user@example.com"` |
| `name` | User's display name | `"John Smith"` |
| `iss` | Token issuer | `"https://gtggridiron.ciamlogin.com/..."` |
| `aud` | Token audience (Client ID) | `"29348959-a014-4550-b3c3-044585c83f0a"` |
| `exp` | Token expiration timestamp | `1735372800` |

### First-Time Login

On first authentication, the backend automatically creates a user record:

```csharp
// HttpContextExtensions.cs extracts claims
var azureAdObjectId = HttpContext.GetAzureAdObjectId();
var email = HttpContext.GetUserEmail();
var displayName = HttpContext.GetUserDisplayName();

// GridironAuthorizationService auto-creates user
var user = await _authService.GetOrCreateUserFromClaimsAsync(
    azureAdObjectId,
    email,
    displayName
);

// User record created in database with:
// - AzureAdObjectId (immutable ID)
// - Email, DisplayName (from Azure AD)
// - IsGlobalAdmin = false (default)
// - CreatedAt, LastLoginAt timestamps
```

**Key Point:** The `AzureAdObjectId` (oid claim) is the **immutable identifier** used for all authorization checks. Email can change in Azure AD, but oid never does.

---

## Authorization Roles

### 1. God (Global Admin)

**Capabilities:**
- ✅ Access **ALL** leagues, conferences, divisions, teams, and players
- ✅ Create/modify/delete any entity in the system
- ✅ Assign or remove roles for any user
- ✅ Promote/demote Commissioners
- ✅ View and manage all users

**Database Flag:**
```csharp
User.IsGlobalAdmin = true
```

**Use Cases:**
- System administrators
- Platform owners
- Support staff resolving issues
- Initial setup/configuration

**Assignment:**
- Manually set by database migration or direct DB update
- Cannot be self-assigned via API
- Typically only 1-3 users per system

---

### 2. Commissioner

**Capabilities:**
- ✅ Full access to **their league(s)** only
- ✅ View/modify all teams in their league
- ✅ View/modify all players in their league
- ✅ Assign/remove General Managers for teams in their league
- ✅ Create/delete teams, conferences, divisions in their league
- ✅ Simulate games for any teams in their league
- ✅ View all users in their league
- ❌ Cannot access other leagues
- ❌ Cannot promote users to Global Admin

**Database Record:**
```csharp
UserLeagueRole {
    UserId = <user_id>,
    LeagueId = <league_id>,
    Role = UserRole.Commissioner,
    TeamId = null  // Commissioners are not tied to a specific team
}
```

**Use Cases:**
- League creator/owner
- League moderators
- Tournament organizers
- Multiple Commissioners per league allowed

**Assignment:**
- **Auto-assigned** when creating a league (creator becomes Commissioner)
- Can be assigned by Global Admin or another Commissioner
- Can be assigned via `POST /api/users/{userId}/league-roles`

---

### 3. General Manager (GM)

**Capabilities:**
- ✅ Full access to **their team(s)** only
- ✅ View/modify roster for their team
- ✅ Edit depth charts for their team
- ✅ View players on their team
- ✅ Simulate games involving their team
- ✅ View schedule and standings for their league
- ✅ View other users in their league (read-only)
- ❌ Cannot modify other teams
- ❌ Cannot assign roles
- ❌ Cannot delete teams or leagues

**Database Record:**
```csharp
UserLeagueRole {
    UserId = <user_id>,
    LeagueId = <league_id>,
    Role = UserRole.GeneralManager,
    TeamId = <team_id>  // REQUIRED: GMs must be assigned to a specific team
}
```

**Use Cases:**
- Team owners
- Players in fantasy leagues
- Participants in tournaments
- Can be GM of multiple teams (even in different leagues)

**Assignment:**
- Assigned by Global Admin or League Commissioner
- Must specify TeamId when assigning
- Can be assigned via `POST /api/users/{userId}/league-roles`

---

## User Lifecycle

### 1. New User Registration

```
User signs in with Azure AD for first time
    ↓
Frontend redirects to Azure Entra ID
    ↓
User authenticates (Google, Email, etc.)
    ↓
Azure returns JWT with oid, email, name claims
    ↓
Frontend makes first API request with JWT
    ↓
Backend extracts oid from token
    ↓
GridironAuthorizationService.GetOrCreateUserFromClaimsAsync()
    ↓
IF User exists (by AzureAdObjectId):
    Update LastLoginAt timestamp
ELSE:
    Create new User record:
        AzureAdObjectId = oid claim
        Email = email claim
        DisplayName = name claim
        IsGlobalAdmin = false
        CreatedAt = DateTime.UtcNow
        LastLoginAt = DateTime.UtcNow
        LeagueRoles = [] (empty - no roles yet)
    ↓
Return User object
```

**Result:** User can now browse public leagues but has no management permissions until assigned a role.

---

### 2. Role Assignment

#### Scenario A: User Creates a League

```
User (already authenticated) creates a league
    ↓
POST /api/leagues-management
    Body: { name: "My League", numberOfConferences: 2, ... }
    ↓
LeaguesManagementController.CreateLeague()
    ↓
Extract azureAdObjectId from JWT token
    ↓
Get or create User from database
    ↓
LeagueBuilderService.CreateLeague(leagueName, userId)
    ↓
Create League entity
    ↓
Auto-create UserLeagueRole:
        UserId = current user
        LeagueId = new league
        Role = Commissioner
        TeamId = null
    ↓
Save to database
    ↓
Return LeagueDto
```

**Result:** User is now Commissioner of their newly created league.

---

#### Scenario B: Commissioner Assigns a GM

```
Commissioner wants to assign "Bob" as GM of "Dallas Cowboys"
    ↓
POST /api/users/{bobUserId}/league-roles
    Body: {
        leagueId: 1,
        role: "GeneralManager",
        teamId: 5
    }
    ↓
UsersController.AssignLeagueRole(bobUserId, request)
    ↓
Validate:
    1. Current user is Commissioner of league OR Global Admin
    2. Bob exists as a User
    3. League exists
    4. Team exists and belongs to league
    5. Bob doesn't already have this exact role
    ↓
Create UserLeagueRole:
        UserId = bobUserId
        LeagueId = 1
        Role = GeneralManager
        TeamId = 5
    ↓
Save to database
    ↓
Return updated UserDto with new role
```

**Result:** Bob can now manage the Dallas Cowboys roster.

---

#### Scenario C: Global Admin Promotes a Commissioner

```
God user wants to promote "Alice" to Commissioner of "NFL 2025"
    ↓
POST /api/users/{aliceUserId}/league-roles
    Body: {
        leagueId: 1,
        role: "Commissioner",
        teamId: null
    }
    ↓
Authorization check: User.IsGlobalAdmin = true
    ↓
Create UserLeagueRole:
        UserId = aliceUserId
        LeagueId = 1
        Role = Commissioner
        TeamId = null
    ↓
Save to database
```

**Result:** Alice can now manage all teams in NFL 2025 league.

---

### 3. Role Removal

```
Commissioner wants to remove GM role from "Bob"
    ↓
DELETE /api/users/{bobUserId}/league-roles/{roleId}
    ↓
UsersController.RemoveLeagueRole(bobUserId, roleId)
    ↓
Validate:
    1. Current user is Commissioner of the role's league OR Global Admin
    2. Role exists and belongs to Bob
    ↓
Soft delete the role:
        UserLeagueRole.IsDeleted = true
        UserLeagueRole.DeletedAt = DateTime.UtcNow
    ↓
Save to database
    ↓
Return updated UserDto (role no longer in LeagueRoles list)
```

**Result:** Bob loses access to the team. Role is soft-deleted (preserved in database for audit trail).

---

### 4. Multi-League Participation

Users can have **different roles in different leagues**:

**Example: User "Charlie"**
```csharp
User: Charlie (ID: 123, AzureAdObjectId: "abc-def-ghi")
LeagueRoles:
    1. { LeagueId: 1, Role: Commissioner, TeamId: null }
       → Charlie is Commissioner of "NFL 2025"

    2. { LeagueId: 2, Role: GeneralManager, TeamId: 15 }
       → Charlie is GM of "Patriots" in "Fantasy League 2025"

    3. { LeagueId: 3, Role: GeneralManager, TeamId: 42 }
       → Charlie is GM of "Bears" in "Retro Bowl League"
```

**Access Summary:**
- Full access to ALL teams in NFL 2025 (Commissioner)
- Access only to Patriots in Fantasy League 2025 (GM)
- Access only to Bears in Retro Bowl League (GM)
- Cannot access any other leagues

---

## Access Control Examples

### Example 1: User Tries to View a Team

```csharp
// TeamsController.cs
public async Task<ActionResult<TeamDto>> GetTeam(int id) {
    // Extract user identity from JWT
    var azureAdObjectId = HttpContext.GetAzureAdObjectId();

    // Check authorization BEFORE accessing database
    var hasAccess = await _authorizationService.CanAccessTeamAsync(
        azureAdObjectId,
        id
    );

    if (!hasAccess) {
        return Forbid();  // 403 Forbidden
    }

    // Authorization passed - return team data
    var team = await _teamRepository.GetByIdAsync(id);
    return Ok(MapToDto(team));
}
```

**Authorization Logic (CanAccessTeamAsync):**
```csharp
public async Task<bool> CanAccessTeamAsync(string azureAdObjectId, int teamId) {
    var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);

    // God can access everything
    if (user.IsGlobalAdmin) {
        return true;
    }

    // Navigate hierarchy: Team → Division → Conference → League
    var team = await _teamRepository.GetByIdAsync(teamId);
    var division = await _divisionRepository.GetByIdAsync(team.DivisionId);
    var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
    var leagueId = conference.LeagueId;

    // Check if user has any role in this league
    var role = user.LeagueRoles.FirstOrDefault(lr =>
        lr.LeagueId == leagueId && !lr.IsDeleted
    );

    if (role == null) {
        return false;  // User has no role in this league
    }

    // Commissioner can access all teams in their league
    if (role.Role == UserRole.Commissioner) {
        return true;
    }

    // GM can only access their specific team
    if (role.Role == UserRole.GeneralManager) {
        return role.TeamId == teamId;
    }

    return false;
}
```

---

### Example 2: Filtering Data by Access

```csharp
// TeamsController.cs
public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams() {
    var azureAdObjectId = HttpContext.GetAzureAdObjectId();

    // Check if user is God
    var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

    var teams = await _teamRepository.GetAllAsync();
    List<Team> filteredTeams;

    if (isGlobalAdmin) {
        // God sees all teams
        filteredTeams = teams;
    } else {
        // Regular users only see teams they have access to
        var accessibleTeamIds = await _authorizationService.GetAccessibleTeamIdsAsync(
            azureAdObjectId
        );
        filteredTeams = teams.Where(t => accessibleTeamIds.Contains(t.Id)).ToList();
    }

    return Ok(filteredTeams.Select(MapToDto));
}
```

**GetAccessibleTeamIdsAsync Logic:**
```csharp
public async Task<List<int>> GetAccessibleTeamIdsAsync(string azureAdObjectId) {
    var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);

    if (user.IsGlobalAdmin) {
        // God can access all teams
        var allTeams = await _teamRepository.GetAllAsync();
        return allTeams.Select(t => t.Id).ToList();
    }

    var accessibleTeamIds = new List<int>();

    foreach (var role in user.LeagueRoles.Where(lr => !lr.IsDeleted)) {
        if (role.Role == UserRole.Commissioner) {
            // Commissioners can access all teams in their league
            var leagueTeams = await GetAllTeamsInLeagueAsync(role.LeagueId);
            accessibleTeamIds.AddRange(leagueTeams.Select(t => t.Id));
        }
        else if (role.Role == UserRole.GeneralManager && role.TeamId.HasValue) {
            // GMs can only access their specific team
            accessibleTeamIds.Add(role.TeamId.Value);
        }
    }

    return accessibleTeamIds.Distinct().ToList();
}
```

---

### Example 3: Authorization Matrix

| Action | God | Commissioner | GM | No Role |
|--------|-----|--------------|-----|---------|
| **View own profile** | ✅ | ✅ | ✅ | ✅ |
| **View all leagues** | ✅ | ✅ Their leagues only | ✅ Their leagues only | ❌ |
| **Create league** | ✅ | ✅ | ✅ | ✅ |
| **Delete league** | ✅ | ✅ Their league only | ❌ | ❌ |
| **View all teams** | ✅ | ✅ Their league only | ✅ Their team only | ❌ |
| **Create team** | ✅ | ✅ In their league | ❌ | ❌ |
| **Edit team roster** | ✅ | ✅ Their league | ✅ Their team only | ❌ |
| **View all players** | ✅ | ✅ Their league | ✅ Their team only | ❌ |
| **Generate draft class** | ✅ | ✅ | ✅ | ✅ |
| **Simulate game** | ✅ | ✅ Their league | ✅ If their team plays | ❌ |
| **View users in league** | ✅ | ✅ Their league | ✅ Their league (read-only) | ❌ |
| **Assign GM role** | ✅ | ✅ In their league | ❌ | ❌ |
| **Assign Commissioner** | ✅ | ✅ In their league | ❌ | ❌ |
| **Remove role** | ✅ | ✅ In their league | ❌ | ❌ |
| **Promote to God** | ✅ | ❌ | ❌ | ❌ |

---

## API Endpoints

### User Management

#### Get Current User
```http
GET /api/users/me
Authorization: Bearer <JWT>

Response 200:
{
  "id": 1,
  "email": "user@example.com",
  "displayName": "John Smith",
  "isGlobalAdmin": false,
  "createdAt": "2025-01-01T00:00:00Z",
  "lastLoginAt": "2025-01-15T10:30:00Z",
  "leagueRoles": [
    {
      "id": 1,
      "leagueId": 1,
      "leagueName": "NFL 2025",
      "role": "Commissioner",
      "teamId": null,
      "teamName": null,
      "assignedAt": "2025-01-01T00:00:00Z"
    },
    {
      "id": 2,
      "leagueId": 2,
      "leagueName": "Fantasy League",
      "role": "GeneralManager",
      "teamId": 15,
      "teamName": "Patriots",
      "assignedAt": "2025-01-02T00:00:00Z"
    }
  ]
}
```

#### Get Users in League
```http
GET /api/users/league/{leagueId}
Authorization: Bearer <JWT>

Requirements: User must have access to the league (Commissioner, GM, or God)

Response 200: Array of UserDto (only users with roles in that league)
```

#### Assign League Role
```http
POST /api/users/{userId}/league-roles
Authorization: Bearer <JWT>
Content-Type: application/json

Body:
{
  "leagueId": 1,
  "role": "GeneralManager",  // or "Commissioner"
  "teamId": 5  // Required for GeneralManager, null for Commissioner
}

Requirements:
- User must be Global Admin OR Commissioner of the league

Response 200: Updated UserDto with new role
Response 400: Validation error (invalid role, missing teamId, duplicate role)
Response 403: Not authorized (not Commissioner or God)
Response 404: User, league, or team not found
```

#### Remove League Role
```http
DELETE /api/users/{userId}/league-roles/{roleId}
Authorization: Bearer <JWT>

Requirements: User must be Global Admin OR Commissioner of the league

Response 200: Updated UserDto with role removed (soft-deleted)
Response 403: Not authorized
Response 404: User or role not found
```

---

### League Management

#### Create League
```http
POST /api/leagues-management
Authorization: Bearer <JWT>
Content-Type: application/json

Body:
{
  "name": "NFL 2025",
  "numberOfConferences": 2,
  "divisionsPerConference": 4,
  "teamsPerDivision": 4
}

Auto-assigns: Creator becomes Commissioner of the league

Response 201: LeagueDto with full hierarchy
```

#### Get All Leagues (Filtered)
```http
GET /api/leagues-management
Authorization: Bearer <JWT>

Returns: Only leagues user has access to
- God: All leagues
- Commissioner: Their leagues
- GM: Leagues where they are GM
- No role: Empty array
```

---

### Team Management

#### Get Teams (Filtered)
```http
GET /api/teams
Authorization: Bearer <JWT>

Returns: Only teams user has access to
- God: All teams
- Commissioner: All teams in their leagues
- GM: Only their team(s)
```

#### Get Team Roster
```http
GET /api/teams/{id}/roster
Authorization: Bearer <JWT>

Requirements: User must have access to the team

Response 200: TeamDetailDto with full roster and depth charts
Response 403: No access to this team
Response 404: Team not found
```

---

## Frontend Integration

### Authentication Setup

**App.tsx (MSAL Configuration):**
```tsx
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';

const msalConfig = {
  auth: {
    clientId: '29348959-a014-4550-b3c3-044585c83f0a',
    authority: 'https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com',
    redirectUri: 'http://localhost:3000',
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  }
};

const msalInstance = new PublicClientApplication(msalConfig);

export function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/teams" element={<ProtectedRoute><TeamsPage /></ProtectedRoute>} />
        </Routes>
      </Router>
    </MsalProvider>
  );
}
```

---

### Protected Routes

**ProtectedRoute Component:**
```tsx
import { useMsal } from '@azure/msal-react';
import { Navigate } from 'react-router-dom';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { accounts } = useMsal();

  if (accounts.length === 0) {
    // User not authenticated - redirect to login
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
```

---

### Making Authenticated API Calls

**client.ts (Axios with MSAL):**
```tsx
import axios from 'axios';
import { msalInstance } from './msalConfig';

const client = axios.create({
  baseURL: '/api',
});

// Intercept requests to add JWT token
client.interceptors.request.use(async (config) => {
  const accounts = msalInstance.getAllAccounts();

  if (accounts.length > 0) {
    try {
      const response = await msalInstance.acquireTokenSilent({
        scopes: ['openid', 'profile', 'email'],
        account: accounts[0],
      });

      config.headers.Authorization = `Bearer ${response.accessToken}`;
    } catch (error) {
      // Token expired - redirect to login
      await msalInstance.loginRedirect();
    }
  }

  return config;
});

export default client;
```

---

### Checking User Roles in Frontend

**useCurrentUser Hook:**
```tsx
import { useQuery } from '@tanstack/react-query';
import client from './client';

export function useCurrentUser() {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: async () => {
      const { data } = await client.get('/users/me');
      return data;
    },
  });
}
```

**Using Role Checks in Components:**
```tsx
export function TeamDashboard() {
  const { data: user } = useCurrentUser();

  // Check if user is God
  const isGod = user?.isGlobalAdmin === true;

  // Check if user is Commissioner of current league
  const isCommissioner = user?.leagueRoles?.some(
    role => role.leagueId === currentLeagueId && role.role === 'Commissioner'
  );

  // Check if user is GM of current team
  const isGM = user?.leagueRoles?.some(
    role => role.teamId === currentTeamId && role.role === 'GeneralManager'
  );

  return (
    <div>
      {(isGod || isCommissioner) && (
        <button>Delete Team</button>  // Only show for God/Commissioner
      )}

      {(isGod || isCommissioner || isGM) && (
        <button>Edit Roster</button>  // Show for team managers
      )}
    </div>
  );
}
```

---

## Database Schema

### User Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AzureAdObjectId NVARCHAR(255) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(255) NOT NULL,
    IsGlobalAdmin BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(255) NULL,
    DeletionReason NVARCHAR(MAX) NULL
);

CREATE INDEX IX_Users_AzureAdObjectId ON Users(AzureAdObjectId);
CREATE INDEX IX_Users_IsGlobalAdmin ON Users(IsGlobalAdmin) WHERE IsGlobalAdmin = 1;
```

### UserLeagueRole Table
```sql
CREATE TABLE UserLeagueRoles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    LeagueId INT NOT NULL,
    Role INT NOT NULL,  -- 0 = Commissioner, 1 = GeneralManager
    TeamId INT NULL,    -- NULL for Commissioner, required for GM
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(255) NULL,
    DeletionReason NVARCHAR(MAX) NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (LeagueId) REFERENCES Leagues(Id),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),

    CONSTRAINT UQ_UserLeagueRole UNIQUE (UserId, LeagueId, Role, TeamId)
);

CREATE INDEX IX_UserLeagueRoles_UserId ON UserLeagueRoles(UserId);
CREATE INDEX IX_UserLeagueRoles_LeagueId ON UserLeagueRoles(LeagueId);
CREATE INDEX IX_UserLeagueRoles_TeamId ON UserLeagueRoles(TeamId) WHERE TeamId IS NOT NULL;
```

---

## Security Best Practices

### 1. Never Trust Client-Side Checks

❌ **Wrong:**
```tsx
// Frontend only check - INSECURE
if (user.isGlobalAdmin) {
  deleteTeam(teamId);  // Backend must still validate!
}
```

✅ **Correct:**
```csharp
// Backend always validates before action
public async Task<ActionResult> DeleteTeam(int id) {
    var azureAdObjectId = HttpContext.GetAzureAdObjectId();

    // Always check authorization on backend
    var isGlobalAdmin = await _authService.IsGlobalAdminAsync(azureAdObjectId);
    var isCommissioner = await _authService.IsCommissionerOfTeamLeagueAsync(
        azureAdObjectId,
        id
    );

    if (!isGlobalAdmin && !isCommissioner) {
        return Forbid();  // 403 Forbidden
    }

    // Authorization passed - proceed with deletion
    await _teamRepository.DeleteAsync(id);
    return NoContent();
}
```

### 2. Use Immutable Identifiers

✅ **Always use `AzureAdObjectId` (oid claim)** for authorization checks, not email.

**Why?**
- Email can change in Azure AD
- oid is immutable and unique
- oid survives email changes, account renames, etc.

### 3. Validate JWT on Every Request

The ASP.NET Core `[Authorize]` attribute automatically:
- Validates JWT signature
- Checks token expiration
- Verifies issuer and audience

```csharp
// Program.cs - JWT validation configured
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com";
        options.Audience = "29348959-a014-4550-b3c3-044585c83f0a";
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

### 4. Soft Delete for Audit Trail

All role removals use **soft delete** to preserve audit history:

```csharp
// NEVER hard delete roles
await _context.UserLeagueRoles.Remove(role);  // ❌ Wrong

// Always soft delete
role.IsDeleted = true;
role.DeletedAt = DateTime.UtcNow;
role.DeletedBy = currentUserEmail;
await _context.SaveChangesAsync();  // ✅ Correct
```

Benefits:
- Can restore accidentally removed roles
- Audit trail for compliance
- Historical data for analytics

### 5. Principle of Least Privilege

- New users start with **zero roles**
- Must be explicitly granted access
- GMs can only see/modify their team
- Commissioners can only manage their league
- God role reserved for system administrators only

### 6. Token Refresh

MSAL automatically handles token refresh:
- Access tokens expire after 1 hour
- MSAL silently refreshes using refresh token
- If refresh fails, user redirected to login

```tsx
// MSAL handles this automatically
const response = await msalInstance.acquireTokenSilent({
  scopes: ['openid', 'profile', 'email'],
  account: accounts[0],
});
// If silent refresh fails, throws InteractionRequiredAuthError
```

---

## Future Enhancements

### Planned Features

1. **Team Ownership Transfer**
   - Allow Commissioner to transfer GM role to another user
   - Maintain audit trail of all owners

2. **League Invitations**
   - Generate invite links for leagues
   - Users can accept invite to join as GM or Commissioner
   - Expiring invite tokens

3. **Role Requests**
   - Users can request to join a league
   - Commissioners approve/deny requests
   - Notification system

4. **Activity Logs**
   - Track all authorization changes
   - "Who did what, when" audit log
   - Searchable log viewer for Commissioners

5. **Multi-Factor Authentication**
   - Optional MFA for God accounts
   - Configurable per-league security policies

6. **API Keys for Automation**
   - Allow God users to generate API keys
   - Useful for scripts, bots, automated tournaments

---

## Support & Troubleshooting

### Common Issues

**Issue:** "User not found in token"
- **Cause:** oid claim missing from JWT
- **Fix:** Check Azure Entra ID app registration claims configuration

**Issue:** "403 Forbidden on all endpoints"
- **Cause:** User has no roles assigned
- **Fix:** Assign user to league as Commissioner or GM via `/api/users/{id}/league-roles`

**Issue:** "Commissioner can't delete team"
- **Cause:** Team belongs to different league
- **Fix:** User must be Commissioner of the team's league, not a different league

**Issue:** "Token expired"
- **Cause:** Access token lifetime exceeded (1 hour)
- **Fix:** MSAL should auto-refresh; if not, user needs to re-authenticate

---

## Summary

Gridiron's authorization system provides:

✅ **Secure**: Azure AD authentication with JWT validation
✅ **Flexible**: Multi-role, multi-league support
✅ **Hierarchical**: God → Commissioner → GM role cascade
✅ **Auditable**: Soft delete preserves history
✅ **Scalable**: Supports unlimited leagues and users
✅ **Standards-Based**: OAuth 2.0 + OIDC + RBAC

The system is production-ready and follows enterprise security best practices.
