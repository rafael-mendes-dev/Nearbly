import { useEffect, useState, type ReactNode } from 'react'
import { motion, useMotionValue, useTransform, type PanInfo } from 'motion/react'
import './react-bits.css'
import { useHydratedReducedMotion } from './useHydratedReducedMotion'

function DraggableCard({ children, onSendToBack, disabled }: { children: ReactNode; onSendToBack: () => void; disabled: boolean }) {
  const x = useMotionValue(0)
  const y = useMotionValue(0)
  const rotateX = useTransform(y, [-100, 100], [9, -9])
  const rotateY = useTransform(x, [-100, 100], [-9, 9])
  const dragEnd = (_event: MouseEvent | TouchEvent | PointerEvent, info: PanInfo) => {
    if (Math.abs(info.offset.x) > 65 || Math.abs(info.offset.y) > 65) onSendToBack()
    else { x.set(0); y.set(0) }
  }

  return (
    <motion.div className="rb-stack-drag" style={{ x, y, rotateX, rotateY }} drag={!disabled} dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }} dragElastic={0.35} onDragEnd={dragEnd} onClick={disabled ? onSendToBack : undefined}>
      {children}
    </motion.div>
  )
}

export function Stack({ cards, className = '', autoplay = true, autoplayDelay = 4200 }: { cards: ReactNode[]; className?: string; autoplay?: boolean; autoplayDelay?: number }) {
  const reduceMotion = useHydratedReducedMotion()
  const [stack, setStack] = useState(() => cards.map((content, id) => ({ id, content })))
  const sendToBack = (id: number) => setStack((current) => {
    const next = [...current]
    const index = next.findIndex((card) => card.id === id)
    if (index < 0) return current
    const [card] = next.splice(index, 1)
    next.unshift(card)
    return next
  })

  useEffect(() => {
    if (!autoplay || reduceMotion || stack.length < 2) return
    const timer = window.setInterval(() => sendToBack(stack[stack.length - 1].id), autoplayDelay)
    return () => window.clearInterval(timer)
  }, [autoplay, autoplayDelay, reduceMotion, stack])

  return (
    <div className={`rb-stack ${className}`}>
      {stack.map((card, index) => (
        <DraggableCard key={card.id} onSendToBack={() => sendToBack(card.id)} disabled={Boolean(reduceMotion)}>
          <motion.div className="rb-stack-card" animate={{ rotateZ: (stack.length - index - 1) * 2.4, scale: 1 + index * .045 - stack.length * .045 }} transition={{ type: 'spring', stiffness: 250, damping: 24 }}>
            {card.content}
          </motion.div>
        </DraggableCard>
      ))}
    </div>
  )
}
