import { test, expect } from '@playwright/test'

test.describe('Game Simulation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/simulate')
  })

  test('should display page title and dropdowns', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Simulate Game' })).toBeVisible()
    await expect(page.getByText('Home Team')).toBeVisible()
    await expect(page.getByText('Away Team')).toBeVisible()
  })

  test('should have disabled simulate button initially', async ({ page }) => {
    const simulateButton = page.getByRole('button', { name: /simulate game/i })
    await expect(simulateButton).toBeDisabled()
  })

  test('should enable simulate button when teams selected', async ({ page }) => {
    // Note: This test requires the API to be running with actual teams
    // In a real scenario, you might mock the API or use test data

    // Select home team
    await page.selectOption('select >> nth=0', { index: 1 })

    // Select away team
    await page.selectOption('select >> nth=1', { index: 2 })

    // Button should now be enabled
    const simulateButton = page.getByRole('button', { name: /simulate game/i })
    await expect(simulateButton).toBeEnabled()
  })

  test('should display instructions card', async ({ page }) => {
    await expect(page.getByText('How it works')).toBeVisible()
    await expect(page.getByText('Select a home team and an away team')).toBeVisible()
  })
})
