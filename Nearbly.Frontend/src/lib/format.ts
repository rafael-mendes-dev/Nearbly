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
