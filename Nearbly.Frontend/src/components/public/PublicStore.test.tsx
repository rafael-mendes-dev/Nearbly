import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import PublicStore from './PublicStore'
import { api } from '../../lib/api/client'
import type { PublicStoreResponse } from '../../lib/api/types'

const store: PublicStoreResponse = {
  id: 'store', name: 'Café Central', slug: 'cafe-central', description: null, logoUrl: null, primaryColor: '#2B22E0', secondaryColor: '#06080F', links: [], tabs: [
    { id: 'products', key: 'products', name: 'Produtos', contentType: 'products', sortOrder: 0, links: [], products: [{ id: 'product', name: 'Café coado', description: '250 ml', imageUrl: 'https://example.com/product.webp', price: 8.5, isAvailable: true, sortOrder: 0 }], markdownBlocks: [], galleryItems: [] },
    { id: 'markdown', key: 'markdown', name: 'Sobre', contentType: 'markdown', sortOrder: 1, links: [], products: [], markdownBlocks: [{ id: 'block', title: 'Horários', markdown: '## Semana\n\n<script>alert(1)</script>Aberto', sortOrder: 0 }], galleryItems: [] },
    { id: 'gallery', key: 'gallery', name: 'Fotos', contentType: 'gallery', sortOrder: 2, links: [], products: [], markdownBlocks: [], galleryItems: [{ id: 'image', imageUrl: 'https://example.com/gallery.webp', altText: 'Interior do café', caption: 'Salão', sortOrder: 0 }] },
  ],
}

describe('PublicStore content renderers', () => {
  afterEach(() => vi.restoreAllMocks())

  it('renders the first tab, sanitizes markdown, and switches to products and gallery', async () => {
    vi.spyOn(api, 'registerView').mockResolvedValue(undefined)
    render(<PublicStore store={store} source="direct" />)

    expect(screen.getByRole('heading', { name: 'Café coado' })).toBeInTheDocument()
    expect(screen.getByText('R$ 8,50')).toBeInTheDocument()

    screen.getByRole('tab', { name: 'Sobre' }).click()
    expect(await screen.findByRole('heading', { name: 'Horários' })).toBeInTheDocument()
    expect(screen.queryByText('alert(1)')).not.toBeInTheDocument()

    screen.getByRole('tab', { name: 'Fotos' }).click()
    expect(await screen.findByAltText('Interior do café')).toBeInTheDocument()
  })
})
