# NuGet Authentication Guide

This guide explains how to configure NuGet package authentication for the Gridiron project, which depends on the `Gridiron.Engine` package hosted on GitHub Packages.

## Overview

The Gridiron project uses the `Gridiron.Engine` NuGet package from the `merciless-creations` GitHub organization. GitHub Packages requires authentication to download packages, even from public repositories within the same organization.

## Local Development Setup

### Step 1: Create a Personal Access Token (PAT)

1. Go to [GitHub Settings](https://github.com/settings/tokens)
2. Click **Developer settings** → **Personal access tokens** → **Tokens (classic)**
3. Click **Generate new token (classic)**
4. Configure the token:
   - **Note**: `NuGet package read access` (or similar descriptive name)
   - **Expiration**: 1 year (recommended)
   - **Scopes**: Check only `read:packages`
5. Click **Generate token**
6. **Copy the token immediately** - you won't be able to see it again!

### Step 2: Set the Environment Variable

The `nuget.config` file in this repository is configured to read credentials from the `NUGET_AUTH_TOKEN` environment variable.

**Windows (PowerShell)**
```powershell
# Set for current session
$env:NUGET_AUTH_TOKEN = "ghp_your_token_here"

# Set permanently for user (persists across sessions)
[Environment]::SetEnvironmentVariable("NUGET_AUTH_TOKEN", "ghp_your_token_here", "User")
```

**Windows (Command Prompt)**
```cmd
# Set for current session
set NUGET_AUTH_TOKEN=ghp_your_token_here

# Set permanently (requires admin, or use System Properties)
setx NUGET_AUTH_TOKEN ghp_your_token_here
```

**Linux/macOS**
```bash
# Set for current session
export NUGET_AUTH_TOKEN=ghp_your_token_here

# Add to shell profile for persistence
echo 'export NUGET_AUTH_TOKEN=ghp_your_token_here' >> ~/.bashrc
# or ~/.zshrc for zsh
```

### Step 3: Restore Packages

```bash
dotnet restore
```

If authentication is configured correctly, the `Gridiron.Engine` package will download from GitHub Packages.

## CI/CD Configuration

### GitHub Actions

The CI/CD pipeline uses an organization secret called `NUGET_READ_TOKEN` to authenticate with GitHub Packages.

**How it works:**

1. The `NUGET_READ_TOKEN` secret is stored at the organization level (`merciless-creations`)
2. The workflow explicitly updates the NuGet source with credentials before restore:

```yaml
- name: Configure NuGet authentication
  run: |
    dotnet nuget update source github-merciless \
      --username "github-actions" \
      --password "$NUGET_TOKEN" \
      --store-password-in-clear-text \
      --configfile nuget.config
  env:
    NUGET_TOKEN: ${{ secrets.NUGET_READ_TOKEN }}
```

### Setting Up the Organization Secret

If you need to set up or rotate the `NUGET_READ_TOKEN` secret:

1. Go to [github.com/merciless-creations](https://github.com/merciless-creations)
2. Click **Settings** (organization settings)
3. Navigate to **Secrets and variables** → **Actions**
4. Click **New organization secret** (or edit existing)
5. Configure:
   - **Name**: `NUGET_READ_TOKEN`
   - **Value**: Your PAT with `read:packages` scope
   - **Repository access**: Select repositories that need access (at minimum: `gridiron`)
6. Click **Add secret** or **Update secret**

## nuget.config Structure

The repository contains a `nuget.config` file that defines the GitHub Packages source:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-merciless" value="https://nuget.pkg.github.com/merciless-creations/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-merciless>
      <add key="Username" value="USERNAME" />
      <add key="ClearTextPassword" value="%NUGET_AUTH_TOKEN%" />
    </github-merciless>
  </packageSourceCredentials>
</configuration>
```

**Key points:**
- The `github-merciless` source points to the organization's GitHub Packages feed
- Credentials use the `%NUGET_AUTH_TOKEN%` environment variable
- The `Username` value is not validated by GitHub - any non-empty string works

## Troubleshooting

### Error: 401 Unauthorized

**Symptoms:**
```
error NU1301: Failed to retrieve information about 'Gridiron.Engine'
Response status code does not indicate success: 401 (Unauthorized).
```

**Solutions:**
1. Verify `NUGET_AUTH_TOKEN` environment variable is set:
   ```bash
   # Windows PowerShell
   echo $env:NUGET_AUTH_TOKEN
   
   # Linux/Mac
   echo $NUGET_AUTH_TOKEN
   ```
2. Verify your PAT has `read:packages` scope
3. Verify the PAT hasn't expired
4. Regenerate and update the token if needed

### Error: Package Not Found

**Symptoms:**
```
error NU1102: Unable to find package Gridiron.Engine
```

**Solutions:**
1. Verify the package exists on GitHub Packages
2. Check the package version in your `.csproj` file matches an available version
3. Run `dotnet nuget locals all --clear` to clear the NuGet cache

### CI/CD Failures

**Symptoms:**
- Build passes locally but fails in GitHub Actions
- 401 errors during `dotnet restore` in CI

**Solutions:**
1. Verify `NUGET_READ_TOKEN` organization secret exists and has correct value
2. Verify the secret is enabled for the `gridiron` repository
3. Check the workflow is using the correct secret name
4. Regenerate and update the organization secret if needed

## Security Considerations

1. **Never commit tokens** to the repository
2. **Use minimum required scopes** - only `read:packages` is needed
3. **Set token expiration** - 1 year maximum, with calendar reminder to rotate
4. **Use organization secrets** for CI/CD - not repository secrets
5. **Rotate tokens regularly** - especially if team members leave

## Related Documentation

- [GitHub Packages Documentation](https://docs.github.com/en/packages)
- [NuGet Configuration](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file)
- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)

---

**Last Updated:** November 2025
