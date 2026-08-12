import { useRef, type ReactNode } from 'react'
import { motion, useMotionValue, useSpring } from 'motion/react'
import './react-bits.css'
import { useHydratedReducedMotion } from './useHydratedReducedMotion'

interface MagnetProps {
  children: ReactNode
  className?: string
  padding?: number
  strength?: number
}

export function Magnet({ children, className = '', padding = 70, strength = 5 }: MagnetProps) {
  const ref = useRef<HTMLDivElement>(null)
  const reducedMotion = useHydratedReducedMotion()
  const x = useSpring(useMotionValue(0), { stiffness: 240, damping: 18 })
  const y = useSpring(useMotionValue(0), { stiffness: 240, damping: 18 })

  const handleMove = (event: React.PointerEvent<HTMLDivElement>) => {
    if (reducedMotion || event.pointerType === 'touch' || !ref.current) return
    const rect = ref.current.getBoundingClientRect()
    const centerX = rect.left + rect.width / 2
    const centerY = rect.top + rect.height / 2
    if (Math.abs(event.clientX - centerX) < rect.width / 2 + padding && Math.abs(event.clientY - centerY) < rect.height / 2 + padding) {
      x.set((event.clientX - centerX) / strength)
      y.set((event.clientY - centerY) / strength)
    }
  }

  const reset = () => {
    x.set(0)
    y.set(0)
  }

  return (
    <motion.div
      ref={ref}
      className={`rb-magnet ${className}`}
      style={reducedMotion ? undefined : { x, y }}
      onPointerMove={handleMove}
      onPointerLeave={reset}
    >
      {children}
    </motion.div>
  )
}
