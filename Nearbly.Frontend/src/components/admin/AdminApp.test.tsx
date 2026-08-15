import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import AdminApp from './AdminApp'
import { api } from '../../lib/api/client'
import { saveSession } from '../../lib/auth/session'
import type { LoginResponse, MediaResponse, StoreResponse } from '../../lib/api/types'

const session: LoginResponse = { accessToken: 'token', tokenType: 'Bearer', expiresAtUtc: '2099-01-01T00:00:00Z' }
const store: StoreResponse = {
  id: 'store-1', name: 'Café Central', slug: 'cafe-central', publicCode: 's_store1', description: '', logoUrl: null, logoMediaId: null,
  primaryColor: '#2B22E0', secondaryColor: '#06080F', isActive: true, createdAtUtc: '', updatedAtUtc: '',
}
const media: MediaResponse = { id: 'media-1', url: '/media/media-1', mimeType: 'image/webp', sizeBytes: 1, width: 1, height: 1, isActive: true, createdAtUtc: '' }

describe('AdminApp store creation', () => {
  afterEach(() => {
    cleanup()
    sessionStorage.clear()
    window.history.replaceState({}, '', '/')
    vi.restoreAllMocks()
  })

  it('uploads and associates a logo selected while creating a store', async () => {
    window.history.replaceState({}, '', '/admin/login')
    vi.spyOn(api, 'login').mockResolvedValue(session)
    let stores: StoreResponse[] = []
    vi.spyOn(api, 'stores').mockImplementation(async () => stores)
    const createStore = vi.spyOn(api, 'createStore').mockImplementation(async () => { stores = [store]; return store })
    const uploadMedia = vi.spyOn(api, 'uploadMedia').mockResolvedValue(media)
    const updateStore = vi.spyOn(api, 'updateStore').mockResolvedValue({ ...store, logoMediaId: media.id })
    const user = userEvent.setup()

    render(<AdminApp />)

    await user.type(await screen.findByLabelText('Email'), 'admin@test.local')
    await user.type(screen.getByLabelText('Senha'), 'NearblyTest123')
    await user.click(screen.getByRole('button', { name: /Entrar/ }))
    await user.click(await screen.findByRole('button', { name: 'Nova loja' }))
    await user.type(screen.getByLabelText('Nome'), 'Café Central')
    await user.type(screen.getByLabelText('Slug público'), 'cafe-central')
    const logo = new File(['logo'], 'logo.png', { type: 'image/png' })
    await user.upload(screen.getByLabelText('Selecionar logo da loja'), logo)
    await user.click(screen.getByRole('button', { name: 'Salvar loja' }))

    await waitFor(() => expect(createStore).toHaveBeenCalledWith(expect.objectContaining({ logoUrl: null }), session.accessToken))
    expect(uploadMedia).toHaveBeenCalledWith(store.id, logo, session.accessToken)
    expect(updateStore).toHaveBeenCalledWith(store.id, expect.objectContaining({ logoMediaId: media.id, logoUrl: null }), session.accessToken)
    expect(await screen.findByRole('heading', { name: 'Café Central' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Editar Café Central' }))
    const primaryColor = screen.getByLabelText('Cor principal em hexadecimal')
    await user.clear(primaryColor)
    await user.type(primaryColor, '#00D65D')
    await user.click(screen.getByRole('button', { name: 'Salvar loja' }))

    await waitFor(() => expect(updateStore).toHaveBeenLastCalledWith(store.id, expect.objectContaining({ logoUrl: null, primaryColor: '#00D65D', isActive: true }), session.accessToken))
    expect(updateStore.mock.calls.at(-1)?.[1]).not.toHaveProperty('logoMediaId')
  })

  it('updates colors in settings without replacing an uploaded logo', async () => {
    window.history.replaceState({}, '', `/admin/lojas/${store.id}/configuracoes`)
    saveSession(session)
    const storeWithMedia = { ...store, logoUrl: media.url, logoMediaId: media.id }
    vi.spyOn(api, 'store').mockResolvedValue(storeWithMedia)
    const updateStore = vi.spyOn(api, 'updateStore').mockResolvedValue(storeWithMedia)
    const user = userEvent.setup()

    render(<AdminApp />)

    const primaryColor = await screen.findByLabelText('Cor principal em hexadecimal')
    await user.clear(primaryColor)
    await user.type(primaryColor, '#00D65D')
    await user.click(screen.getByRole('button', { name: 'Salvar loja' }))

    await waitFor(() => expect(updateStore).toHaveBeenCalledWith(store.id, expect.objectContaining({ logoUrl: null, primaryColor: '#00D65D', isActive: true }), session.accessToken))
    expect(updateStore.mock.calls[0]?.[1]).not.toHaveProperty('logoMediaId')
  })
})
