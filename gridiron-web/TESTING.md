# Testing Guide - Gridiron Frontend

This document describes the testing strategy and how to run tests for the Gridiron React frontend.

## Testing Stack

We use three complementary testing approaches:

1. **Component Tests** - Vitest + React Testing Library
2. **API Integration Tests** - Vitest + MSW (Mock Service Worker)
3. **End-to-End Tests** - Playwright

---

## Quick Start

```bash
# Run all component/integration tests
npm test

# Run E2E tests
npm run test:e2e

# Run all tests with UI
npm run test:ui
npm run test:e2e:ui
```

---

## 1. Component Tests (Vitest + React Testing Library)

### Purpose
Test individual React components in isolation without a browser.

### What to Test
- ✅ Component renders without crashing
- ✅ Props are displayed correctly
- ✅ User interactions (clicks, inputs)
- ✅ Conditional rendering (loading, error states)
- ❌ Don't test implementation details
- ❌ Don't test third-party libraries

### Running Tests

```bash
# Watch mode (re-runs on file changes)
npm test

# Run once
npm test -- --run

# With UI (visual test runner)
npm run test:ui

# With coverage
npm run test:coverage
```

### Example Test

```typescript
// src/components/__tests__/Navigation.test.tsx
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { Navigation } from '../Navigation'

describe('Navigation', () => {
  it('renders all navigation links', () => {
    render(
      <BrowserRouter>
        <Navigation />
      </BrowserRouter>
    )

    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Teams')).toBeInTheDocument()
  })
})
```

### File Structure
```
src/
├── components/
│   ├── Navigation.tsx
│   └── __tests__/
│       └── Navigation.test.tsx
├── pages/
│   ├── HomePage.tsx
│   └── __tests__/
│       └── HomePage.test.tsx
```

---

## 2. API Integration Tests (MSW)

### Purpose
Test components that interact with the API using mocked responses.

### What MSW Does
- Intercepts HTTP requests at the network level
- Returns mock responses without hitting the real API
- Works in both tests and the browser (for development)

### Running Tests

Same as component tests (they run together):

```bash
npm test
```

### Mock Data Location

```
src/test/mocks/
├── handlers.ts    - API route handlers
└── server.ts      - MSW server setup
```

### Example Mock Handler

```typescript
// src/test/mocks/handlers.ts
import { http, HttpResponse } from 'msw'

export const handlers = [
  http.get('/api/teams', () => {
    return HttpResponse.json([
      { id: 1, name: 'Falcons', city: 'Atlanta' }
    ])
  }),
]
```

### Example Integration Test

```typescript
// src/pages/__tests__/TeamsPage.test.tsx
import { describe, it, expect } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClientProvider } from '@tanstack/react-query'
import { TeamsPage } from '../TeamsPage'

describe('TeamsPage', () => {
  it('displays teams after loading', async () => {
    render(
      <QueryClientProvider client={queryClient}>
        <TeamsPage />
      </QueryClientProvider>
    )

    // MSW intercepts the API call and returns mock data
    await waitFor(() => {
      expect(screen.getByText('Atlanta Falcons')).toBeInTheDocument()
    })
  })
})
```

### Adding New Mock Endpoints

Edit `src/test/mocks/handlers.ts`:

```typescript
export const handlers = [
  // Add new handler
  http.post('/api/games/simulate', async ({ request }) => {
    const body = await request.json()
    return HttpResponse.json({
      id: 1,
      homeScore: 24,
      awayScore: 17,
    })
  }),
]
```

---

## 3. End-to-End Tests (Playwright)

### Purpose
Test complete user journeys in a real browser.

### What to Test
- ✅ Critical user flows (game simulation, navigation)
- ✅ Multi-page interactions
- ✅ Real API integration (with test database)
- ❌ Don't duplicate component tests
- ❌ Don't test every edge case (too slow)

### Prerequisites

**IMPORTANT:** E2E tests require the backend API and a seeded database.

Before running E2E tests, ensure:
1. ✅ API is running on `http://localhost:5000`
2. ✅ Database is seeded with test data
3. ✅ Frontend dev server will auto-start (configured in `playwright.config.ts`)

**To verify the API is ready:**
```bash
# Visit Swagger UI
open http://localhost:5000/swagger/index.html
```

**If E2E tests fail due to missing/corrupted data, reset the database:**

