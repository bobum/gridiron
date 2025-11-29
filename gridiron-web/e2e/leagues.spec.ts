import { test, expect } from '@playwright/test'

test.describe('Leagues Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/leagues')
  })

  test('should display page header', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'My Leagues' })).toBeVisible()
    await expect(page.getByText('Manage your leagues or create a new one')).toBeVisible()
  })

  test('should display create league button', async ({ page }) => {
    await expect(page.getByTestId('create-league-button')).toBeVisible()
  })

  test('should have leagues link in navigation bar', async ({ page }) => {
    await page.goto('/')
    await expect(page.getByRole('link', { name: 'Leagues' })).toBeVisible()
  })

  test('should navigate to leagues from nav bar', async ({ page }) => {
    await page.goto('/')
    await page.click('text=Leagues')
    await expect(page).toHaveURL('/leagues')
    await expect(page.getByRole('heading', { name: 'My Leagues' })).toBeVisible()
  })
})

test.describe('League Creation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/leagues')
  })

  test('should open create league modal', async ({ page }) => {
    await page.getByTestId('create-league-button').click()
    await expect(page.getByTestId('create-league-modal')).toBeVisible()
    await expect(page.getByRole('heading', { name: 'Create New League' })).toBeVisible()
  })

  test('should close modal when clicking X', async ({ page }) => {
    await page.getByTestId('create-league-button').click()
    await expect(page.getByTestId('create-league-modal')).toBeVisible()
    await page.getByTestId('close-modal-button').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible()
  })

  test('should close modal when clicking Cancel', async ({ page }) => {
    await page.getByTestId('create-league-button').click()
    await expect(page.getByTestId('create-league-modal')).toBeVisible()
    await page.getByRole('button', { name: 'Cancel' }).click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible()
  })

  test('should display league structure form', async ({ page }) => {
    await page.getByTestId('create-league-button').click()
    await expect(page.getByTestId('league-name-input')).toBeVisible()
    await expect(page.getByTestId('conferences-input')).toBeVisible()
    await expect(page.getByTestId('divisions-input')).toBeVisible()
    await expect(page.getByTestId('teams-input')).toBeVisible()
    await expect(page.getByTestId('total-teams')).toBeVisible()
  })

  test('should calculate total teams correctly', async ({ page }) => {
    await page.getByTestId('create-league-button').click()

    // Default values: 2 conferences × 2 divisions × 4 teams = 16 teams
    await expect(page.getByTestId('total-teams')).toHaveText('16')

    // Change to 2 × 4 × 4 = 32 teams
    await page.getByTestId('divisions-input').fill('4')
    await expect(page.getByTestId('total-teams')).toHaveText('32')

    // Change to 1 × 1 × 1 = 1 team
    await page.getByTestId('conferences-input').fill('1')
    await page.getByTestId('divisions-input').fill('1')
    await page.getByTestId('teams-input').fill('1')
    await expect(page.getByTestId('total-teams')).toHaveText('1')
  })

  test('should show error if name is empty', async ({ page }) => {
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('form-error')).toContainText('League name is required')
  })

  test('should create a league successfully', async ({ page }) => {
    await page.getByTestId('create-league-button').click()

    // Fill in the form
    await page.getByTestId('league-name-input').fill('Test League E2E')
    await page.getByTestId('conferences-input').fill('2')
    await page.getByTestId('divisions-input').fill('2')
    await page.getByTestId('teams-input').fill('2')

    // Submit
    await page.getByTestId('submit-create-league').click()

    // Modal should close and league should appear in list
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })

    // Wait for at least one league card with this name to appear
    await expect(page.getByText('Test League E2E').first()).toBeVisible({ timeout: 10000 })
  })
})

