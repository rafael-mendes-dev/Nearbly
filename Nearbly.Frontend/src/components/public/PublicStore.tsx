import { useEffect, useMemo, useRef, useState } from 'react'
import { ArrowUpRight, Camera, ExternalLink, Globe2, Mail, MapPin, Phone, QrCode, Smartphone } from 'lucide-react'
import { api, API_BASE_URL } from '../../lib/api/client'
import type { PublicLinkResponse, PublicStoreResponse } from '../../lib/api/types'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeSanitize from 'rehype-sanitize'
import { contrastText, isSafeColor } from '../../lib/format'
import { AnimatedContent } from '../react-bits/AnimatedContent'
import { AnimatedList } from '../react-bits/AnimatedList'
import { SpotlightCard } from '../react-bits/SpotlightCard'
import './public-store.css'

const iconFor = (type: string | null) => {
  const common = { size: 20, strokeWidth: 1.8 }
  if (type === 'instagram' || type === 'camera') return <Camera {...common} />
  if (type === 'email' || type === 'mail') return <Mail {...common} />
  if (type === 'phone' || type === 'whatsapp') return <Phone {...common} />
  if (type === 'website' || type === 'globe' || type === 'facebook') return <Globe2 {...common} />
  if (type === 'map' || type === 'location') return <MapPin {...common} />
  return <ExternalLink {...common} />
}

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

const linkHref = (link: PublicLinkResponse, source: string) => {
  const base = API_BASE_URL || window.location.origin
  const href = new URL(link.href, base)
  if (source) href.searchParams.set('src', source)
  return href.toString()
}

const mediaHref = (path: string) => path.startsWith('/media/') ? `${API_BASE_URL || window.location.origin}${path}` : path

function TabContent({ tab, source, primary }: { tab: PublicStoreResponse['tabs'][number]; source: string; primary: string }) {
  const [lightbox, setLightbox] = useState<string | null>(null)
  if (tab.contentType === 'products') {
    return tab.products.length ? <div className="public-products">
      {tab.products.map((product) => <article className={`public-product ${product.isAvailable ? '' : 'is-unavailable'}`} key={product.id}>
        <img loading="lazy" src={mediaHref(product.imageUrl)} alt="" />
        <div><h3>{product.name}</h3>{product.description && <p>{product.description}</p>}<strong>{product.price === null ? 'Consulte' : product.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</strong>{!product.isAvailable && <small>Indisponível</small>}</div>
      </article>)}
    </div> : <EmptyContent label="Nenhum produto publicado ainda." />
  }
  if (tab.contentType === 'markdown') {
    return tab.markdownBlocks.length ? <div className="public-markdown">{tab.markdownBlocks.map((block) => <article key={block.id}>{block.title && <h3>{block.title}</h3>}<ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>{block.markdown}</ReactMarkdown></article>)}</div> : <EmptyContent label="Nenhum texto publicado ainda." />
  }
  if (tab.contentType === 'gallery') {
    return tab.galleryItems.length ? <><div className="public-gallery">{tab.galleryItems.map((item) => <button type="button" key={item.id} onClick={() => setLightbox(item.imageUrl)}><img loading="lazy" src={mediaHref(item.imageUrl)} alt={item.altText} /><span>{item.caption}</span></button>)}</div>{lightbox && <div className="public-lightbox" role="dialog" aria-modal="true" aria-label="Imagem ampliada" onClick={() => setLightbox(null)}><button type="button" aria-label="Fechar imagem" onClick={() => setLightbox(null)}>×</button><img src={mediaHref(lightbox)} alt="" /></div>}</> : <EmptyContent label="Nenhuma imagem publicada ainda." />
  }
  return tab.links.length ? <AnimatedList className="public-links">{tab.links.map((link, index) => <SpotlightCard key={link.id} className="public-link-shell" spotlightColor={`${primary}32`}><a className="public-link" href={linkHref(link, source)}><span className="public-link-index">{String(index + 1).padStart(2, '0')}</span><span className="public-link-icon">{iconFor(link.icon || link.type)}</span><span className="public-link-copy"><strong>{link.label}</strong><small>{linkTypeLabel(link.type)}</small></span><ArrowUpRight className="public-link-arrow" size={20} /></a></SpotlightCard>)}</AnimatedList> : <EmptyContent label="Nenhum link publicado ainda." />
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

  useEffect(() => {
    if (hasRegisteredView.current) return
    hasRegisteredView.current = true
    void api.registerView(store.slug, source || undefined).catch(() => undefined)
  }, [source, store.slug])

  return (
    <main
      id="conteudo"
      className="public-store"
      style={{
        '--store-primary': safePrimary,
        '--store-secondary': safeSecondary,
        '--store-foreground': foreground,
        '--store-page-foreground': '#F7F8FF',
      } as React.CSSProperties}
    >
      <div className="public-store-accent" aria-hidden="true" />
      <div className="public-store-shell">
        <header className="public-store-header">
          <a className="public-store-badge" href="/" aria-label="Conhecer a Nearbly">
            <img src="/brand/logo-mark-white.svg" alt="" />
            <span>página Nearbly</span>
          </a>
          <span className="public-store-source">{sourceInfo.icon}{sourceInfo.label}</span>
        </header>

        <div className="public-store-layout">
          <AnimatedContent className="public-profile-motion" distance={26} duration={0.8}>
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

          <section className="public-directory" aria-labelledby="public-directory-title">
            <AnimatedContent distance={22} delay={0.08}>
              <div className="public-directory-heading">
                <div>
                  <span>Explore</span>
                  <h2 id="public-directory-title">Links oficiais</h2>
                </div>
                <small>{selectedTab.contentType === 'links' ? `${selectedTab.links.length} ${selectedTab.links.length === 1 ? 'destino' : 'destinos'}` : selectedTab.contentType}</small>
              </div>
              {tabs.length > 1 && (
                <nav className="public-tabs" aria-label="Categorias de links" role="tablist">
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
              {selectedTab && <TabContent tab={selectedTab} source={source} primary={safePrimary} />}
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
