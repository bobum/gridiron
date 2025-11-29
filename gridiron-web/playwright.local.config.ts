import { defineConfig, devices } from '@playwright/test'

/**
 * Local E2E test configuration.
 *
 * This config is for running E2E tests locally with proper authentication bypass.
 * It always starts a fresh dev server with VITE_E2E_TEST_MODE=true.
 *
 * Usage:
 *   npm run test:e2e:local
 *
 * Prerequisites:
 *   - API must be running: dotnet run (in Gridiron.WebApi folder)
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: false,
  retries: 0,
  workers: undefined,
  reporter: 'html',
  timeout: 30000,
  expect: {
    timeout: 5000,
  },
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'npm run dev:e2e',
    url: 'http://localhost:3000',
    reuseExistingServer: false, // Always start fresh with E2E mode
    timeout: 60000,
  },
})
