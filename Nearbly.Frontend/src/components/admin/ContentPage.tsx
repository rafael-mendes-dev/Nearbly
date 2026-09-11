import { useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeSanitize from 'rehype-sanitize'
import { ImagePlus, Link2, Plus, Settings, Trash2, Upload, X } from 'lucide-react'
import { api, API_BASE_URL } from '../../lib/api/client'
import { linkIcon, linkIconOptions } from '../../lib/link-icons'
import { problemMessage } from '../../lib/api/problem'
import type { AdminLinkResponse, ContentType, GalleryItemResponse, LinkInput, MarkdownBlockResponse, ProductResponse, StoreResponse, TabInput, TabResponse } from '../../lib/api/types'

const types: Array<{ value: ContentType; label: string; hint: string }> = [
  { value: 'links', label: 'Links', hint: 'Ações e destinos rastreados.' },
  { value: 'products', label: 'Produtos', hint: 'Vitrine informativa sem checkout.' },
  { value: 'markdown', label: 'Texto Markdown', hint: 'Texto formatado e sanitizado.' },
  { value: 'gallery', label: 'Galeria', hint: 'Imagens com legenda e lightbox.' },
]

const mediaHref = (path: string) => path.startsWith('/media/') ? `${API_BASE_URL || window.location.origin}${path}` : path

export default function ContentPage({ token }: { token: string }) {
  const { storeId = '' } = useParams<{ storeId: string }>()
  const queryClient = useQueryClient()
  const store = useQuery({ queryKey: ['store', storeId], queryFn: () => api.store(storeId, token), enabled: Boolean(storeId) })
  const tabs = useQuery({ queryKey: ['tabs', storeId], queryFn: () => api.tabs(storeId, token), enabled: Boolean(storeId) })
  const [selectedId, setSelectedId] = useState('')
  const [showTabForm, setShowTabForm] = useState(false)
  const availableTabs = tabs.data ?? []
  const selected = availableTabs.find((tab) => tab.id === selectedId) ?? availableTabs[0]

  const saveTab = useMutation({
    mutationFn: (input: TabInput | { id: string; input: TabInput }) => 'id' in input
      ? api.updateTab(storeId, input.id, { ...input.input, isActive: selected?.isActive }, token)
      : api.createTab(storeId, input, token),
    onSuccess: (tab) => { void queryClient.invalidateQueries({ queryKey: ['tabs', storeId] }); setSelectedId(tab.id); setShowTabForm(false) },
  })
  const deleteTab = useDelete<TabResponse>(['tabs', storeId], (id) => api.deleteTab(storeId, id, token))

  return <section className="admin-section content-workspace">
    <div className="section-top">
      <div><span className="eyebrow">Conteúdo {store.data ? ` / ${store.data.name}` : ''}</span><h1>Conteúdo</h1><p>Organize abas e escolha o formato que cada visitante vai encontrar.</p></div>
      <button className="button button-dark" type="button" onClick={() => setShowTabForm(true)}><Plus size={17} /> Nova aba</button>
    </div>
    {tabs.error && <div className="alert alert-error">{problemMessage(tabs.error)}</div>}
    {deleteTab.error && <div className="alert alert-error">{problemMessage(deleteTab.error)}</div>}
    {showTabForm && <TabEditor saving={saveTab.isPending} error={saveTab.error} onCancel={() => setShowTabForm(false)} onSave={(input) => saveTab.mutate(input)} />}
    <div className="content-layout">
      <aside className="content-tabs" aria-label="Abas de conteúdo">
        <div className="content-tabs-heading"><span>Abas</span><strong>{availableTabs.length}</strong></div>
        {availableTabs.map((tab) => <button type="button" className={selected?.id === tab.id ? 'is-active' : ''} key={tab.id} onClick={() => setSelectedId(tab.id)}><span><strong>{tab.name}</strong><small>{typeLabel(tab.contentType)}</small></span><span className="content-tab-order">{String(tab.sortOrder).padStart(2, '0')}</span></button>)}
        {!availableTabs.length && <p className="empty-state">Crie a primeira aba para começar.</p>}
      </aside>
      {selected ? <ContentEditor key={selected.id} store={store.data} tab={selected} token={token} onUpdateTab={(input) => saveTab.mutate({ id: selected.id, input })} onDelete={() => window.confirm(`Excluir ${selected.name} permanentemente?`) && deleteTab.mutate(selected.id)} /> : <div className="content-empty"><ImagePlus size={28} /><strong>Seu conteúdo começa aqui</strong><p>Crie uma aba e selecione um formato.</p></div>}
    </div>
  </section>
}

function ContentEditor({ store, tab, token, onUpdateTab, onDelete }: { store: StoreResponse | undefined; tab: TabResponse; token: string; onUpdateTab: (input: TabInput) => void; onDelete: () => void }) {
  const type = tab.contentType ?? 'links'
  return <div className="content-editor">
    <div className="content-editor-heading"><div><span className="eyebrow">{typeLabel(type)}</span><h2>{tab.name}</h2><p>{types.find((item) => item.value === type)?.hint}</p></div><div className="content-editor-actions"><button className="button-icon" type="button" onClick={onDelete} aria-label={`Excluir ${tab.name}`} title="Excluir aba"><Trash2 size={17} /></button></div></div>
    <TabSettings tab={tab} onSave={onUpdateTab} />
    {type === 'links' && <LinksEditor storeId={tab.storeId} tabId={tab.id} token={token} />}
    {type === 'products' && <ProductsEditor storeId={tab.storeId} tabId={tab.id} token={token} />}
    {type === 'markdown' && <MarkdownEditor storeId={tab.storeId} tabId={tab.id} token={token} />}
    {type === 'gallery' && <GalleryEditor storeId={tab.storeId} tabId={tab.id} token={token} />}
    {store && <p className="content-footnote">As alterações aparecem na página pública de <strong>/{store.publicCode}</strong>.</p>}
  </div>
}

function TabEditor({ tab, saving, error, onCancel, onSave }: { tab?: TabResponse; saving: boolean; error: unknown; onCancel: () => void; onSave: (input: TabInput) => void }) {
  const [key, setKey] = useState(tab?.key ?? '')
  const [name, setName] = useState(tab?.name ?? '')
  const [sortOrder, setSortOrder] = useState(tab?.sortOrder ?? 0)
  const [contentType, setContentType] = useState<ContentType>(tab?.contentType ?? 'links')
  return <div className="form-sheet">
    <div className="sheet-heading"><div><span className="eyebrow">{tab ? 'Editar aba' : 'Nova aba'}</span><h2>{tab?.name ?? 'Escolha um formato'}</h2></div><button className="button-icon" type="button" onClick={onCancel} aria-label="Fechar formulário"><X size={18} /></button></div>
    <form className="form-grid" onSubmit={(event) => { event.preventDefault(); onSave({ key, name, sortOrder, contentType }) }}>
      <label className="field"><span>Chave</span><input value={key} onChange={(event) => setKey(event.target.value)} placeholder="menu" required maxLength={80} /></label><label className="field"><span>Nome exibido</span><input value={name} onChange={(event) => setName(event.target.value)} placeholder="Menu" required maxLength={120} /></label><label className="field"><span>Ordem</span><input type="number" min="0" value={sortOrder} onChange={(event) => setSortOrder(Number(event.target.value))} /></label><label className="field"><span>Tipo de conteúdo</span><select value={contentType} onChange={(event) => setContentType(event.target.value as ContentType)} disabled={Boolean(tab)}>{types.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}</select></label>
      {error ? <div className="alert alert-error">{problemMessage(error)}</div> : null}
      <div className="sheet-actions"><button className="button button-quiet" type="button" onClick={onCancel}>Cancelar</button><button className="button button-dark" type="submit" disabled={saving}>{saving ? 'Salvando…' : 'Salvar aba'}</button></div>
    </form>
  </div>
}

function TabSettings({ tab, onSave }: { tab: TabResponse; onSave: (input: TabInput) => void }) {
  const [open, setOpen] = useState(false)
  const [key, setKey] = useState(tab.key)
  const [name, setName] = useState(tab.name)
  const [sortOrder, setSortOrder] = useState(tab.sortOrder)
  if (!open) return <button className="content-settings-trigger" type="button" onClick={() => setOpen(true)}><Settings size={15} /> Editar aba</button>
  return <form className="content-inline-settings" onSubmit={(event) => { event.preventDefault(); onSave({ key, name, sortOrder, contentType: tab.contentType }); setOpen(false) }}><label>Chave<input value={key} onChange={(event) => setKey(event.target.value)} required /></label><label>Nome<input value={name} onChange={(event) => setName(event.target.value)} required /></label><label>Ordem<input type="number" min="0" value={sortOrder} onChange={(event) => setSortOrder(Number(event.target.value))} /></label><button className="button button-dark" type="submit">Salvar</button><button className="button button-quiet" type="button" onClick={() => setOpen(false)}>Cancelar</button></form>
}

function LinksEditor({ storeId, tabId, token }: { storeId: string; tabId: string; token: string }) {
  const links = useQuery({ queryKey: ['links', storeId], queryFn: () => api.links(storeId, token) })
  const deleteLink = useDelete<AdminLinkResponse>(['links', storeId], (id) => api.deleteLink(storeId, id, token))
  const [label, setLabel] = useState(''); const [url, setUrl] = useState(''); const [type, setType] = useState('website'); const [icon, setIcon] = useState('')
  const [editing, setEditing] = useState<AdminLinkResponse | null>(null)
  const clear = () => { setLabel(''); setUrl(''); setType('website'); setIcon(''); setEditing(null) }
  const save = useMutation({ mutationFn: () => {
    const input: LinkInput = { type, label, icon: icon || null, url, sortOrder: editing?.sortOrder ?? (links.data?.filter((item) => item.storeTabId === tabId).length ?? 0), storeTabId: tabId }
    return editing ? api.updateLink(storeId, editing.id, { ...input, isActive: editing.isActive }, token) : api.createLink(storeId, input, token)
  }, onSuccess: () => { clear(); void links.refetch() } })
  const tabLinks = (links.data ?? []).filter((link) => link.storeTabId === tabId)
  return <ContentPanel title="Links" count={tabLinks.length}>
    <form className="content-add-form" onSubmit={(event) => { event.preventDefault(); save.mutate() }}>
      <label className="field"><span>Texto</span><input value={label} onChange={(event) => setLabel(event.target.value)} placeholder="Fale conosco" required /></label>
      <label className="field"><span>Destino</span><input type="url" value={url} onChange={(event) => setUrl(event.target.value)} placeholder="https://..." required /></label>
      <label className="field"><span>Tipo</span><select value={type} onChange={(event) => setType(event.target.value)}><option value="website">Site</option><option value="instagram">Instagram</option><option value="whatsapp">WhatsApp</option><option value="facebook">Facebook</option><option value="location">Localização</option></select></label>
      <label className="field"><span>Ícone</span><div className="icon-select-control"><span className="icon-select-preview" aria-hidden="true">{linkIcon(icon || type, 19)}</span><select value={icon} onChange={(event) => setIcon(event.target.value)}>{linkIconOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}</select></div></label>
      <button className="button button-dark" type="submit" disabled={save.isPending}>{editing ? <><Settings size={16} /> Salvar link</> : <><Plus size={16} /> Adicionar link</>}</button>
      {editing && <button className="button button-quiet" type="button" onClick={clear}>Cancelar edição</button>}
    </form>
    {save.error && <div className="alert alert-error">{problemMessage(save.error)}</div>}
    {deleteLink.error && <div className="alert alert-error">{problemMessage(deleteLink.error)}</div>}
    <div className="content-items">{tabLinks.map((link) => <div className="content-item-row" key={link.id}><span className="content-item-mark">{linkIcon(link.icon || link.type, 17)}</span><span><strong>{link.label}</strong><small>{link.url}</small></span><button className="button-icon" type="button" onClick={() => { setEditing(link); setLabel(link.label); setUrl(link.url); setType(link.type); setIcon(link.icon ?? '') }} aria-label={`Editar ${link.label}`}><Settings size={15} /></button><button className="button-icon" type="button" disabled={deleteLink.isPending} onClick={() => window.confirm(`Excluir ${link.label} permanentemente?`) && deleteLink.mutate(link.id)} aria-label={`Excluir ${link.label}`}><Trash2 size={15} /></button></div>)}{!tabLinks.length && <p className="empty-state">Nenhum link nesta aba.</p>}</div>
  </ContentPanel>
}

function ProductsEditor({ storeId, tabId, token }: { storeId: string; tabId: string; token: string }) {
  const products = useQuery({ queryKey: ['products', storeId, tabId], queryFn: () => api.products(storeId, tabId, token) })
  const deactivate = useDelete<ProductResponse>(['products', storeId, tabId], (id) => api.deleteProduct(storeId, tabId, id, token))
  const [name, setName] = useState(''); const [description, setDescription] = useState(''); const [price, setPrice] = useState(''); const [available, setAvailable] = useState(true); const [file, setFile] = useState<File | null>(null)
  const create = useMutation({ mutationFn: async () => { if (!file) throw new Error('Selecione uma imagem.'); const media = await api.uploadMedia(storeId, file, token); return api.createProduct(storeId, tabId, { name, description: description || null, mediaAssetId: media.id, price: price ? Number(price) : null, isAvailable: available, sortOrder: products.data?.length ?? 0 }, token) }, onSuccess: () => { setName(''); setDescription(''); setPrice(''); setFile(null); void products.refetch() } })
  return <ContentPanel title="Produtos" count={products.data?.length ?? 0}><form className="content-add-form content-product-form" onSubmit={(event) => { event.preventDefault(); create.mutate() }}><label className="media-drop"><input type="file" accept="image/jpeg,image/png,image/webp" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /><Upload size={20} /><span>{file?.name ?? 'Adicionar imagem'}</span><small>JPEG, PNG ou WebP até 5 MB</small></label><label className="field"><span>Nome</span><input value={name} onChange={(event) => setName(event.target.value)} required maxLength={160} /></label><label className="field"><span>Descrição</span><textarea value={description} onChange={(event) => setDescription(event.target.value)} rows={3} /></label><label className="field"><span>Preço em BRL</span><input type="number" min="0" step="0.01" value={price} onChange={(event) => setPrice(event.target.value)} placeholder="Opcional" /></label><label className="check-field"><input type="checkbox" checked={available} onChange={(event) => setAvailable(event.target.checked)} /> Disponível</label><button className="button button-dark" type="submit" disabled={create.isPending}><Plus size={16} /> Adicionar produto</button></form>{create.error && <div className="alert alert-error">{problemMessage(create.error)}</div>}{deactivate.error && <div className="alert alert-error">{problemMessage(deactivate.error)}</div>}<div className="content-items">{(products.data ?? []).map((product) => <ContentItem key={product.id} image={product.imageUrl} title={product.name} detail={product.price === null ? 'Preço sob consulta' : product.price.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })} pending={deactivate.isPending} onDelete={() => window.confirm(`Excluir ${product.name} permanentemente?`) && deactivate.mutate(product.id)} />)}{!products.data?.length && <p className="empty-state">Nenhum produto nesta aba.</p>}</div></ContentPanel>
}

