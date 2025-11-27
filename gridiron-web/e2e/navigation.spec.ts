import { test, expect } from '@playwright/test'

test.describe('Navigation', () => {
  test('should navigate between pages', async ({ page }) => {
    // Start at home page
    await page.goto('/')

    // Should see the title
    await expect(page.getByRole('heading', { name: 'Gridiron Football Manager' })).toBeVisible()

    // Navigate to Teams page
    await page.click('text=Teams')
    await expect(page).toHaveURL('/teams')
    await expect(page.getByRole('heading', { name: 'Teams' })).toBeVisible()

    // Navigate to Simulate page
    await page.click('text=Simulate Game')
    await expect(page).toHaveURL('/simulate')
    await expect(page.getByRole('heading', { name: 'Simulate Game' })).toBeVisible()

    // Navigate back to Home
    await page.click('text=Home')
    await expect(page).toHaveURL('/')
  })

  test('should display navigation bar on all pages', async ({ page }) => {
    await page.goto('/')
    await expect(page.locator('nav')).toContainText('Gridiron')

    await page.goto('/teams')
    await expect(page.locator('nav')).toContainText('Gridiron')

    await page.goto('/simulate')
    await expect(page.locator('nav')).toContainText('Gridiron')
  })
})
