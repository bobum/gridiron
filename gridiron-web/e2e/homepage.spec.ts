import { test, expect } from '@playwright/test'

test.describe('HomePage', () => {
  test('should display main heading', async ({ page }) => {
    await page.goto('/')

    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // DEBUG: Log what text is actually visible
    const bodyText = await page.locator('body').textContent()
    console.log('=== VISIBLE BODY TEXT ===')
    console.log(bodyText)
    console.log('=== END BODY TEXT ===')

    await expect(page.getByRole('heading', { name: 'Gridiron Football Manager' })).toBeVisible()
  })

  test('should show API status', async ({ page }) => {
    await page.goto('/')

    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (API Status Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // API Status card should be visible
    await expect(page.getByText('API Status')).toBeVisible()
  })

  test('should display quick action cards', async ({ page }) => {
    await page.goto('/')

    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (Quick Action Cards Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // Check for quick action cards using exact match for the card content
    await expect(page.getByRole('link', { name: '🏈 View Teams Browse all teams' })).toBeVisible()
    await expect(page.getByRole('link', { name: '⚡ Simulate Game Run a game' })).toBeVisible()
  })

  test('quick action cards should navigate', async ({ page }) => {
    await page.goto('/')

    // DEBUG: Log the entire HTML content
    const html = await page.content()
    console.log('=== FULL HTML CONTENT (Navigation Test) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // Click on "View Teams" card
    await page.getByText('View Teams').click()
    await expect(page).toHaveURL('/teams')
  })
})
