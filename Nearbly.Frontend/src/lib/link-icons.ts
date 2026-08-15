import { createElement, type ReactNode } from 'react'
import { ExternalLink, Globe2, Mail, MapPin, Phone } from 'lucide-react'
import { FaLinkedinIn } from 'react-icons/fa6'
import { SiFacebook, SiGoogle, SiGooglemaps, SiInstagram, SiTiktok, SiWhatsapp, SiYoutube } from 'react-icons/si'

export const linkIconOptions = [
  { value: '', label: 'Automático' },
  { value: 'website', label: 'Site' },
  { value: 'instagram', label: 'Instagram' },
  { value: 'whatsapp', label: 'WhatsApp' },
  { value: 'facebook', label: 'Facebook' },
  { value: 'google', label: 'Google' },
  { value: 'maps', label: 'Google Maps' },
  { value: 'youtube', label: 'YouTube' },
  { value: 'tiktok', label: 'TikTok' },
  { value: 'linkedin', label: 'LinkedIn' },
  { value: 'email', label: 'E-mail' },
  { value: 'phone', label: 'Telefone' },
  { value: 'location', label: 'Localização' },
  { value: 'external', label: 'Link externo' },
] as const

export const linkIcon = (value: string | null | undefined, size = 20): ReactNode => {
  switch (value?.trim().toLowerCase()) {
    case 'instagram':
    case 'camera':
      return createElement(SiInstagram, { size })
    case 'whatsapp':
      return createElement(SiWhatsapp, { size })
    case 'facebook':
      return createElement(SiFacebook, { size })
    case 'google':
      return createElement(SiGoogle, { size })
    case 'maps':
    case 'map':
    case 'googlemaps':
    case 'google_maps':
    case 'google-maps':
    case 'location':
      return createElement(SiGooglemaps, { size })
    case 'youtube':
      return createElement(SiYoutube, { size })
    case 'tiktok':
      return createElement(SiTiktok, { size })
    case 'linkedin':
      return createElement(FaLinkedinIn, { size })
    case 'email':
    case 'mail':
      return createElement(Mail, { size, strokeWidth: 1.8 })
    case 'phone':
      return createElement(Phone, { size, strokeWidth: 1.8 })
    case 'website':
    case 'globe':
      return createElement(Globe2, { size, strokeWidth: 1.8 })
    case 'marker':
      return createElement(MapPin, { size, strokeWidth: 1.8 })
    default:
      return createElement(ExternalLink, { size, strokeWidth: 1.8 })
  }
}
