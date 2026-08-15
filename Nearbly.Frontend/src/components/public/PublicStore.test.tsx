import { cleanup, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import PublicStore from './PublicStore'
import { api } from '../../lib/api/client'
import type { PublicStoreResponse } from '../../lib/api/types'

const store: PublicStoreResponse = {
  id: 'store', name: 'Café Central', slug: 'cafe-central', publicCode: 's_store', description: null, logoUrl: null, primaryColor: '#2B22E0', secondaryColor: '#06080F', links: [
    { id: 'instagram', type: 'instagram', label: 'Instagram', icon: null, href: '/r/instagram' },
    { id: 'whatsapp', type: 'whatsapp', label: 'WhatsApp', icon: null, href: '/r/whatsapp' },
  ], tabs: [
    { id: 'products', key: 'products', name: 'Produtos', contentType: 'products', sortOrder: 0, links: [], products: [{ id: 'product', name: 'Café coado', description: '250 ml', imageUrl: 'https://example.com/product.webp', price: 8.5, isAvailable: true, sortOrder: 0 }], markdownBlocks: [], galleryItems: [] },
    { id: 'markdown', key: 'markdown', name: 'Sobre', contentType: 'markdown', sortOrder: 1, links: [], products: [], markdownBlocks: [{ id: 'block', title: 'Horários', markdown: '## Semana\n\n<script>alert(1)</script>Aberto', sortOrder: 0 }], galleryItems: [] },
    { id: 'gallery', key: 'gallery', name: 'Fotos', contentType: 'gallery', sortOrder: 2, links: [], products: [], markdownBlocks: [], galleryItems: [{ id: 'image', imageUrl: 'https://example.com/gallery.webp', altText: 'Interior do café', caption: 'Salão', sortOrder: 0 }] },
  ],
}

describe('PublicStore content renderers', () => {
  afterEach(() => { cleanup(); vi.restoreAllMocks() })

  it('renders full-width tabs, opens product images, sanitizes markdown, and switches to gallery', async () => {
    vi.spyOn(api, 'registerView').mockResolvedValue(undefined)
    const user = userEvent.setup()
    render(<PublicStore store={store} source="direct" />)

    expect(screen.getByRole('heading', { name: 'Café coado' })).toBeInTheDocument()
    expect(screen.getByText('R$ 8,50')).toBeInTheDocument()
    expect(screen.getByRole('tablist')).toHaveStyle({ '--tab-count': '3' })

    await user.click(screen.getByRole('button', { name: 'Ampliar imagem de Café coado' }))
    expect(within(screen.getByRole('dialog', { name: 'Imagem ampliada' })).getByAltText('Imagem de Café coado')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Fechar imagem' }))
    expect(screen.queryByRole('dialog', { name: 'Imagem ampliada' })).not.toBeInTheDocument()

    await user.click(screen.getByRole('tab', { name: 'Sobre' }))
    expect(await screen.findByRole('heading', { name: 'Horários' })).toBeInTheDocument()
    expect(screen.queryByText('alert(1)')).not.toBeInTheDocument()

    await user.click(screen.getByRole('tab', { name: 'Fotos' }))
    expect(await screen.findByAltText('Interior do café')).toBeInTheDocument()
  })

  it('promotes the main contact as call to action and offers it on available products', () => {
    vi.spyOn(api, 'registerView').mockResolvedValue(undefined)
    render(<PublicStore store={store} source="qr_code" />)

    expect(screen.getByRole('link', { name: /Pedir no WhatsApp/ })).toHaveAttribute('href', expect.stringContaining('/r/whatsapp?src=qr_code'))
    expect(screen.getByRole('link', { name: 'Instagram' })).toHaveAttribute('href', expect.stringContaining('/r/instagram?src=qr_code'))
    expect(screen.getByRole('link', { name: /Pedir agora/ })).toHaveAttribute('href', expect.stringContaining('/r/whatsapp?src=qr_code'))
    expect(screen.getByText('1 produto')).toBeInTheDocument()
  })

  it('hides purchase actions for stores without links and for unavailable products', () => {
    vi.spyOn(api, 'registerView').mockResolvedValue(undefined)
    const soldOut: PublicStoreResponse = {
      ...store,
      links: [],
      tabs: [{ ...store.tabs[0], products: [{ ...store.tabs[0].products[0], isAvailable: false }] }],
    }
    render(<PublicStore store={soldOut} source="direct" />)

    expect(screen.queryByRole('link', { name: /Pedir/ })).not.toBeInTheDocument()
    expect(screen.getByText('Indisponível')).toBeInTheDocument()
  })
})
