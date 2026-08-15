import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { StoreQrCodeButton } from './StoreQrCodeDialog'

vi.mock('qrcode', () => ({ toDataURL: vi.fn().mockResolvedValue('data:image/png;base64,qr-code') }))

describe('StoreQrCodeButton', () => {
  it('shows a downloadable PNG for the store public page', async () => {
    const user = userEvent.setup()
    render(<StoreQrCodeButton storeName="Café Central" publicCode="s_1a2b3c4d5e6f78901234567890abcdef" />)

    await user.click(screen.getByRole('button', { name: 'Gerar QR Code' }))

    expect(await screen.findByRole('img', { name: 'QR Code para a página de Café Central' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Testar página' })).toHaveAttribute('href', 'http://localhost:3000/s_1a2b3c4d5e6f78901234567890abcdef?src=qr_code')
    expect(screen.getByRole('link', { name: 'Baixar PNG' })).toHaveAttribute('download', 'qr-code-s_1a2b3c4d5e6f78901234567890abcdef.png')
  })
})
