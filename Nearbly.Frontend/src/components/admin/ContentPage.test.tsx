import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import ContentPage from './ContentPage'
import { api } from '../../lib/api/client'
import type { AdminLinkResponse, StoreResponse, TabResponse } from '../../lib/api/types'

const store: StoreResponse = {
  id: 'store-1', name: 'Café Central', slug: 'cafe-central', description: null, logoUrl: null, logoMediaId: null,
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

  it('updates a link to inactive after deactivation', async () => {
    let isActive = true
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockResolvedValue([tab])
    vi.spyOn(api, 'links').mockImplementation(async () => [{ ...activeLink, isActive }])
    vi.spyOn(api, 'deactivateLink').mockImplementation(async () => { isActive = false })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    const deactivateButton = await screen.findByRole('button', { name: 'Desativar Instagram' })
    await user.click(deactivateButton)

    expect(await screen.findByText('Inativo')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Desativar Instagram' })).not.toBeInTheDocument()
  })

  it('removes a deactivated tab from the active content workspace', async () => {
    let isActive = true
    vi.stubGlobal('confirm', vi.fn(() => true))
    vi.spyOn(api, 'store').mockResolvedValue(store)
    vi.spyOn(api, 'tabs').mockImplementation(async () => [{ ...tab, isActive }])
    vi.spyOn(api, 'links').mockResolvedValue([])
    vi.spyOn(api, 'deactivateTab').mockImplementation(async () => { isActive = false })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
    const user = userEvent.setup()
    render(<QueryClientProvider client={queryClient}><MemoryRouter initialEntries={[`/lojas/${store.id}/conteudo`]}><Routes><Route path="/lojas/:storeId/conteudo" element={<ContentPage token="token" />} /></Routes></MemoryRouter></QueryClientProvider>)

    await user.click(await screen.findByRole('button', { name: 'Desativar Links' }))

    expect(await screen.findByText('Nenhuma aba ativa.')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Desativar Links' })).not.toBeInTheDocument()
  })
})
