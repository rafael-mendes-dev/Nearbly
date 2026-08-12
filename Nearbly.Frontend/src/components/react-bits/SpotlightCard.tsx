import { useRef, type MouseEventHandler, type ReactNode } from 'react'
import './react-bits.css'

export function SpotlightCard({ children, className = '', spotlightColor = 'rgba(19, 187, 239, 0.16)' }: { children: ReactNode; className?: string; spotlightColor?: string }) {
  const ref = useRef<HTMLElement>(null)
  const handleMouseMove: MouseEventHandler<HTMLElement> = (event) => {
    const element = ref.current
    if (!element) return
    const rect = element.getBoundingClientRect()
    element.style.setProperty('--mouse-x', `${event.clientX - rect.left}px`)
    element.style.setProperty('--mouse-y', `${event.clientY - rect.top}px`)
    element.style.setProperty('--spotlight-color', spotlightColor)
  }

  return <article ref={ref} onMouseMove={handleMouseMove} className={`rb-spotlight ${className}`}>{children}</article>
}
