export const formatNumber = (value: number) => new Intl.NumberFormat('pt-BR').format(value)
export const formatPercent = (value: number) => `${new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 }).format(value)}%`
export const formatDate = (value: string) => new Intl.DateTimeFormat('pt-BR', { dateStyle: 'medium' }).format(new Date(value))

export const isSafeColor = (value: string | null | undefined): value is `#${string}` => /^#[0-9a-f]{6}$/i.test(value ?? '')

export const contrastText = (value: string | null | undefined) => {
  if (!isSafeColor(value)) return '#10211c'
  const [r, g, b] = [1, 3, 5].map((index) => Number.parseInt(value.slice(index, index + 2), 16) / 255)
    .map((channel) => channel <= 0.03928 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4)
  return (0.2126 * r + 0.7152 * g + 0.0722 * b) > 0.42 ? '#10211c' : '#ffffff'
}

const channelHex = (channel: number) => Math.round(Math.min(1, Math.max(0, channel)) * 255).toString(16).padStart(2, '0')

/** Keeps the hue of a brand color but forces the lightness, so any secondary color can tint a dark surface without losing contrast. */
export const shade = (value: string | null | undefined, lightness: number, saturationCap = 1) => {
  const [red, green, blue] = isSafeColor(value)
    ? [1, 3, 5].map((index) => Number.parseInt(value.slice(index, index + 2), 16) / 255)
    : [0, 0, 0]
  const max = Math.max(red, green, blue)
  const delta = max - Math.min(red, green, blue)
  const source = (max + Math.min(red, green, blue)) / 2
  const saturation = Math.min(delta === 0 ? 0 : delta / (1 - Math.abs(2 * source - 1)), saturationCap)
  const hue = delta === 0 ? 0 : 60 * (max === red ? ((((green - blue) / delta) % 6) + 6) % 6 : max === green ? (blue - red) / delta + 2 : (red - green) / delta + 4)
  const chroma = (1 - Math.abs(2 * lightness - 1)) * saturation
  const middle = chroma * (1 - Math.abs(((hue / 60) % 2) - 1))
  const offset = lightness - chroma / 2
  const sectors = [[chroma, middle, 0], [middle, chroma, 0], [0, chroma, middle], [0, middle, chroma], [middle, 0, chroma], [chroma, 0, middle]]
  const [shadeRed, shadeGreen, shadeBlue] = sectors[Math.floor(hue / 60) % 6]
  return `#${channelHex(shadeRed + offset)}${channelHex(shadeGreen + offset)}${channelHex(shadeBlue + offset)}`
}

export const fillDateGaps = (series: Array<{ date: string; views: number }>, from?: string, to?: string) => {
  if (!from || !to) return series
  const values = new Map(series.map((item) => [item.date, item.views]))
  const result: Array<{ date: string; views: number }> = []
  for (let date = new Date(`${from}T00:00:00Z`); date <= new Date(`${to}T00:00:00Z`); date.setUTCDate(date.getUTCDate() + 1)) {
    const key = date.toISOString().slice(0, 10)
    result.push({ date: key, views: values.get(key) ?? 0 })
  }
  return result
}
