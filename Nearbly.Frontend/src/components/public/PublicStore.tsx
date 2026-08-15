import { useEffect, useMemo, useRef, useState } from 'react'
import { ArrowUpRight, Globe2, Maximize2, QrCode, Smartphone } from 'lucide-react'
import { api, API_BASE_URL } from '../../lib/api/client'
import { linkIcon } from '../../lib/link-icons'
import type { PublicLinkResponse, PublicStoreResponse } from '../../lib/api/types'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeSanitize from 'rehype-sanitize'
import { contrastText, isSafeColor, shade } from '../../lib/format'
import { AnimatedContent } from '../react-bits/AnimatedContent'
import { AnimatedList } from '../react-bits/AnimatedList'
import { SpotlightCard } from '../react-bits/SpotlightCard'
import './public-store.css'

const linkTypeLabel = (type: string) => {
  const labels: Record<string, string> = {
    email: 'E-mail',
    facebook: 'Facebook',
    instagram: 'Instagram',
    location: 'Localização',
    map: 'Como chegar',
    phone: 'Telefone',
    website: 'Site',
    whatsapp: 'WhatsApp',
  }
  return labels[type.toLowerCase()] ?? type
}

const sourceDetails = (source: string) => {
  if (source === 'nfc') return { label: 'acesso por NFC', icon: <Smartphone size={14} /> }
  if (source === 'qr_code') return { label: 'acesso por QR Code', icon: <QrCode size={14} /> }
  return { label: 'presença digital', icon: <Globe2 size={14} /> }
}

const contactPriority = ['whatsapp', 'phone', 'email', 'map', 'location', 'website']
const linkKind = (link: PublicLinkResponse) => (link.icon || link.type).trim().toLowerCase()

const pickContact = (links: PublicLinkResponse[]) => {
  for (const kind of contactPriority) {
    const found = links.find((link) => linkKind(link) === kind || link.type.trim().toLowerCase() === kind)
    if (found) return found
  }
  return links[0] ?? null
}

const contactCopy = (link: PublicLinkResponse | null) => {
  switch (link ? linkKind(link) : '') {
    case 'whatsapp': return { primary: 'Pedir no WhatsApp', product: 'Pedir agora' }
    case 'phone': return { primary: 'Ligar para a loja', product: 'Tenho interesse' }
    case 'email': return { primary: 'Enviar e-mail', product: 'Tenho interesse' }
    case 'map': case 'maps': case 'location': return { primary: 'Como chegar', product: 'Ver na loja' }
    default: return { primary: 'Falar com a loja', product: 'Tenho interesse' }
  }
}

const plural = (count: number, singular: string, many: string) => `${count} ${count === 1 ? singular : many}`

const tabSummary = (tab: PublicStoreResponse['tabs'][number]) => {
  if (tab.contentType === 'products') return plural(tab.products.length, 'produto', 'produtos')
  if (tab.contentType === 'gallery') return plural(tab.galleryItems.length, 'foto', 'fotos')
  if (tab.contentType === 'markdown') return plural(tab.markdownBlocks.length, 'seção', 'seções')
  return plural(tab.links.length, 'destino', 'destinos')
}

const tabTitle = (tab: PublicStoreResponse['tabs'][number]) => {
  if (tab.contentType === 'products') return 'Destaques da loja'
  if (tab.contentType === 'gallery') return 'Galeria'
  if (tab.contentType === 'markdown') return 'Informações'
  return 'Links oficiais'
}

const linkHref = (link: PublicLinkResponse, source: string) => {
  const base = API_BASE_URL || window.location.origin
  const href = new URL(link.href, base)
  if (source) href.searchParams.set('src', source)
  return href.toString()
}

const mediaHref = (path: string) => path.startsWith('/media/') ? `${API_BASE_URL || window.location.origin}${path}` : path

