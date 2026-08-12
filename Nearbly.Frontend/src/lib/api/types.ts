export type TrafficSource = 'Nfc' | 'QrCode' | 'Direct' | 'Unknown'

export interface LoginRequest { email: string; password: string }
export interface LoginResponse {
  accessToken: string
  tokenType: 'Bearer'
  expiresAtUtc: string
}

export interface StoreResponse {
  id: string
  name: string
  slug: string
  description: string | null
  logoUrl: string | null
  primaryColor: string | null
  secondaryColor: string | null
  logoMediaId: string | null
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface StoreInput {
  name: string
  slug: string
  description?: string | null
  logoUrl?: string | null
  primaryColor?: string | null
  secondaryColor?: string | null
  logoMediaId?: string | null
}

export interface StoreUpdate extends StoreInput { isActive?: boolean }

export interface TabResponse {
  id: string
  storeId: string
  key: string
  name: string
  contentType: ContentType
  sortOrder: number
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type ContentType = 'links' | 'products' | 'markdown' | 'gallery'
export interface TabInput { key: string; name: string; sortOrder?: number; contentType?: ContentType }
export interface TabUpdate extends TabInput { isActive?: boolean }

export interface AdminLinkResponse {
  id: string
  storeId: string
  storeTabId: string | null
  type: string
  label: string
  icon: string | null
  url: string
  sortOrder: number
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface LinkInput {
  type: string
  label: string
  icon?: string | null
  url: string
  sortOrder?: number
  storeTabId?: string | null
}

export interface LinkUpdate extends LinkInput { isActive?: boolean }

export interface MediaResponse { id: string; url: string; mimeType: string; sizeBytes: number; width: number; height: number; isActive: boolean; createdAtUtc: string }
export interface ProductResponse { id: string; storeId: string; storeTabId: string; name: string; description: string | null; mediaAssetId: string; imageUrl: string; price: number | null; isAvailable: boolean; sortOrder: number; isActive: boolean; createdAtUtc: string; updatedAtUtc: string }
export interface ProductInput { name: string; description?: string | null; mediaAssetId: string; price?: number | null; isAvailable?: boolean; sortOrder?: number }
export interface MarkdownBlockResponse { id: string; storeId: string; storeTabId: string; title: string | null; markdown: string; sortOrder: number; isActive: boolean; createdAtUtc: string; updatedAtUtc: string }
export interface MarkdownBlockInput { title?: string | null; markdown: string; sortOrder?: number }
export interface GalleryItemResponse { id: string; storeId: string; storeTabId: string; mediaAssetId: string; imageUrl: string; altText: string; caption: string | null; sortOrder: number; isActive: boolean; createdAtUtc: string; updatedAtUtc: string }
export interface GalleryItemInput { mediaAssetId: string; altText: string; caption?: string | null; sortOrder?: number }

export interface PublicLinkResponse { id: string; type: string; label: string; icon: string | null; href: string }
export interface PublicProductResponse { id: string; name: string; description: string | null; imageUrl: string; price: number | null; isAvailable: boolean; sortOrder: number }
export interface PublicMarkdownBlockResponse { id: string; title: string | null; markdown: string; sortOrder: number }
export interface PublicGalleryItemResponse { id: string; imageUrl: string; altText: string; caption: string | null; sortOrder: number }
export interface PublicTabResponse { id: string; key: string; name: string; contentType: ContentType; sortOrder: number; links: PublicLinkResponse[]; products: PublicProductResponse[]; markdownBlocks: PublicMarkdownBlockResponse[]; galleryItems: PublicGalleryItemResponse[] }
export interface PublicStoreResponse {
  id: string
  name: string
  slug: string
  description: string | null
  logoUrl: string | null
  logoMediaId?: string | null
  primaryColor: string | null
  secondaryColor: string | null
  links: PublicLinkResponse[]
  tabs: PublicTabResponse[]
}

export interface StoreAnalyticsResponse {
  views: number
  clicks: number
  ctr: number
  sources: Record<TrafficSource, number>
  topLinks: Array<{ linkId: string; label: string; type: string; clicks: number }>
  viewsByDay: Array<{ date: string; views: number }>
}
