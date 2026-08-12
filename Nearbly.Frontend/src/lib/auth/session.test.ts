import { beforeEach, describe, expect, it } from 'vitest'
import { clearSession, readSession, saveSession } from './session'

describe('admin session', () => {
  beforeEach(() => { sessionStorage.clear() })

  it('persists a valid session and removes an expired one', () => {
    saveSession({ accessToken: 'jwt', tokenType: 'Bearer', expiresAtUtc: new Date(Date.now() + 60_000).toISOString() })
    expect(readSession()?.accessToken).toBe('jwt')
    saveSession({ accessToken: 'expired', tokenType: 'Bearer', expiresAtUtc: new Date(Date.now() - 60_000).toISOString() })
    expect(readSession()).toBeNull()
    clearSession()
  })
})
