import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import ContentPage from './ContentPage'
import { api } from '../../lib/api/client'
import type { AdminLinkResponse, StoreResponse, TabResponse } from '../../lib/api/types'

const store: StoreResponse = {
  id: 'store-1', name: 'Café Central', slug: 'cafe-central', publicCode: 's_store1', description: null, logoUrl: null, logoMediaId: null,
  primaryColor: null, secondaryColor: null, isActive: true, createdAtUtc: '', updatedAtUtc: '',
}

const tab: TabResponse = {
  id: 'tab-1', storeId: store.id, key: 'links', name: 'Links', contentType: 'links', sortOrder: 0,
  isActive: true, createdAtUtc: '', updatedAtUtc: '',
}

const activeLink: AdminLinkResponse = {
  id: 'link-1', storeId: store.id, storeTabId: tab.id, type: 'instagram', label: 'Instagram', icon: null,
  url: 'https://instagram.com/cafe', sortOrder: 0, isActive: true, createdAtUtc: '', updatedAtUtc: '',
}

describe('ContentPage', () => {
  afterEach(() => { cleanup(); vi.restoreAllMocks() })

  it('removes a deleted link from the workspace', async () => {
    let links = [activeLink]
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockResolvedValue([tab])
    vi.stubGlobal('confirm', vi.fn(() => true))
    vi.spyOn(api, 'links').mockImplementation(async () => links)
    const deleteLink = vi.spyOn(api, 'deleteLink').mockImplementation(async () => { links = [] })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    await user.click(await screen.findByRole('button', { name: 'Excluir Instagram' }))

    expect(deleteLink).toHaveBeenCalledWith(store.id, activeLink.id, 'token')
    expect(screen.queryByRole('button', { name: 'Excluir Instagram' })).not.toBeInTheDocument()
  })

  it('removes a deleted tab from the content workspace', async () => {
    let tabs = [tab]
    vi.stubGlobal('confirm', vi.fn(() => true))
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockImplementation(async () => tabs)
    vi.spyOn(api, 'links').mockResolvedValue([])
    vi.spyOn(api, 'deleteTab').mockImplementation(async () => { tabs = [] })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    await user.click(await screen.findByRole('button', { name: 'Excluir Links' }))

    expect(await screen.findByText('Crie a primeira aba para começar.')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Excluir Links' })).not.toBeInTheDocument()
  })

  it('submits an icon selected from the preset menu', async () => {
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockResolvedValue([tab])
    vi.spyOn(api, 'links').mockResolvedValue([])
    const createLink = vi.spyOn(api, 'createLink').mockResolvedValue(activeLink)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    await user.type(await screen.findByLabelText('Texto'), 'Nosso mapa')
    await user.type(screen.getByLabelText('Destino'), 'https://maps.google.com/?q=cafe')
    await user.selectOptions(screen.getByLabelText('Ícone'), 'maps')
    await user.click(screen.getByRole('button', { name: 'Adicionar link' }))

    expect(createLink).toHaveBeenCalledWith(store.id, expect.objectContaining({ icon: 'maps' }), 'token')
  })

  it('edits a link inside its tab', async () => {
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockResolvedValue([tab])
    vi.spyOn(api, 'links').mockResolvedValue([activeLink])
    const updateLink = vi.spyOn(api, 'updateLink').mockResolvedValue(activeLink)

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    await user.click(await screen.findByRole('button', { name: 'Editar Instagram' }))
    await user.clear(screen.getByLabelText('Destino'))
    await user.type(screen.getByLabelText('Destino'), 'https://instagram.com/novo')
    await user.click(screen.getByRole('button', { name: 'Salvar link' }))

    expect(updateLink).toHaveBeenCalledWith(store.id, activeLink.id, expect.objectContaining({ url: 'https://instagram.com/novo', storeTabId: tab.id }), 'token')
  })
})