function MarkdownEditor({ storeId, tabId, token }: { storeId: string; tabId: string; token: string }) {
  const blocks = useQuery({ queryKey: ['markdown', storeId, tabId], queryFn: () => api.markdownBlocks(storeId, tabId, token) })
  const deactivate = useDelete<MarkdownBlockResponse>(['markdown', storeId, tabId], (id) => api.deleteMarkdownBlock(storeId, tabId, id, token))
  const [title, setTitle] = useState(''); const [markdown, setMarkdown] = useState(''); const [preview, setPreview] = useState(false)
  const create = useMutation({ mutationFn: () => api.createMarkdownBlock(storeId, tabId, { title: title || null, markdown, sortOrder: blocks.data?.length ?? 0 }, token), onSuccess: () => { setTitle(''); setMarkdown(''); void blocks.refetch() } })
  return <ContentPanel title="Texto Markdown" count={blocks.data?.length ?? 0}><div className="markdown-editor-tabs"><button type="button" className={!preview ? 'is-active' : ''} onClick={() => setPreview(false)}>Editar</button><button type="button" className={preview ? 'is-active' : ''} onClick={() => setPreview(true)}>Visualizar</button></div>{preview ? <div className="markdown-preview"><ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>{markdown || '*Nada para visualizar ainda.*'}</ReactMarkdown></div> : <form className="content-add-form" onSubmit={(event) => { event.preventDefault(); create.mutate() }}><label className="field"><span>Título opcional</span><input value={title} onChange={(event) => setTitle(event.target.value)} maxLength={160} /></label><label className="field"><span>Markdown</span><textarea value={markdown} onChange={(event) => setMarkdown(event.target.value)} rows={8} required maxLength={20000} placeholder="## Horários e atendimento" /></label><button className="button button-dark" type="submit" disabled={create.isPending}><Plus size={16} /> Adicionar bloco</button></form>}{create.error && <div className="alert alert-error">{problemMessage(create.error)}</div>}{deactivate.error && <div className="alert alert-error">{problemMessage(deactivate.error)}</div>}<div className="content-items">{(blocks.data ?? []).map((block) => <ContentItem key={block.id} title={block.title ?? 'Bloco sem título'} detail={block.markdown.slice(0, 100)} pending={deactivate.isPending} onDelete={() => window.confirm(`Excluir ${block.title ?? 'este bloco'} permanentemente?`) && deactivate.mutate(block.id)} />)}{!blocks.data?.length && <p className="empty-state">Nenhum bloco nesta aba.</p>}</div></ContentPanel>
}

