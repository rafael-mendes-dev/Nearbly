import type { ApiProblem } from './problem'
import { ApiError, isApiProblem } from './problem'
import { clearSession } from '../auth/session'
import type {
  AdminLinkResponse, GalleryItemInput, GalleryItemResponse, LinkInput, LinkUpdate, LoginRequest, LoginResponse, MarkdownBlockInput, MarkdownBlockResponse, MediaResponse, ProductInput, ProductResponse, PublicStoreResponse,
  StoreAnalyticsResponse, StoreInput, StoreResponse, StoreUpdate, TabInput, TabResponse, TabUpdate,
} from './types'

const runtimeApiBase = typeof window !== 'undefined'
  ? (import.meta.env.PUBLIC_API_BASE_URL ?? '')
  : (import.meta.env.API_BASE_URL ?? import.meta.env.PUBLIC_API_BASE_URL ?? 'http://localhost:5112')

export const API_BASE_URL = runtimeApiBase.replace(/\/$/, '')

const readProblem = async (response: Response): Promise<ApiProblem> => {
  let body: unknown
  try { body = await response.json() } catch { body = undefined }
  if (isApiProblem(body)) return body
  return {
    title: response.statusText || 'Request failed',
    status: response.status,
    detail: 'O servidor não conseguiu concluir esta operação.',
  }
}

const request = async <T>(path: string, init: RequestInit = {}, token?: string): Promise<T> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init.body && !(init.body instanceof FormData) ? { 'Content-Type': 'application/json' } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  })
  if (!response.ok) {
    if (response.status === 401 && typeof window !== 'undefined') {
      clearSession()
      window.dispatchEvent(new Event('nearbly:session-expired'))
    }
    throw new ApiError(await readProblem(response))
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

const json = (body: unknown): RequestInit => ({ method: 'POST', body: JSON.stringify(body) })
const putJson = (body: unknown): RequestInit => ({ method: 'PUT', body: JSON.stringify(body) })

export const api = {
  login: (input: LoginRequest) => request<LoginResponse>('/api/admin/auth/login', json(input)),
  publicStore: (slug: string) => request<PublicStoreResponse>(`/api/public/stores/${encodeURIComponent(slug)}`),
  registerView: (slug: string, source?: string) => request<void>(`/api/public/stores/${encodeURIComponent(slug)}/views`, {
    ...json(source ? { source: source === 'nfc' ? 'Nfc' : source === 'qr_code' ? 'QrCode' : source === 'unknown' ? 'Unknown' : 'Direct' } : {}),
  }),
  stores: (token: string) => request<StoreResponse[]>('/api/admin/stores', {}, token),
  store: (id: string, token: string) => request<StoreResponse>(`/api/admin/stores/${id}`, {}, token),
  createStore: (input: StoreInput, token: string) => request<StoreResponse>('/api/admin/stores', { ...json(input) }, token),
  updateStore: (id: string, input: StoreUpdate, token: string) => request<StoreResponse>(`/api/admin/stores/${id}`, { ...putJson(input) }, token),
  deactivateStore: (id: string, token: string) => request<void>(`/api/admin/stores/${id}`, { method: 'DELETE' }, token),
  tabs: (storeId: string, token: string) => request<TabResponse[]>(`/api/admin/stores/${storeId}/tabs`, {}, token),
  createTab: (storeId: string, input: TabInput, token: string) => request<TabResponse>(`/api/admin/stores/${storeId}/tabs`, { ...json(input) }, token),
  updateTab: (storeId: string, id: string, input: TabUpdate, token: string) => request<TabResponse>(`/api/admin/stores/${storeId}/tabs/${id}`, { ...putJson(input) }, token),
  deactivateTab: (storeId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/tabs/${id}`, { method: 'DELETE' }, token),
  links: (storeId: string, token: string) => request<AdminLinkResponse[]>(`/api/admin/stores/${storeId}/links`, {}, token),
  createLink: (storeId: string, input: LinkInput, token: string) => request<AdminLinkResponse>(`/api/admin/stores/${storeId}/links`, { ...json(input) }, token),
  updateLink: (storeId: string, id: string, input: LinkUpdate, token: string) => request<AdminLinkResponse>(`/api/admin/stores/${storeId}/links/${id}`, { ...putJson(input) }, token),
  deactivateLink: (storeId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/links/${id}`, { method: 'DELETE' }, token),
  uploadMedia: (storeId: string, file: File, token: string) => { const body = new FormData(); body.append('file', file); return request<MediaResponse>(`/api/admin/stores/${storeId}/media`, { method: 'POST', body }, token) },
  deactivateMedia: (storeId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/media/${id}`, { method: 'DELETE' }, token),
  products: (storeId: string, tabId: string, token: string) => request<ProductResponse[]>(`/api/admin/stores/${storeId}/tabs/${tabId}/products`, {}, token),
  createProduct: (storeId: string, tabId: string, input: ProductInput, token: string) => request<ProductResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/products`, { ...json(input) }, token),
  updateProduct: (storeId: string, tabId: string, id: string, input: ProductInput & { isActive?: boolean }, token: string) => request<ProductResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/products/${id}`, { ...putJson(input) }, token),
  deactivateProduct: (storeId: string, tabId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/tabs/${tabId}/products/${id}`, { method: 'DELETE' }, token),
  markdownBlocks: (storeId: string, tabId: string, token: string) => request<MarkdownBlockResponse[]>(`/api/admin/stores/${storeId}/tabs/${tabId}/markdown-blocks`, {}, token),
  createMarkdownBlock: (storeId: string, tabId: string, input: MarkdownBlockInput, token: string) => request<MarkdownBlockResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/markdown-blocks`, { ...json(input) }, token),
  updateMarkdownBlock: (storeId: string, tabId: string, id: string, input: MarkdownBlockInput & { isActive?: boolean }, token: string) => request<MarkdownBlockResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/markdown-blocks/${id}`, { ...putJson(input) }, token),
  deactivateMarkdownBlock: (storeId: string, tabId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/tabs/${tabId}/markdown-blocks/${id}`, { method: 'DELETE' }, token),
  galleryItems: (storeId: string, tabId: string, token: string) => request<GalleryItemResponse[]>(`/api/admin/stores/${storeId}/tabs/${tabId}/gallery-items`, {}, token),
  createGalleryItem: (storeId: string, tabId: string, input: GalleryItemInput, token: string) => request<GalleryItemResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/gallery-items`, { ...json(input) }, token),
  updateGalleryItem: (storeId: string, tabId: string, id: string, input: GalleryItemInput & { isActive?: boolean }, token: string) => request<GalleryItemResponse>(`/api/admin/stores/${storeId}/tabs/${tabId}/gallery-items/${id}`, { ...putJson(input) }, token),
  deactivateGalleryItem: (storeId: string, tabId: string, id: string, token: string) => request<void>(`/api/admin/stores/${storeId}/tabs/${tabId}/gallery-items/${id}`, { method: 'DELETE' }, token),
  analytics: (storeId: string, token: string, from?: string, to?: string) => {
    const query = new URLSearchParams()
    if (from) query.set('from', from)
    if (to) query.set('to', to)
    return request<StoreAnalyticsResponse>(`/api/admin/stores/${storeId}/analytics${query.size ? `?${query}` : ''}`, {}, token)
  },
}
