import { test, expect } from '@playwright/test'

test.describe('Navigation', () => {
  test('should navigate between pages', async ({ page }) => {
    // Start at home page
    await page.goto('/')

    // DEBUG: Log the entire HTML content at homepage
    let html = await page.content()
    console.log('=== FULL HTML CONTENT (Home Page) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    // Should see the title
    await expect(page.getByRole('heading', { name: 'Gridiron Football Manager' })).toBeVisible()

    // Navigate to Teams page
    await page.click('text=Teams')
    await expect(page).toHaveURL('/teams')

    // DEBUG: Log the entire HTML content at teams page
    html = await page.content()
    console.log('=== FULL HTML CONTENT (Teams Page) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.getByRole('heading', { name: 'Teams' })).toBeVisible()

    // Navigate to Simulate page
    await page.click('text=Simulate Game')
    await expect(page).toHaveURL('/simulate')

    // DEBUG: Log the entire HTML content at simulate page
    html = await page.content()
    console.log('=== FULL HTML CONTENT (Simulate Page) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.getByRole('heading', { name: 'Simulate Game' })).toBeVisible()

    // Navigate back to Home
    await page.click('text=Home')
    await expect(page).toHaveURL('/')
  })

  test('should display navigation bar on all pages', async ({ page }) => {
    await page.goto('/')

    // DEBUG: Log the entire HTML content
    let html = await page.content()
    console.log('=== FULL HTML CONTENT (Nav Bar Test - Home) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.locator('nav')).toContainText('Gridiron')

    await page.goto('/teams')

    // DEBUG: Log the entire HTML content
    html = await page.content()
    console.log('=== FULL HTML CONTENT (Nav Bar Test - Teams) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.locator('nav')).toContainText('Gridiron')

    await page.goto('/simulate')

    // DEBUG: Log the entire HTML content
    html = await page.content()
    console.log('=== FULL HTML CONTENT (Nav Bar Test - Simulate) ===')
    console.log(html)
    console.log('=== END HTML CONTENT ===')

    await expect(page.locator('nav')).toContainText('Gridiron')
  })
})