function TabContent({ tab, source, primary, contact }: { tab: PublicStoreResponse['tabs'][number]; source: string; primary: string; contact: PublicLinkResponse | null }) {
  const [lightbox, setLightbox] = useState<{ imageUrl: string; altText: string } | null>(null)
  if (tab.contentType === 'products') {
    return tab.products.length ? <><AnimatedList className="public-products">
      {tab.products.map((product) => <article className={`public-product ${product.isAvailable ? '' : 'is-unavailable'}`} key={product.id}>
        <button className="public-product-image" type="button" onClick={() => setLightbox({ imageUrl: product.imageUrl, altText: `Imagem de ${product.name}` })} aria-label={`Ampliar imagem de ${product.name}`}>
          <img loading="lazy" src={mediaHref(product.imageUrl)} alt={`Imagem de ${product.name}`} />
          <span className="public-product-zoom" aria-hidden="true"><Maximize2 size={15} /></span>
          {!product.isAvailable && <small>Indisponível</small>}
        </button>
        <div className="public-product-body">
          <h3>{product.name}</h3>
          {product.description && <p>{product.description}</p>}
          <div className="public-product-buy">
            <strong>{product.price === null ? 'Consulte' : product.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</strong>
            {contact && product.isAvailable && <a className="public-product-cta" href={linkHref(contact, source)}>{contactCopy(contact).product}<ArrowUpRight size={14} /></a>}
          </div>
        </div>
      </article>)}
    </AnimatedList>{lightbox && <ImageLightbox imageUrl={lightbox.imageUrl} altText={lightbox.altText} onClose={() => setLightbox(null)} />}</> : <EmptyContent label="Nenhum produto publicado ainda." />
  }
  if (tab.contentType === 'markdown') {
    return tab.markdownBlocks.length ? <div className="public-markdown">{tab.markdownBlocks.map((block) => <article key={block.id}>{block.title && <h3>{block.title}</h3>}<ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>{block.markdown}</ReactMarkdown></article>)}</div> : <EmptyContent label="Nenhum texto publicado ainda." />
  }
  if (tab.contentType === 'gallery') {
    return tab.galleryItems.length ? <><div className="public-gallery">{tab.galleryItems.map((item) => <button type="button" key={item.id} onClick={() => setLightbox({ imageUrl: item.imageUrl, altText: item.altText })}><img loading="lazy" src={mediaHref(item.imageUrl)} alt={item.altText} /><i aria-hidden="true"><Maximize2 size={15} /></i>{item.caption && <span>{item.caption}</span>}</button>)}</div>{lightbox && <ImageLightbox imageUrl={lightbox.imageUrl} altText={lightbox.altText} onClose={() => setLightbox(null)} />}</> : <EmptyContent label="Nenhuma imagem publicada ainda." />
  }
  return tab.links.length ? <AnimatedList className="public-links">{tab.links.map((link, index) => <SpotlightCard key={link.id} className="public-link-shell" spotlightColor={`${primary}32`}><a className="public-link" href={linkHref(link, source)}><span className="public-link-index">{String(index + 1).padStart(2, '0')}</span><span className="public-link-icon">{linkIcon(link.icon || link.type)}</span><span className="public-link-copy"><strong>{link.label}</strong><small>{linkTypeLabel(link.type)}</small></span><ArrowUpRight className="public-link-arrow" size={20} /></a></SpotlightCard>)}</AnimatedList> : <EmptyContent label="Nenhum link publicado ainda." />
}

function ImageLightbox({ imageUrl, altText, onClose }: { imageUrl: string; altText: string; onClose: () => void }) {
  return <div className="public-lightbox" role="dialog" aria-modal="true" aria-label="Imagem ampliada" onMouseDown={onClose}><button type="button" aria-label="Fechar imagem" onMouseDown={(event) => event.stopPropagation()} onClick={onClose}>×</button><img src={mediaHref(imageUrl)} alt={altText} /></div>
}

function EmptyContent({ label }: { label: string }) {
  return <div className="public-empty"><Smartphone size={24} /><p>{label}</p></div>
}

export default function PublicStore({ store, source }: { store: PublicStoreResponse; source: string }) {
  const [activeTab, setActiveTab] = useState(store.tabs[0]?.id ?? 'root')
  const hasRegisteredView = useRef(false)
  const safePrimary = isSafeColor(store.primaryColor) ? store.primaryColor : '#2B22E0'
  const safeSecondary = isSafeColor(store.secondaryColor) ? store.secondaryColor : '#06080F'
  const foreground = contrastText(safePrimary)
  const tabs = useMemo(() => store.tabs.length ? store.tabs : [{ id: 'root', key: 'root', name: 'Todos', contentType: 'links' as const, sortOrder: 0, links: store.links, products: [], markdownBlocks: [], galleryItems: [] }], [store.links, store.tabs])
  const selectedTab = tabs.find((tab) => tab.id === activeTab) ?? tabs[0]
  const sourceInfo = sourceDetails(source)
  const initials = store.name.trim().split(/\s+/).slice(0, 2).map((word) => word[0]).join('').toUpperCase()
  const allLinks = useMemo(() => [...new Map([...store.links, ...store.tabs.flatMap((tab) => tab.links)].map((link) => [link.id, link])).values()], [store.links, store.tabs])
  const contact = useMemo(() => pickContact(allLinks), [allLinks])
  const shortcuts = useMemo(() => allLinks.filter((link) => link.id !== contact?.id).slice(0, 3), [allLinks, contact])

  useEffect(() => {
    if (hasRegisteredView.current) return
    hasRegisteredView.current = true
    void api.registerView(store.publicCode, source || undefined).catch(() => undefined)
  }, [source, store.publicCode])

  return (
    <main
      id="conteudo"
      className={`public-store ${contact ? 'has-cta' : ''}`}
      style={{
        '--store-primary': safePrimary,
        '--store-secondary': safeSecondary,
        '--store-foreground': foreground,
        '--store-page-foreground': '#F7F8FF',
        '--store-base': shade(safeSecondary, 0.052, 0.6),
        '--store-surface': shade(safeSecondary, 0.1, 0.55),
        '--store-surface-strong': shade(safeSecondary, 0.155, 0.5),
        '--store-line': shade(safeSecondary, 0.28, 0.38),
        '--store-glow': shade(safeSecondary, 0.34, 0.5),
      } as React.CSSProperties}
    >
      <div className="public-store-accent" aria-hidden="true" />
      <div className="public-store-glow" aria-hidden="true"><i /><i /></div>
      <div className="public-store-shell">
        <header className="public-store-header">
          <a className="public-store-badge" href="/" aria-label="Conhecer a Nearbly">
            <img src="/brand/logo-mark-white.svg" alt="" />
            <span>página Nearbly</span>
          </a>
          <span className="public-store-source">{sourceInfo.icon}{sourceInfo.label}</span>
        </header>

        <div className="public-store-layout">
          <div className="public-profile-motion">
            <AnimatedContent distance={26} duration={0.8}>
              <section className="public-profile" aria-labelledby="public-store-name">
                <div className="public-logo-frame">
                  <div className="public-logo">
                    {store.logoUrl
                      ? <img src={mediaHref(store.logoUrl)} alt={`Logo de ${store.name}`} />
                      : <span>{initials}</span>}
                  </div>
                </div>
                <span className="public-profile-kicker"><i /> página oficial</span>
                <h1 id="public-store-name">{store.name}</h1>
                {store.description && <p>{store.description}</p>}
              </section>
            </AnimatedContent>
            {contact && (
              <div className="public-cta-dock">
                <a className="public-cta" href={linkHref(contact, source)}>
                  <span className="public-cta-icon" aria-hidden="true">{linkIcon(contact.icon || contact.type, 18)}</span>
                  {contactCopy(contact).primary}
                  <ArrowUpRight size={18} />
                </a>
                {shortcuts.length > 0 && (
                  <div className="public-cta-shortcuts">
                    {shortcuts.map((link) => (
                      <a key={link.id} href={linkHref(link, source)} aria-label={link.label} title={link.label}>{linkIcon(link.icon || link.type, 17)}</a>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          <section className="public-directory" aria-labelledby="public-directory-title">
            <AnimatedContent distance={22} delay={0.08}>
              <div className="public-directory-heading">
                <div>
                  <span>Explore</span>
                  <h2 id="public-directory-title">{tabTitle(selectedTab)}</h2>
                </div>
                <small>{tabSummary(selectedTab)}</small>
              </div>
              {tabs.length > 1 && (
                <nav className="public-tabs" aria-label="Categorias de links" role="tablist" style={{ '--tab-count': tabs.length } as React.CSSProperties}>
                  {tabs.map((tab) => (
                    <button
                      type="button"
                      role="tab"
                      key={tab.id}
                      className={activeTab === tab.id ? 'is-active' : ''}
                      aria-selected={activeTab === tab.id}
                      aria-controls="public-links-panel"
                      onClick={() => setActiveTab(tab.id)}
                    >
                      {tab.name}
                    </button>
                  ))}
                </nav>
              )}
            </AnimatedContent>

            <div id="public-links-panel" className="public-links-panel" role="tabpanel" tabIndex={0}>
              {selectedTab && <TabContent tab={selectedTab} source={source} primary={safePrimary} contact={contact} />}
            </div>

            <footer className="public-footer">
              <span>Feito com</span>
              <a href="/" aria-label="Conhecer a Nearbly">
                <img src="/brand/logo-mark-white.svg" alt="" />
                Nearbly
              </a>
            </footer>
          </section>
        </div>
      </div>
    </main>
  )
}
