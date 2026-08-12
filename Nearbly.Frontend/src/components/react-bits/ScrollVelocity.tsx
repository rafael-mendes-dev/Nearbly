import { useRef } from 'react'
import {
  motion,
  useAnimationFrame,
  useMotionValue,
  useScroll,
  useSpring,
  useTransform,
  useVelocity,
} from 'motion/react'
import './react-bits.css'
import { useHydratedReducedMotion } from './useHydratedReducedMotion'

const wrap = (min: number, max: number, value: number) => {
  const range = max - min
  return ((((value - min) % range) + range) % range) + min
}

interface VelocityLineProps {
  text: string
  baseVelocity: number
  reverse?: boolean
}

function VelocityLine({ text, baseVelocity, reverse = false }: VelocityLineProps) {
  const baseX = useMotionValue(0)
  const { scrollY } = useScroll()
  const scrollVelocity = useVelocity(scrollY)
  const smoothVelocity = useSpring(scrollVelocity, { damping: 42, stiffness: 340 })
  const velocityFactor = useTransform(smoothVelocity, [-1000, 0, 1000], [-3, 0, 3], { clamp: false })
  const x = useTransform(baseX, value => `${wrap(-25, 0, value)}%`)
  const direction = useRef(reverse ? -1 : 1)
  const reducedMotion = useHydratedReducedMotion()

  useAnimationFrame((_, delta) => {
    if (reducedMotion) return
    const velocity = velocityFactor.get()
    if (velocity < 0) direction.current = -1
    if (velocity > 0) direction.current = 1
    const move = direction.current * baseVelocity * (delta / 1000)
    baseX.set(baseX.get() + move + direction.current * move * Math.abs(velocity))
  })

  return (
    <div className="rb-velocity-line" aria-label={text}>
      <motion.div className="rb-velocity-track" style={reducedMotion ? undefined : { x }} aria-hidden="true">
        {[0, 1, 2, 3, 4].map(index => <span key={index}>{text}</span>)}
      </motion.div>
    </div>
  )
}

interface ScrollVelocityProps {
  texts: [string, string]
  velocity?: number
  className?: string
}

export function ScrollVelocity({ texts, velocity = 3.4, className = '' }: ScrollVelocityProps) {
  return (
    <div className={`rb-scroll-velocity ${className}`}>
      <VelocityLine text={texts[0]} baseVelocity={-velocity} />
      <VelocityLine text={texts[1]} baseVelocity={velocity} reverse />
    </div>
  )
}
