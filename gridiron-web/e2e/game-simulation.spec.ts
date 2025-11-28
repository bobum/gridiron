import { test, expect } from '@playwright/test'

test.describe('Game Simulation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/simulate')
  })

  test('should display page title and dropdowns', async ({ page }) => {
    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (Game Sim - Title Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.getByRole('heading', { name: 'Simulate Game' })).toBeVisible()
    await expect(page.locator('label').filter({ hasText: 'Home Team' }).first()).toBeVisible()
    await expect(page.locator('label').filter({ hasText: 'Away Team' }).first()).toBeVisible()
  })

  test('should have disabled simulate button initially', async ({ page }) => {
    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (Game Sim - Button Disabled Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    const simulateButton = page.getByRole('button', { name: /simulate game/i })
    await expect(simulateButton).toBeDisabled()
  })

  test('should enable simulate button when teams selected', async ({ page }) => {
    // Note: This test requires the API to be running with actual teams
    // In a real scenario, you might mock the API or use test data

    // DEBUG: Log the entire HTML content before selection
    let html = await page.content()
    console.log('=== FULL HTML CONTENT (Game Sim - Before Team Selection) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // Select home team
    await page.selectOption('select >> nth=0', { index: 1 })

    // Select away team
    await page.selectOption('select >> nth=1', { index: 2 })

    // DEBUG: Log the entire HTML content after selection
    html = await page.content()
    console.log('=== FULL HTML CONTENT (Game Sim - After Team Selection) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // Button should now be enabled
    const simulateButton = page.getByRole('button', { name: /simulate game/i })
    await expect(simulateButton).toBeEnabled()
  })

  test('should display instructions card', async ({ page }) => {
    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (Game Sim - Instructions Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.getByText('How it works')).toBeVisible()
    await expect(page.getByText('Select a home team and an away team')).toBeVisible()
  })
})
