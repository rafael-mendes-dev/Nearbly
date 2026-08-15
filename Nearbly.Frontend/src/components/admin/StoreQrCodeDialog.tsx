import { useEffect, useMemo, useState } from 'react'
import { Download, ExternalLink, LoaderCircle, QrCode, X } from 'lucide-react'
import { toDataURL } from 'qrcode'

const storeQrCodeUrl = (publicCode: string, origin: string) => {
  const url = new URL(`/${publicCode}`, origin)
  url.searchParams.set('src', 'qr_code')
  return url.toString()
}

export function StoreQrCodeButton({ storeName, publicCode }: { storeName: string; publicCode: string }) {
  const [open, setOpen] = useState(false)
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [error, setError] = useState(false)
  const pageUrl = useMemo(() => storeQrCodeUrl(publicCode, window.location.origin), [publicCode])

  useEffect(() => {
    if (!open) return

    let cancelled = false
    void toDataURL(pageUrl, {
      errorCorrectionLevel: 'M',
      margin: 2,
      width: 1200,
      color: { dark: '#06080FFF', light: '#FFFFFFFF' },
    }).then(
      (dataUrl) => { if (!cancelled) setImageUrl(dataUrl) },
      () => { if (!cancelled) setError(true) },
    )

    return () => { cancelled = true }
  }, [open, pageUrl])

  useEffect(() => {
    if (!open) return
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === 'Escape') setOpen(false) }
    window.addEventListener('keydown', closeOnEscape)
    return () => window.removeEventListener('keydown', closeOnEscape)
  }, [open])

  return <>
    <button className="button button-quiet" type="button" onClick={() => { setImageUrl(null); setError(false); setOpen(true) }}>
      <QrCode size={17} /> Gerar QR Code
    </button>
    {open && <div className="qr-code-backdrop" role="presentation" onMouseDown={() => setOpen(false)}>
      <section className="qr-code-dialog" role="dialog" aria-modal="true" aria-labelledby="qr-code-title" onMouseDown={(event) => event.stopPropagation()}>
        <div className="qr-code-dialog-heading">
          <div><span className="eyebrow">Página pública</span><h2 id="qr-code-title">QR Code de {storeName}</h2></div>
          <button className="button-icon" type="button" onClick={() => setOpen(false)} aria-label="Fechar QR Code"><X size={18} /></button>
        </div>
        <div className="qr-code-image">
          {imageUrl && <img src={imageUrl} alt={`QR Code para a página de ${storeName}`} />}
          {!imageUrl && !error && <LoaderCircle className="qr-code-loading" size={28} aria-label="Gerando QR Code" />}
          {error && <p className="field-error">Não foi possível gerar o QR Code.</p>}
        </div>
        <code className="qr-code-url">{pageUrl}</code>
        <div className="qr-code-actions">
          <a className="button button-quiet" href={pageUrl} target="_blank" rel="noreferrer"><ExternalLink size={17} /> Testar página</a>
          {imageUrl && <a className="button button-dark" href={imageUrl} download={`qr-code-${publicCode}.png`}><Download size={17} /> Baixar PNG</a>}
        </div>
      </section>
    </div>}
  </>
}
