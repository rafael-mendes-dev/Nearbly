import { useInView, useMotionValue, useSpring } from 'motion/react'
import { useEffect, useRef } from 'react'
import { useHydratedReducedMotion } from './useHydratedReducedMotion'

export function CountUp({ value, suffix = '', decimals = 0 }: { value: number; suffix?: string; decimals?: number }) {
  const ref = useRef<HTMLSpanElement>(null)
  const inView = useInView(ref, { once: true, amount: 0.5 })
  const reduceMotion = useHydratedReducedMotion()
  const motionValue = useMotionValue(0)
  const spring = useSpring(motionValue, { damping: 32, stiffness: 92 })

  useEffect(() => {
    const format = (number: number) => `${new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    }).format(number)}${suffix}`

    if (ref.current) ref.current.textContent = reduceMotion ? format(value) : format(0)
    const unsubscribe = spring.on('change', (latest) => {
      if (ref.current) ref.current.textContent = format(latest)
    })
    if (inView) motionValue.set(value)
    return unsubscribe
  }, [decimals, inView, motionValue, reduceMotion, spring, suffix, value])

  return <span ref={ref} className="rb-count-up">{value.toLocaleString('pt-BR')}{suffix}</span>
}