test.describe('League Detail Page', () => {
  test.beforeEach(async ({ page }) => {
    // First create a league to view
    await page.goto('/leagues')
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('league-name-input').fill('Detail Test League')
    await page.getByTestId('conferences-input').fill('2')
    await page.getByTestId('divisions-input').fill('2')
    await page.getByTestId('teams-input').fill('2')
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })

    // Navigate to the league detail page
    await page.getByText('Detail Test League').first().click()
    await expect(page.getByTestId('league-name')).toBeVisible({ timeout: 10000 })
  })

  test('should display league header information', async ({ page }) => {
    await expect(page.getByTestId('league-name')).toHaveText('Detail Test League')
    await expect(page.getByText('Commissioner', { exact: false })).toBeVisible()
  })

  test('should display league stats', async ({ page }) => {
    const main = page.getByRole('main')
    await expect(main.getByText('Conferences', { exact: true })).toBeVisible()
    await expect(main.getByText('Divisions', { exact: true })).toBeVisible()
    await expect(main.getByText('Teams', { exact: true })).toBeVisible()
    await expect(main.getByText('Players', { exact: true })).toBeVisible()
  })

  test('should display league structure with expandable conferences', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'League Structure' })).toBeVisible()
    // Click on first conference to expand
    await page.locator('button').filter({ hasText: 'Conference' }).first().click()
    // Should see divisions after expanding
    await expect(page.getByText('Division', { exact: false }).first()).toBeVisible()
  })

  test('should display commissioner controls', async ({ page }) => {
    // As the creator, user should see commissioner controls
    await expect(page.getByTestId('populate-rosters-button')).toBeVisible()
    await expect(page.getByTestId('edit-league-button')).toBeVisible()
    await expect(page.getByTestId('delete-league-button')).toBeVisible()
  })

  test('should show back link to leagues list', async ({ page }) => {
    await expect(page.getByRole('link', { name: /Back to Leagues/ })).toBeVisible()
    await page.getByRole('link', { name: /Back to Leagues/ }).click()
    await expect(page).toHaveURL('/leagues')
  })
})

test.describe('League Edit Functionality', () => {
  test.beforeEach(async ({ page }) => {
    // Create a league and navigate to it
    await page.goto('/leagues')
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('league-name-input').fill('Edit Test League')
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })
    await page.getByText('Edit Test League').first().click()
    await expect(page.getByTestId('league-name')).toBeVisible({ timeout: 10000 })
  })

  test('should open edit modal', async ({ page }) => {
    await page.getByTestId('edit-league-button').click()
    await expect(page.getByTestId('edit-league-modal')).toBeVisible()
    await expect(page.getByTestId('edit-name-input')).toHaveValue('Edit Test League')
  })

  test('should update league name', async ({ page }) => {
    await page.getByTestId('edit-league-button').click()
    await page.getByTestId('edit-name-input').fill('Updated League Name')
    await page.getByTestId('submit-edit-league').click()
    await expect(page.getByTestId('edit-league-modal')).not.toBeVisible({ timeout: 10000 })
    await expect(page.getByTestId('league-name')).toHaveText('Updated League Name')
  })

  test('should toggle league active status', async ({ page }) => {
    await page.getByTestId('edit-league-button').click()
    // Toggle the checkbox
    await page.getByTestId('edit-active-checkbox').click()
    await page.getByTestId('submit-edit-league').click()
    await expect(page.getByTestId('edit-league-modal')).not.toBeVisible({ timeout: 10000 })
  })
})

test.describe('League Delete Functionality', () => {
  test.beforeEach(async ({ page }) => {
    // Create a league and navigate to it
    await page.goto('/leagues')
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('league-name-input').fill('Delete Test League')
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })
    await page.getByText('Delete Test League').first().click()
    await expect(page.getByTestId('league-name')).toBeVisible({ timeout: 10000 })
  })

  test('should open delete confirmation modal', async ({ page }) => {
    await page.getByTestId('delete-league-button').click()
    await expect(page.getByTestId('delete-league-modal')).toBeVisible()
    await expect(page.getByText('Are you sure you want to delete')).toBeVisible()
  })

  test('should close delete modal when clicking Cancel', async ({ page }) => {
    await page.getByTestId('delete-league-button').click()
    await expect(page.getByTestId('delete-league-modal')).toBeVisible()
    await page.getByRole('button', { name: 'Cancel' }).click()
    await expect(page.getByTestId('delete-league-modal')).not.toBeVisible()
  })

  test('should delete league and redirect to leagues list', async ({ page }) => {
    // Get current URL to extract league ID
    const url = page.url()
    const leagueId = url.split('/').pop()

    await page.getByTestId('delete-league-button').click()
    await page.getByTestId('confirm-delete-league').click()
    // Should redirect to leagues list
    await expect(page).toHaveURL('/leagues', { timeout: 10000 })
    // The specific league card should no longer exist
    await expect(page.getByTestId(`league-card-${leagueId}`)).not.toBeVisible()
  })
})

