using System.Security.Claims;

namespace Gridiron.WebApi.Extensions;

/// <summary>
/// Extension methods for HttpContext to extract user information from JWT claims
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the Azure AD Object ID (oid claim) from the current user's JWT token.
    /// This is the unique identifier for the user in Azure Entra ID.
    /// In E2E test mode, returns a special test user ID to bypass authentication.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>Azure AD Object ID, or null if not authenticated or claim not found</returns>
    public static string? GetAzureAdObjectId(this HttpContext context)
    {
        // Check if we're in E2E test mode
        var isE2ETestMode = Environment.GetEnvironmentVariable("E2E_TEST_MODE") == "true";
        if (isE2ETestMode)
        {
            // Return a fake Azure AD Object ID for E2E tests
            // This allows the application to function without real authentication
            return "e2e-test-user-object-id";
        }

        // The 'oid' claim contains the user's unique object ID in Azure AD
        // This is the immutable identifier we use for authorization
        // Azure AD v2.0 tokens use different claim names depending on configuration:
        // - "oid" (short name when MapInboundClaims = false)
        // - "http://schemas.microsoft.com/identity/claims/objectidentifier" (full URI)
        // - "sub" (subject claim, often same as oid in v2.0 tokens)
        return context.User?.FindFirst("oid")?.Value
            ?? context.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the user's email from JWT claims
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>Email address, or null if not found</returns>
    public static string? GetUserEmail(this HttpContext context)
    {
        return context.User?.FindFirst("email")?.Value
            ?? context.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the user's display name from JWT claims
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>Display name, or null if not found</returns>
    public static string? GetUserDisplayName(this HttpContext context)
    {
        return context.User?.FindFirst("name")?.Value
            ?? context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the user is authenticated
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>True if authenticated, false otherwise</returns>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.User?.Identity?.IsAuthenticated ?? false;
    }
}
