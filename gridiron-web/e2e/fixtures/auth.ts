import { test as base, Page } from '@playwright/test';

/**
 * Fixture that extends Playwright's test to include mock MSAL authentication
 * This automatically authenticates all tests without requiring Azure Entra ID
 */

async function injectMockAuth(page: Page) {
  await page.addInitScript(() => {
    // Mock MSAL account
    const mockAccount = {
      homeAccountId: 'test-user-123.test-tenant',
      environment: 'gtggridiron.ciamlogin.com',
      tenantId: 'test-tenant-id',
      username: 'testuser@gridiron.com',
      localAccountId: 'test-user-123',
      name: 'Test User',
      idTokenClaims: {
        aud: '29348959-a014-4550-b3c3-044585c83f0a',
        iss: 'https://gtggridiron.ciamlogin.com/test-tenant-id/v2.0',
        iat: Math.floor(Date.now() / 1000),
        nbf: Math.floor(Date.now() / 1000),
        exp: Math.floor(Date.now() / 1000) + 3600,
        name: 'Test User',
        preferred_username: 'testuser@gridiron.com',
        oid: 'test-user-123',
        sub: 'test-user-123',
        tid: 'test-tenant-id',
      },
    };

    const mockAccessToken = {
      homeAccountId: 'test-user-123.test-tenant',
      environment: 'gtggridiron.ciamlogin.com',
      credentialType: 'AccessToken',
      clientId: '29348959-a014-4550-b3c3-044585c83f0a',
      secret: 'mock-access-token-for-testing',
      tokenType: 'Bearer',
      realm: 'test-tenant-id',
      target: 'openid profile email',
      cachedAt: Math.floor(Date.now() / 1000).toString(),
      expiresOn: (Math.floor(Date.now() / 1000) + 3600).toString(),
    };

    const mockIdToken = {
      homeAccountId: 'test-user-123.test-tenant',
      environment: 'gtggridiron.ciamlogin.com',
      credentialType: 'IdToken',
      clientId: '29348959-a014-4550-b3c3-044585c83f0a',
      secret: 'mock-id-token-for-testing',
      realm: 'test-tenant-id',
    };

    // Create MSAL cache keys
    const accountKey = `test-user-123.test-tenant-gtggridiron.ciamlogin.com-test-tenant-id`;
    const accessTokenKey = `${accountKey}-accesstoken-29348959-a014-4550-b3c3-044585c83f0a-test-tenant-id-openid profile email---`;
    const idTokenKey = `${accountKey}-idtoken-29348959-a014-4550-b3c3-044585c83f0a-test-tenant-id---`;

    // Store in sessionStorage (MSAL's cache location)
    sessionStorage.setItem(accountKey, JSON.stringify(mockAccount));
    sessionStorage.setItem(accessTokenKey, JSON.stringify(mockAccessToken));
    sessionStorage.setItem(idTokenKey, JSON.stringify(mockIdToken));

    // Store active account
    sessionStorage.setItem('msal.account.keys', JSON.stringify([accountKey]));
  });
}

export const test = base.extend({
  page: async ({ page }, use) => {
    // Inject mock auth before every test
    await injectMockAuth(page);
    await use(page);
  },
});

export { expect } from '@playwright/test';
