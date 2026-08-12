import { describe, expect, it } from 'vitest'
import { contrastText, fillDateGaps, isSafeColor } from './format'

describe('format helpers', () => {
  it('accepts only six-digit hex colors and chooses readable foreground', () => {
    expect(isSafeColor('#C8EF9F')).toBe(true)
    expect(isSafeColor('#fff')).toBe(false)
    expect(contrastText('#ffffff')).toBe('#10211c')
    expect(contrastText('#10211c')).toBe('#ffffff')
  })

  it('fills missing analytics days with zero', () => {
    expect(fillDateGaps([{ date: '2026-08-02', views: 4 }], '2026-08-01', '2026-08-03')).toEqual([
      { date: '2026-08-01', views: 0 },
      { date: '2026-08-02', views: 4 },
      { date: '2026-08-03', views: 0 },
    ])
  })
})