function GalleryEditor({ storeId, tabId, token }: { storeId: string; tabId: string; token: string }) {
  const items = useQuery({ queryKey: ['gallery', storeId, tabId], queryFn: () => api.galleryItems(storeId, tabId, token) })
  const deactivate = useDelete<GalleryItemResponse>(['gallery', storeId, tabId], (id) => api.deleteGalleryItem(storeId, tabId, id, token))
  const [files, setFiles] = useState<File[]>([]); const [altText, setAltText] = useState(''); const [caption, setCaption] = useState('')
  const create = useMutation({ mutationFn: async () => { if (!files.length) throw new Error('Selecione ao menos uma imagem.'); const start = items.data?.length ?? 0; for (const [index, file] of files.entries()) { const media = await api.uploadMedia(storeId, file, token); await api.createGalleryItem(storeId, tabId, { mediaAssetId: media.id, altText: altText || file.name.replace(/\.[^.]+$/, ''), caption: caption || null, sortOrder: start + index }, token) } }, onSuccess: () => { setFiles([]); setAltText(''); setCaption(''); void items.refetch() } })
  return <ContentPanel title="Galeria" count={items.data?.length ?? 0}><form className="content-add-form" onSubmit={(event) => { event.preventDefault(); create.mutate() }}><label className="media-drop"><input type="file" accept="image/jpeg,image/png,image/webp" multiple onChange={(event) => setFiles(Array.from(event.target.files ?? []))} /><ImagePlus size={20} /><span>{files.length ? `${files.length} imagem(ns) selecionada(s)` : 'Adicionar imagens'}</span><small>O texto alternativo é obrigatório</small></label><label className="field"><span>Texto alternativo</span><input value={altText} onChange={(event) => setAltText(event.target.value)} maxLength={200} placeholder="Imagem do ambiente" /></label><label className="field"><span>Legenda opcional</span><input value={caption} onChange={(event) => setCaption(event.target.value)} maxLength={500} /></label><button className="button button-dark" type="submit" disabled={create.isPending}><Plus size={16} /> Adicionar imagens</button></form>{create.error && <div className="alert alert-error">{problemMessage(create.error)}</div>}{deactivate.error && <div className="alert alert-error">{problemMessage(deactivate.error)}</div>}<div className="content-gallery-items">{(items.data ?? []).map((item) => <ContentItem key={item.id} image={item.imageUrl} title={item.altText} detail={item.caption ?? ''} pending={deactivate.isPending} onDelete={() => window.confirm(`Excluir ${item.altText} permanentemente?`) && deactivate.mutate(item.id)} />)}{!items.data?.length && <p className="empty-state">Nenhuma imagem nesta galeria.</p>}</div></ContentPanel>
}