test.describe('Populate Rosters Functionality', () => {
  test.beforeEach(async ({ page }) => {
    // Create a league and navigate to it
    await page.goto('/leagues')
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('league-name-input').fill('Populate Test League')
    await page.getByTestId('conferences-input').fill('1')
    await page.getByTestId('divisions-input').fill('1')
    await page.getByTestId('teams-input').fill('1')
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })
    await page.getByText('Populate Test League').first().click()
    await expect(page.getByTestId('league-name')).toBeVisible({ timeout: 10000 })
  })

  test('should open populate rosters modal', async ({ page }) => {
    await page.getByTestId('populate-rosters-button').click()
    await expect(page.getByTestId('populate-rosters-modal')).toBeVisible()
    await expect(page.getByText('This will generate 53 random players')).toBeVisible()
  })

  test('should have optional seed input', async ({ page }) => {
    await page.getByTestId('populate-rosters-button').click()
    await expect(page.getByTestId('populate-seed-input')).toBeVisible()
    await expect(page.getByText('Use a seed for reproducible player generation')).toBeVisible()
  })

  test('should populate rosters with seed', async ({ page }) => {
    await page.getByTestId('populate-rosters-button').click()
    await page.getByTestId('populate-seed-input').fill('12345')
    await page.getByTestId('confirm-populate-rosters').click()
    // Modal should close after success
    await expect(page.getByTestId('populate-rosters-modal')).not.toBeVisible({ timeout: 30000 })
  })
})

test.describe('League User Management', () => {
  test.beforeEach(async ({ page }) => {
    // Create a league and navigate to it
    await page.goto('/leagues')
    await page.getByTestId('create-league-button').click()
    await page.getByTestId('league-name-input').fill('User Management Test League')
    await page.getByTestId('submit-create-league').click()
    await expect(page.getByTestId('create-league-modal')).not.toBeVisible({ timeout: 10000 })
    await page.getByText('User Management Test League').first().click()
    await expect(page.getByTestId('league-name')).toBeVisible({ timeout: 10000 })
  })

  test('should display League Members section for commissioners', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'League Members' })).toBeVisible()
    await expect(page.getByTestId('add-user-button')).toBeVisible()
  })

  test('should open add user modal', async ({ page }) => {
    await page.getByTestId('add-user-button').click()
    await expect(page.getByTestId('add-user-modal')).toBeVisible()
    await expect(page.getByRole('heading', { name: 'Add User to League' })).toBeVisible()
  })

  test('should show user id and role inputs', async ({ page }) => {
    await page.getByTestId('add-user-button').click()
    await expect(page.getByTestId('add-user-id-input')).toBeVisible()
    await expect(page.getByTestId('add-user-role-select')).toBeVisible()
  })

  test('should show team select when role is GeneralManager', async ({ page }) => {
    await page.getByTestId('add-user-button').click()
    // Default role is GeneralManager, so team select should be visible
    await expect(page.getByTestId('add-user-team-select')).toBeVisible()

    // Change to Commissioner
    await page.getByTestId('add-user-role-select').selectOption('Commissioner')
    // Team select should not be visible
    await expect(page.getByTestId('add-user-team-select')).not.toBeVisible()
  })

  test('should close add user modal when clicking X', async ({ page }) => {
    await page.getByTestId('add-user-button').click()
    await expect(page.getByTestId('add-user-modal')).toBeVisible()
    await page.locator('[data-testid="add-user-modal"] button').filter({ hasText: '×' }).click()
    await expect(page.getByTestId('add-user-modal')).not.toBeVisible()
  })
})
