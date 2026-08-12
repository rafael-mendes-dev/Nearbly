import { afterEach, describe, expect, it, vi } from 'vitest'
import { api } from './client'
import { ApiError } from './problem'

afterEach(() => vi.restoreAllMocks())

describe('api client', () => {
  it('converts Problem Details into ApiError', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: 'Not found', status: 404, detail: 'Store not found.' }), { status: 404, headers: { 'content-type': 'application/problem+json' } })))
    await expect(api.publicStore('missing')).rejects.toMatchObject({ problem: { status: 404, detail: 'Store not found.' } })
  })

  it('sends bearer token and JSON body for a store update', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ id: '1' }), { status: 200, headers: { 'content-type': 'application/json' } }))
    vi.stubGlobal('fetch', fetchMock)
    await api.updateStore('store-1', { name: 'Novo nome', slug: 'novo-nome' }, 'jwt')
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining('/api/admin/stores/store-1'), expect.objectContaining({ method: 'PUT', body: JSON.stringify({ name: 'Novo nome', slug: 'novo-nome' }), headers: expect.objectContaining({ Authorization: 'Bearer jwt', 'Content-Type': 'application/json' }) }))
  })

  it('keeps the error type explicit', () => {
    expect(new ApiError({ title: 'x', status: 400, detail: 'y' })).toBeInstanceOf(Error)
  })
})
