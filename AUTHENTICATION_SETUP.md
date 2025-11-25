# Authentication Setup Guide - Microsoft Entra External ID + Google

**Created:** November 24, 2024
**Purpose:** Complete reference for authentication configuration in production
**Status:** Development configuration documented, ready for production deployment

---

## Table of Contents

1. [Overview](#overview)
2. [Microsoft Entra External ID Configuration](#microsoft-entra-external-id-configuration)
3. [Google Cloud Console Configuration](#google-cloud-console-configuration)
4. [Application IDs and Secrets Reference](#application-ids-and-secrets-reference)
5. [Identity Providers Configuration](#identity-providers-configuration)
6. [Production Deployment Checklist](#production-deployment-checklist)
7. [Troubleshooting](#troubleshooting)
8. [Critical Lessons Learned](#critical-lessons-learned)

---

## Overview

### Authentication Architecture

```
User Authentication Flow:
  1. User visits React app (gridiron-web)
  2. Clicks "Sign in" → Redirects to Entra External ID
  3. Chooses identity provider:
     - Email/Password (local account)
     - Microsoft Account
     - Google
  4. Authenticates with chosen provider
  5. Redirects back to React app with authorization code
  6. React app exchanges code for access token (PKCE flow)
  7. Access token sent to ASP.NET Core API in Authorization header
  8. API validates token with Entra External ID
  9. API returns protected resources
```

### Technologies Used

- **Identity Provider:** Microsoft Entra External ID (formerly Azure AD B2C)
- **Social Logins:** Google, Microsoft Account
- **Frontend Auth:** MSAL React (@azure/msal-browser, @azure/msal-react)
- **Backend Auth:** Microsoft.Identity.Web (ASP.NET Core 8.0)
- **OAuth Flow:** Authorization Code + PKCE (for SPA security)

---

## Microsoft Entra External ID Configuration

### Tenant Information

| Property | Value |
|----------|-------|
| **Tenant Name** | `gtggridiron` |
| **Full Domain** | `gtggridiron.onmicrosoft.com` |
| **Tenant Type** | External Tenant (CIAM - Customer Identity Access Management) |
| **Login Endpoint** | `https://gtggridiron.ciamlogin.com` |
| **Azure Portal** | https://portal.azure.com |

**How to Access Tenant:**
1. Go to: https://portal.azure.com
2. Click profile icon (top right) → "Switch directory"
3. Select: **gtg-gridiron** (External Tenant)

---

### App Registrations

#### 1. React SPA Application

**Name:** `Goal To Go Football`
**Application (client) ID:** `29348959-a014-4550-b3c3-044585c83f0a`
**Platform Type:** Single-page application (SPA)

**Redirect URIs (Configured):**
- Development: `http://localhost:5173/`
- Production: `https://yourdomain.com/` (add when deployed)

**Front-channel Logout URL:**
- Leave empty for development (HTTP not allowed)
- Production: `https://yourdomain.com/` (add when deployed)

**Token Configuration:**
- Optional claims added: `email`, `given_name`, `family_name`
- ID token lifetime: 1 hour (default)
- Access token lifetime: 1 hour (default)
- Refresh token lifetime: 24 hours (SPA default)

**API Permissions:**
- Microsoft Graph: `User.Read` (delegated)
- Gridiron-API: `Teams.Read`, `Teams.Write`, `Leagues.Manage` (when API registered)

**Path to Configure:**
```
Microsoft Entra ID → App registrations → Goal To Go Football
```

---

#### 2. Web API Application (To Be Created)

**Name:** `Gridiron-API` (not yet created)
**Application (client) ID:** TBD
**Application ID URI:** `api://gridiron-api`

**Exposed API Scopes (To Configure):**
- `api://gridiron-api/Teams.Read` - Read team and player data
- `api://gridiron-api/Teams.Write` - Manage teams and simulate games
- `api://gridiron-api/Leagues.Manage` - Create and manage leagues

**Path to Create:**
```
Microsoft Entra ID → App registrations → + New registration
```

---

### User Flow Configuration

**User Flow Name:** `SignUpSignIn`
**Type:** Sign up and sign in (combined flow)
**Version:** Recommended

**Identity Providers Enabled:**
- ✅ Email accounts (local accounts with password)
- ✅ Microsoft Account (personal Microsoft accounts)
- ✅ Google (federated identity)
- ❌ Facebook (skipped for MVP)

**User Attributes Collected:**
| Attribute | Collect During Sign-up | Return in Token |
|-----------|----------------------|-----------------|
| Email Address | ✅ Required | ✅ Yes |
| Display Name | ✅ Required | ✅ Yes |
| Given Name | ☑️ Optional | ✅ Yes |
| Surname | ☑️ Optional | ✅ Yes |

**Multi-Factor Authentication:** Optional (not required)

**Conditional Access:** Not configured (requires Premium P1)

**Linked Applications:**
- Goal To Go Football (SPA)

**Path to Configure:**
```
Microsoft Entra ID → External Identities → User flows → SignUpSignIn
```

**Test User Flow:**
```
External Identities → User flows → SignUpSignIn → "Run user flow"
```

---

### Company Branding (Optional)

**Path to Configure:**
```
Microsoft Entra ID → User experiences → Company branding
```

**Recommended Settings:**
- Banner logo: Upload your logo (280x60px)
- Background image: Upload background (1920x1080px)
- Sign-in page text: "Sign in to Gridiron Football - Manage your teams and leagues"
- Privacy policy URL: `https://yourdomain.com/privacy`
- Terms of service URL: `https://yourdomain.com/terms`

---

## Google Cloud Console Configuration

### Project Information

| Property | Value |
|----------|-------|
| **Project Name** | `gOAL TO gO fOOTBALL` |
| **Project ID** | `goal-to-go-football` |
| **Google Cloud Console** | https://console.cloud.google.com |

**How to Access:**
1. Go to: https://console.cloud.google.com
2. Select project: **gOAL TO gO fOOTBALL** (from dropdown at top)

---

### OAuth 2.0 Client ID

**Name:** `Goal To Go Football - Entra ID`
**Type:** Web application
**Client ID:** `224957018220-53l2di79kq3q397d52kot4buc0gs8b2t.apps.googleusercontent.com`
**Client Secret:** (stored in Azure - see below)

**Authorized Redirect URIs (CRITICAL!):**

⚠️ **These MUST be exact - including the `/federation/oauth2` path**

```
https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com/federation/oauth2
https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com/oauth2/v2.0/authresp
https://login.microsoftonline.com/te/gtggridiron.onmicrosoft.com/oauth2/authresp
```

**Why Multiple URIs?**
- First URI (`/federation/oauth2`) is what Entra External ID actually sends
- Other URIs are fallbacks for different configurations

**Path to Configure:**
```
Google Cloud Console → APIs & Services → Credentials → Goal To Go Football - Entra ID
```

---

### OAuth Consent Screen

**Publishing Status:** In production
**User Type:** External (any Google account can sign in)

**App Information:**
- App name: `Gridiron Football`
- User support email: Your email
- Developer contact: Your email

**App Domain:**
- Application home page: `https://www.microsoftonline.com`
- Privacy policy: `https://www.microsoftonline.com`
- Terms of service: `https://www.microsoftonline.com`

**Authorized Domains (CRITICAL!):**
```
microsoftonline.com
ciamlogin.com
```

**Scopes Requested:**
- `openid` (automatically included)
- `profile` (automatically included)
- `email` (automatically included)

**OAuth User Cap:**
- Default limit applies (not a concern for consumer apps)

**Path to Configure:**
```
Google Cloud Console → APIs & Services → OAuth consent screen
```

---

### APIs Enabled

Required APIs for OAuth to work:

1. **Google+ API** (even though deprecated, sometimes required)
   - Path: APIs & Services → Library → Search "Google+ API" → Enable

2. **People API** (for profile information)
   - Path: APIs & Services → Library → Search "People API" → Enable

---

## Application IDs and Secrets Reference

### Where Each ID/Secret Is Used

| Credential | Used In | Configuration File | Environment Variable |
|------------|---------|-------------------|---------------------|
| **SPA Client ID** | React Frontend | `.env` | `VITE_ENTRA_CLIENT_ID` |
| **Tenant ID** | React Frontend | `.env` | `VITE_ENTRA_TENANT_ID` |
| **Tenant Name** | React Frontend | `.env` | `VITE_ENTRA_TENANT_NAME` |
| **API Client ID** | ASP.NET API | `appsettings.json` or User Secrets | `AzureAd:ClientId` |
| **Tenant ID** | ASP.NET API | `appsettings.json` or User Secrets | `AzureAd:TenantId` |
| **Google Client ID** | Azure Portal | External Identities → Google | N/A (stored in Azure) |
| **Google Client Secret** | Azure Portal | External Identities → Google | N/A (stored in Azure) |

---

### React Frontend Configuration (.env file)

**File:** `gridiron-web/.env`

```env
# Microsoft Entra External ID Configuration
VITE_ENTRA_CLIENT_ID=29348959-a014-4550-b3c3-044585c83f0a
VITE_ENTRA_TENANT_ID=<your-tenant-id-guid>
VITE_ENTRA_TENANT_NAME=gtggridiron

# API Configuration
VITE_API_URL=http://localhost:5000/api
```

**Production (.env.production):**
```env
VITE_ENTRA_CLIENT_ID=29348959-a014-4550-b3c3-044585c83f0a
VITE_ENTRA_TENANT_ID=<your-tenant-id-guid>
VITE_ENTRA_TENANT_NAME=gtggridiron
VITE_API_URL=https://api.yourdomain.com/api
```

**Security:**
- ✅ Add `.env` to `.gitignore`
- ✅ Create `.env.example` as template (without real values)
- ✅ Never commit real credentials to git

---

### ASP.NET Core API Configuration

**File:** `Gridiron.WebApi/appsettings.json`

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<your-tenant-id-guid>",
    "ClientId": "<api-client-id>",
    "Audience": "api://gridiron-api"
  },
  "ConnectionStrings": {
    "GridironDb": "..."
  },
  "Logging": {
    ...
  }
}
```

**Recommended: Use User Secrets for Development**

```bash
dotnet user-secrets init --project Gridiron.WebApi
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>" --project Gridiron.WebApi
dotnet user-secrets set "AzureAd:ClientId" "<api-client-id>" --project Gridiron.WebApi
```

**Production: Use Environment Variables or Azure Key Vault**

```bash
# Environment variables
AzureAd__TenantId=<tenant-id>
AzureAd__ClientId=<api-client-id>
AzureAd__Audience=api://gridiron-api
```

---

## Identity Providers Configuration

### Email with Password (Local Accounts)

**Status:** ✅ Enabled by default

**Configuration:**
- Users create accounts with email/password
- Email verification: Automatic (user receives verification email)
- Password requirements:
  - Minimum 8 characters
  - Must contain 3 of 4: uppercase, lowercase, number, special character
- Password reset: User flow handles automatically

**No additional configuration needed.**

---

### Microsoft Account

**Status:** ✅ Enabled

**Configuration:**
- Built-in to External Tenants
- No external setup required
- Allows users to sign in with:
  - Personal Microsoft accounts (outlook.com, hotmail.com, live.com)
  - Work/school Microsoft accounts

**Path to Enable:**
```
External Identities → User flows → SignUpSignIn → Identity providers → Check "Microsoft Account"
```

---

### Google Identity Provider

**Status:** ✅ Configured

**In Azure Portal:**

**Path:**
```
External Identities → All identity providers → Google
```

**Configuration:**
- Name: `Google`
- Client ID: `224957018220-53l2di79kq3q397d52kot4buc0gs8b2t.apps.googleusercontent.com`
- Client Secret: (paste from Google Cloud Console)

**In User Flow:**

**Path:**
```
External Identities → User flows → SignUpSignIn → Identity providers → Check "Google"
```

---

## Production Deployment Checklist

### Microsoft Entra External ID Updates

- [ ] **Update SPA redirect URIs** to production domain
  - Add: `https://yourdomain.com/`
  - Add: `https://www.yourdomain.com/` (if using www)
  - Keep localhost URIs for local development

- [ ] **Add front-channel logout URL** (now that HTTPS is available)
  - Add: `https://yourdomain.com/`

- [ ] **Create API app registration** (Gridiron-API)
  - Expose API with scopes
  - Note Client ID for API configuration

- [ ] **Grant API permissions** to SPA
  - Add Gridiron-API scopes to Goal To Go Football app
  - Grant admin consent

- [ ] **Update company branding** (optional)
  - Add production privacy policy URL
  - Add production terms of service URL

- [ ] **Configure custom domain** (optional)
  - Set up: `login.yourdomain.com`
  - Requires DNS configuration

---

### Google Cloud Console Updates

- [ ] **Add production redirect URI** to OAuth client
  - Add: `https://yourdomain.com/federation/oauth2` (if domain changes)
  - Pattern: `https://<production-domain>/federation/oauth2`

- [ ] **Update OAuth consent screen**
  - Change application home page to production URL
  - Add production privacy policy URL
  - Add production terms of service URL

- [ ] **Verify app status**
  - Should be "In production" (already set)
  - No action needed unless you want Google verification

- [ ] **Optional: Submit for Google verification**
  - Removes "unverified app" warning
  - Required if requesting sensitive scopes beyond openid/profile/email
  - Process: Google Cloud Console → OAuth consent screen → "Submit for verification"

---

### Application Configuration Updates

- [ ] **React App (.env.production)**
  - Update `VITE_API_URL` to production API URL
  - Update `VITE_ENTRA_CLIENT_ID` if needed
  - Verify tenant ID and name

- [ ] **ASP.NET Core API**
  - Set environment variables or Azure Key Vault:
    - `AzureAd:TenantId`
    - `AzureAd:ClientId` (API client ID, not SPA client ID)
    - `AzureAd:Audience`
  - Update CORS policy to production domain(s)
  - Ensure HTTPS only (no HTTP in production)

- [ ] **Database**
  - Add User and UserTeam tables (migration)
  - Seed any necessary data

---

### Testing Checklist

- [ ] **Test local accounts**
  - Create new account with email/password
  - Verify email verification works
  - Test login with created account
  - Test password reset flow

- [ ] **Test Microsoft Account**
  - Sign in with personal Microsoft account
  - Verify user record created in database
  - Test logout and re-login

- [ ] **Test Google sign-in**
  - Sign in with Google account
  - Verify user record created in database
  - Test logout and re-login

- [ ] **Test API authorization**
  - Protected endpoints require valid token
  - Invalid token returns 401 Unauthorized
  - Missing scopes return 403 Forbidden
  - User can only access their own resources

- [ ] **Test token refresh**
  - Stay logged in > 1 hour
  - Token should auto-refresh
  - No disruption to user

- [ ] **Test on multiple browsers**
  - Chrome
  - Firefox
  - Safari
  - Edge
  - Mobile browsers (iOS Safari, Android Chrome)

---

## Troubleshooting

### Google Sign-In Issues

#### Error: "redirect_uri_mismatch"

**Cause:** Redirect URI in Google doesn't match what Azure is sending.

**Solution:**
1. Look at error page URL in browser
2. Find `redirect_uri=` parameter (URL-encoded)
3. Decode it (replace `%3A` with `:`, `%2F` with `/`)
4. Add exact URI to Google Cloud Console → Credentials → Authorized redirect URIs
5. Wait 5 minutes for propagation
6. Test in incognito browser

**For gtggridiron tenant, the correct URI is:**
```
https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com/federation/oauth2
```

#### Error: "Access blocked: This app's request is invalid"

**Possible causes:**
1. Redirect URI mismatch (see above)
2. Missing authorized domains in OAuth consent screen
3. Client ID/Secret mismatch between Google and Azure

**Solution:**
- Verify authorized domains: `microsoftonline.com`, `ciamlogin.com`
- Re-copy Client ID and Secret from Google → Paste into Azure
- Wait 5-10 minutes after changes

#### Error: "App is not verified"

**Cause:** Google shows this warning for apps not verified by Google.

**Temporary solution:** Users click "Advanced" → "Go to Gridiron Football (unsafe)"

**Permanent solution:** Submit app for Google verification (optional, not required)

---

### Microsoft Account Sign-In Issues

#### Users can't sign in with Microsoft Account

**Check:**
1. Microsoft Account is enabled in user flow
2. Path: External Identities → User flows → SignUpSignIn → Identity providers
3. Verify "Microsoft Account" is checked

---

### API Authentication Issues

#### Error: 401 Unauthorized when calling API

**Causes:**
1. Token not included in request
2. Token expired
3. API not configured to validate tokens

**Check:**
- Browser DevTools → Network tab → Look for `Authorization: Bearer <token>` header
- Verify API has `app.UseAuthentication()` before `app.UseAuthorization()`
- Check `appsettings.json` has correct `AzureAd` section

#### Error: 403 Forbidden for specific endpoints

**Cause:** User doesn't have required scope/permission.

**Check:**
- API authorization policy requires specific scope
- SPA requested and received that scope in token
- Token claims include the scope (decode JWT at jwt.ms)

---

### Token Issues

#### Tokens expire too quickly

**Default lifetimes:**
- Access token: 1 hour
- Refresh token: 24 hours (SPAs)

**To extend:** Requires Azure AD Premium P1 licensing + Conditional Access policies

#### Token refresh fails

**Causes:**
1. Refresh token expired (24 hours for SPAs)
2. Browser blocking third-party cookies (Safari, Firefox)

**Solution:**
- User must re-authenticate after 24 hours (by design)
- For third-party cookie issues: MSAL will fall back to redirect flow

---

## Critical Lessons Learned

### Microsoft Entra UX Issues

**Problem:** Azure Portal doesn't display the redirect URI for identity providers.

**Impact:** Developers must:
- Guess the format
- Decode error URLs
- Search documentation
- Trial and error

**Workaround:** When configuring Google (or any OAuth provider):
1. Set up the provider in Azure
2. Attempt sign-in
3. Look at error URL in browser
4. Find and decode `redirect_uri=` parameter
5. Add that exact URI to the external OAuth provider

**For External Tenants, the format is:**
```
https://{tenant-name}.ciamlogin.com/{tenant-name}.onmicrosoft.com/federation/oauth2
```

**This is NOT documented in the Azure Portal UI.** 😞

---

### External Tenants vs. Workforce Tenants

**External Tenants (CIAM):**
- Use `.ciamlogin.com` domain
- Use `/federation/oauth2` path for social providers
- Designed for consumer apps
- Free for first 50,000 monthly active users

**Workforce Tenants (Traditional Azure AD):**
- Use `login.microsoftonline.com/te/` domain
- Use `/oauth2/authresp` path
- Designed for employees/enterprise
- Different pricing model

**Critical:** The redirect URI format is DIFFERENT between tenant types.

---

### Google OAuth Consent Screen

**Key requirements:**
1. **Authorized domains** MUST include:
   - `microsoftonline.com`
   - `ciamlogin.com`

2. **Redirect URIs** must be EXACT (character-for-character)
   - No trailing slashes unless Azure sends them
   - Case-sensitive
   - Path must match exactly

3. **Publishing status:**
   - "Testing" mode: Only test users can sign in
   - "In production" mode: Anyone can sign in (shows "unverified" warning)

---

### PKCE for SPAs

**Critical:** Single-page applications MUST use Authorization Code + PKCE flow.

**Why:**
- Implicit flow is deprecated (security vulnerabilities)
- SPAs can't securely store client secrets
- PKCE protects against authorization code interception

**MSAL React handles this automatically.**

---

### Token Storage

**Best practice:** Store tokens in sessionStorage (not localStorage)

**Why:**
- sessionStorage cleared when tab closes
- localStorage persists across sessions (XSS risk)
- MSAL React uses sessionStorage by default

**Security:**
- Never put tokens in URL parameters
- Never store tokens in cookies (unless httpOnly + secure)
- Always send tokens in `Authorization: Bearer` header

---

### CORS Configuration

**Development:**
```csharp
policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
```

**Production:**
```csharp
policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
```

**Critical:** Must include `.AllowCredentials()` for authentication to work.

---

## Additional Resources

### Microsoft Documentation

- [Entra External ID Overview](https://learn.microsoft.com/en-us/entra/external-id/customers/overview-customers-ciam)
- [Add Google Federation](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-google-federation-customers)
- [MSAL React Documentation](https://learn.microsoft.com/en-us/entra/identity-platform/msal-react)
- [Microsoft.Identity.Web](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identity.web)

### Google Documentation

- [OAuth 2.0 for Web Apps](https://developers.google.com/identity/protocols/oauth2/web-server)
- [OAuth Consent Screen](https://support.google.com/cloud/answer/10311615)

### Debugging Tools

- [JWT Decoder](https://jwt.ms) - Decode and inspect tokens
- [URL Decoder](https://www.urldecoder.org/) - Decode redirect URIs from error URLs
- Browser DevTools Network tab - Inspect OAuth requests

---

## Document Maintenance

**Last Updated:** November 24, 2024
**Next Review:** Before production deployment
**Owner:** Development Team

**Update this document when:**
- Adding new identity providers
- Changing tenant configuration
- Deploying to production
- Discovering new troubleshooting steps
- Microsoft changes Entra External ID configuration

---

## Quick Reference Card

### Essential URLs

| Service | URL |
|---------|-----|
| Azure Portal | https://portal.azure.com |
| Google Cloud Console | https://console.cloud.google.com |
| Test User Flow | External Identities → User flows → SignUpSignIn → Run user flow |
| JWT Decoder | https://jwt.ms |

### Essential IDs

| Name | Value |
|------|-------|
| Tenant Name | `gtggridiron` |
| SPA Client ID | `29348959-a014-4550-b3c3-044585c83f0a` |
| Google Client ID | `224957018220-53l2di79kq3q397d52kot4buc0gs8b2t.apps.googleusercontent.com` |

### Critical Redirect URI

```
https://gtggridiron.ciamlogin.com/gtggridiron.onmicrosoft.com/federation/oauth2
```

**This MUST be in Google Cloud Console → Credentials → Authorized redirect URIs**

---

**End of Document**
