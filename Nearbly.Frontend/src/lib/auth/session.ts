import type { LoginResponse } from '../api/types'

const key = 'nearbly.admin.session'
export type Session = LoginResponse

export const readSession = (): Session | null => {
  if (typeof window === 'undefined') return null
  try {
    const value = JSON.parse(sessionStorage.getItem(key) ?? 'null') as Session | null
    if (!value || new Date(value.expiresAtUtc).getTime() <= Date.now()) {
      sessionStorage.removeItem(key)
      return null
    }
    return value
  } catch {
    sessionStorage.removeItem(key)
    return null
  }
}

export const saveSession = (session: Session) => sessionStorage.setItem(key, JSON.stringify(session))
export const clearSession = () => sessionStorage.removeItem(key)
