import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach, beforeAll, afterAll } from 'vitest'
import { server } from './mocks/server'
import { resetMockState } from './mocks/handlers'

// Start MSW server before all tests
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))

// Reset handlers and mock state after each test
afterEach(() => {
  server.resetHandlers()
  resetMockState()
  cleanup()
})

// Close MSW server after all tests
afterAll(() => server.close())