function ContentPanel({ title, count, children }: { title: string; count: number; children: ReactNode }) { return <section className="content-panel"><div className="panel-heading"><div><span className="eyebrow">Editor</span><h2>{title}</h2></div><span className="panel-note">{count} {count === 1 ? 'item' : 'itens'}</span></div>{children}</section> }

function ContentItem({ image, title, detail, pending, onDelete }: { image?: string; title: string; detail: string; pending?: boolean; onDelete: () => void }) { return <div className="content-item-row">{image ? <img src={mediaHref(image)} alt="" /> : <span className="content-item-mark"><Link2 size={16} /></span>}<span><strong>{title}</strong><small>{detail}</small></span><button className="button-icon" type="button" disabled={pending} onClick={onDelete} aria-label={`Excluir ${title}`}><Trash2 size={15} /></button></div> }

type Deletable = { id: string }

function useDelete<T extends Deletable>(queryKey: readonly unknown[], remove: (id: string) => Promise<void>) {
  const client = useQueryClient()
  return useMutation<void, Error, string, { previous?: T[] }>({
    mutationFn: remove,
    onMutate: async (id) => {
      await client.cancelQueries({ queryKey })
      const previous = client.getQueryData<T[]>(queryKey)
      client.setQueryData<T[]>(queryKey, (current) => current?.filter((item) => item.id !== id))
      return { previous }
    },
    onError: (_error, _id, context) => {
      if (context?.previous !== undefined) client.setQueryData(queryKey, context.previous)
    },
    onSettled: () => { void client.invalidateQueries({ queryKey }) },
  })
}

function typeLabel(type: ContentType) { return types.find((item) => item.value === type)?.label ?? 'Links' }
