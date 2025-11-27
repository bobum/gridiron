import { test, expect } from './fixtures/auth'

test.describe('HomePage', () => {
  test('should display main heading', async ({ page }) => {
    await page.goto('/')
    await expect(page.getByRole('heading', { name: 'Gridiron Football Manager' })).toBeVisible()
  })

  test('should show API status', async ({ page }) => {
    await page.goto('/')

    // API Status card should be visible
    await expect(page.getByText('API Status')).toBeVisible()
  })

  test('should display quick action cards', async ({ page }) => {
    await page.goto('/')

    // Check for quick action cards using exact match for the card content
    await expect(page.getByRole('link', { name: '🏈 View Teams Browse all teams' })).toBeVisible()
    await expect(page.getByRole('link', { name: '⚡ Simulate Game Run a game' })).toBeVisible()
  })

  test('quick action cards should navigate', async ({ page }) => {
    await page.goto('/')

    // Click on "View Teams" card
    await page.getByText('View Teams').click()
    await expect(page).toHaveURL('/teams')
  })
})
