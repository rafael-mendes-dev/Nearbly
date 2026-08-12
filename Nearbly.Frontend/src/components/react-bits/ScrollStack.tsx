import { useRef } from 'react'
import { motion, useScroll, useTransform, type MotionValue } from 'motion/react'
import { SpotlightCard } from './SpotlightCard'
import './react-bits.css'
import { useHydratedReducedMotion } from './useHydratedReducedMotion'

export interface ScrollStackItem {
  index: string
  label: string
  title: string
  description: string
  detail: string
}

interface StackPanelProps {
  item: ScrollStackItem
  position: number
  total: number
  progress: MotionValue<number>
}

function StackPanel({ item, position, total, progress }: StackPanelProps) {
  const entryStart = position === 0 ? 0 : position / total - 0.16
  const entryEnd = position === 0 ? 0.01 : position / total
  const exitStart = Math.min((position + 1) / total, 0.98)
  const exitEnd = Math.min(exitStart + 0.15, 1)
  const y = useTransform(progress, [entryStart, entryEnd], [position === 0 ? '0%' : '105%', '0%'])
  const scale = useTransform(progress, [exitStart, exitEnd], [1, 0.92])
  const opacity = useTransform(progress, [exitStart, exitEnd], [1, 0.48])
  const reducedMotion = useHydratedReducedMotion()

  return (
    <motion.article
      className="rb-scroll-stack-panel"
      style={reducedMotion ? { zIndex: position + 1 } : { y, scale, opacity, zIndex: position + 1 }}
    >
      <SpotlightCard className="rb-scroll-stack-card">
        <div className="rb-stack-panel-top">
          <span>{item.index}</span>
          <span>{item.label}</span>
        </div>
        <div className="rb-stack-panel-copy">
          <h3>{item.title}</h3>
          <p>{item.description}</p>
        </div>
        <p className="rb-stack-panel-detail">{item.detail}</p>
      </SpotlightCard>
    </motion.article>
  )
}

interface ScrollStackProps {
  items: ScrollStackItem[]
  className?: string
}

export function ScrollStack({ items, className = '' }: ScrollStackProps) {
  const ref = useRef<HTMLDivElement>(null)
  const reducedMotion = useHydratedReducedMotion()
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start start', 'end end'] })

  return (
    <div
      ref={ref}
      className={`rb-scroll-stack ${reducedMotion ? 'is-static' : ''} ${className}`}
      style={{ '--stack-count': items.length } as React.CSSProperties}
    >
      <div className="rb-scroll-stack-sticky">
        {items.map((item, position) => (
          <StackPanel key={item.index} item={item} position={position} total={items.length} progress={scrollYProgress} />
        ))}
      </div>
    </div>
  )
}
