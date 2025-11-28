import { test, expect } from '@playwright/test'

test.describe('Profile Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/profile')
  })

  test('should display page title', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'User Profile' })).toBeVisible()
    await expect(page.getByText('Manage your account and view your leagues')).toBeVisible()
  })

  test('should display account information section', async ({ page }) => {
    await expect(page.getByText('Account Information')).toBeVisible()
    await expect(page.getByText('Your User ID')).toBeVisible()
    await expect(page.getByText('Display Name')).toBeVisible()
    await expect(page.getByText('Email')).toBeVisible()
  })

  test('should display user ID with copy button', async ({ page }) => {
    // User ID should be displayed
    await expect(page.getByText('Your User ID')).toBeVisible()

    // Copy button should be visible
    await expect(page.getByRole('button', { name: 'Copy' })).toBeVisible()

    // Share instruction should be visible
    await expect(page.getByText('Share this ID with league commissioners to join a league')).toBeVisible()
  })

  test('should display My Leagues section with empty state', async ({ page }) => {
    // My Leagues section should be visible
    await expect(page.getByRole('heading', { name: /My Leagues/ })).toBeVisible()

    // In E2E test mode, user has no leagues - should show empty state
    await expect(page.getByText('No leagues yet')).toBeVisible()
    await expect(page.getByText('Join a league by sharing your User ID')).toBeVisible()
  })

  test('should display account activity dates', async ({ page }) => {
    await expect(page.getByText('Member Since')).toBeVisible()
    await expect(page.getByText('Last Login')).toBeVisible()
  })
})

test.describe('Profile Navigation', () => {
  test('should navigate to profile from nav bar', async ({ page }) => {
    await page.goto('/')

    // Click Profile link in navigation
    await page.click('text=Profile')

    // Should be on profile page
    await expect(page).toHaveURL('/profile')
    await expect(page.getByRole('heading', { name: 'User Profile' })).toBeVisible()
  })

  test('should have profile link in navigation bar', async ({ page }) => {
    await page.goto('/')

    // Profile link should be visible in nav
    await expect(page.getByRole('link', { name: 'Profile' })).toBeVisible()
  })
})
