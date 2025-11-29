/**
 * Critical User Journeys E2E Tests
 *
 * These are the TRUE end-to-end tests that verify critical full-stack user flows.
 * They require a real API and database to run.
 *
 * For UI-only tests (modals, form validation, navigation), see the Vitest
 * integration tests in src/pages/__tests__/*.test.tsx
 */
import { test, expect } from '@playwright/test'

test.describe('Critical User Journeys', () => {
  test.describe.configure({ mode: 'serial' })

  test('User Journey: App loads and navigation works', async ({ page }) => {
    // Start at home page
    await page.goto('/')
    await expect(page.getByRole('heading', { name: 'Gridiron Football Manager' })).toBeVisible()

    // Can navigate to Teams (which loads from API)
    await page.click('text=Teams')
    await expect(page).toHaveURL('/teams')
    await expect(page.getByRole('heading', { name: 'Teams' })).toBeVisible()

    // Can navigate to Leagues
    await page.click('text=Leagues')
    await expect(page).toHaveURL('/leagues')
    await expect(page.getByRole('heading', { name: 'My Leagues' })).toBeVisible()

    // Can navigate to Profile
    await page.click('text=Profile')
    await expect(page).toHaveURL('/profile')
    await expect(page.getByRole('heading', { name: 'User Profile' })).toBeVisible()
  })

  test('User Journey: Create, view, and delete a league', async ({ page }) => {
    // Go to leagues page
    await page.goto('/leagues')
    await expect(page.getByRole('heading', { name: 'My Leagues' })).toBeVisible()

    // Create a new league
    await page.getByTestId('create-league-button').click()
    await expect(page.getByTestId('create-league-modal')).toBeVisible()

    const leagueName = `E2E Test League ${Date.now()}`
    await page.getByTestId('league-name-input').fill(leagueName)
    await page.getByTestId('conferences-input').fill('1')
    await page.getByTestId('divisions-input').fill('1')
    await page.getByTestId('teams-input').fill('2')
    await page.getByTestId('submit-create-league').click()

    // Modal should close and league should appear
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })
    await expect(page.getByText(leagueName).first()).toBeVisible({ timeout: 10000 })

    // Click on the league to view details
    await page.getByText(leagueName).first().click()
    await expect(page.getByTestId('league-name')).toHaveText(leagueName)

    // Verify league structure is visible
    await expect(page.getByRole('heading', { name: 'League Structure' })).toBeVisible()

    // Delete the league
    await page.getByTestId('delete-league-button').click()
    await expect(page.getByTestId('delete-league-modal')).toBeVisible()
    await page.getByTestId('confirm-delete-league').click()

    // Should redirect to leagues list and league should be gone
    await expect(page).toHaveURL('/leagues', { timeout: 10000 })
  })

  test('User Journey: View profile with user data from API', async ({ page }) => {
    await page.goto('/profile')

    // Profile loads real user data from API
    await expect(page.getByRole('heading', { name: 'User Profile' })).toBeVisible()
    await expect(page.getByText('Account Information')).toBeVisible()
    await expect(page.getByText('Your User ID')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Copy' })).toBeVisible()

    // Leagues section shows data from API
    await expect(page.getByRole('heading', { name: /My Leagues/ })).toBeVisible()
  })

  test('User Journey: Teams page loads data from API', async ({ page }) => {
    await page.goto('/teams')

    // Page loads and shows teams from the real API
    await expect(page.getByRole('heading', { name: 'Teams' })).toBeVisible()

    // Wait for teams to load (not loading spinner)
    await page.waitForSelector('.animate-spin', { state: 'hidden', timeout: 10000 })

    // Should show team count from API
    await expect(page.getByText(/teams in the league/)).toBeVisible()
  })
})