```bash
# Navigate to the API project directory
cd ../Gridiron.WebApi

# Drop the database (WARNING: deletes all data)
dotnet ef database drop --force

# Recreate database with latest migrations
dotnet ef database update

# Seed with test data
dotnet run -- --seed

# Start the API
dotnet run
```

### Running E2E Tests

```bash
# Run all E2E tests (headless)
npm run test:e2e

# Run with UI (watch mode)
npm run test:e2e:ui

# View last test report
npm run test:e2e:report

# Run specific test file
npx playwright test e2e/navigation.spec.ts

# Run in headed mode (see the browser)
npx playwright test --headed

# Debug mode (pause execution)
npx playwright test --debug
```

### Example E2E Test

```typescript
// e2e/navigation.spec.ts
import { test, expect } from '@playwright/test'

test('should navigate between pages', async ({ page }) => {
  await page.goto('/')

  await page.click('text=Teams')
  await expect(page).toHaveURL('/teams')

  await page.click('text=Home')
  await expect(page).toHaveURL('/')
})
```

### File Structure
```
e2e/
├── navigation.spec.ts
├── homepage.spec.ts
└── game-simulation.spec.ts
```

### Configuration

Edit `playwright.config.ts` to:
- Change browser (Chrome, Firefox, Safari)
- Adjust timeout settings
- Configure CI/CD behavior

---

## Test Coverage

### Viewing Coverage

```bash
npm run test:coverage
```

Coverage reports are generated in `coverage/` directory.

### Coverage Goals

- **Components**: 80%+ coverage
- **Pages**: 70%+ coverage
- **API hooks**: 60%+ coverage
- **Critical paths**: 90%+ coverage (game simulation, navigation)

---

## Writing Good Tests

### Do's ✅

- **Test user behavior**, not implementation
  ```typescript
  // Good: Test what the user sees
  expect(screen.getByText('Submit')).toBeInTheDocument()

  // Bad: Test internal state
  expect(component.state.isSubmitting).toBe(false)
  ```

- **Use accessible queries**
  ```typescript
  // Good: Queries that reflect how users find elements
  screen.getByRole('button', { name: /submit/i })
  screen.getByLabelText('Email')

  // Bad: Implementation details
  container.querySelector('.submit-btn')
  ```

- **Test different states**
  ```typescript
  it('shows loading state')
  it('shows error state')
  it('shows success state')
  ```

### Don'ts ❌

- Don't test third-party libraries (React Query, React Router)
- Don't test CSS styling (use visual regression for that)
- Don't duplicate tests (if E2E covers it, skip component test)
- Don't test implementation details (internal state, private methods)

---

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Every pull request
- Every push to `master`

See `.github/workflows/frontend-tests.yml` (to be added)

### Pre-commit Hook

Consider adding a pre-commit hook to run tests:

```json
// package.json
{
  "husky": {
    "hooks": {
      "pre-commit": "npm test -- --run"
    }
  }
}
```

---

## Debugging Tests

### Component/Integration Tests

```bash
# Run a specific test file
npm test -- Navigation.test.tsx

# Run tests matching a pattern
npm test -- --grep "displays teams"

# Run in UI mode (visual debugger)
npm run test:ui
```

### E2E Tests

```bash
# Debug mode (pauses at each step)
npx playwright test --debug

# Run with browser visible
npx playwright test --headed

# Trace viewer (after test runs)
npx playwright show-trace trace.zip
```

### Common Issues

**Issue**: "Element not found"
- Solution: Use `waitFor` for async operations
  ```typescript
  await waitFor(() => {
    expect(screen.getByText('Loaded')).toBeInTheDocument()
  })
  ```

**Issue**: "Network request failed"
- Solution: Ensure MSW handlers are defined
  ```typescript
  // Check src/test/mocks/handlers.ts
  ```

**Issue**: E2E test times out
- Solution: Ensure dev server is running (`npm run dev`)
- Or configure Playwright to start it automatically (already configured)

---

## Example Test Files

### Component Test
`src/components/__tests__/Navigation.test.tsx` ✅

### Integration Test
`src/pages/__tests__/TeamsPage.test.tsx` ✅

### E2E Test
`e2e/navigation.spec.ts` ✅

---

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [MSW Documentation](https://mswjs.io/)
- [Playwright Documentation](https://playwright.dev/)
- [Testing Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)

---

## Next Steps

1. Run the example tests to verify setup
2. Write tests for new components as you build them
3. Aim for 70%+ coverage on critical paths
4. Add E2E tests for major user flows
5. Set up CI/CD to run tests automatically
