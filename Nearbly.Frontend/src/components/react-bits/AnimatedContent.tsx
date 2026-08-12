import { useEffect, useRef, type HTMLAttributes, type ReactNode } from 'react'
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import './react-bits.css'

gsap.registerPlugin(ScrollTrigger)

interface AnimatedContentProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  distance?: number
  direction?: 'vertical' | 'horizontal'
  reverse?: boolean
  duration?: number
  delay?: number
  threshold?: number
}

export function AnimatedContent({
  children,
  className = '',
  distance = 34,
  direction = 'vertical',
  reverse = false,
  duration = 0.72,
  delay = 0,
  threshold = 0.12,
  ...props
}: AnimatedContentProps) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const element = ref.current
    if (!element || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return

    const axis = direction === 'horizontal' ? 'x' : 'y'
    const offset = (reverse ? -1 : 1) * distance
    const animation = gsap.fromTo(
      element,
      { [axis]: offset },
      {
        [axis]: 0,
        duration,
        delay,
        ease: 'power3.out',
        scrollTrigger: {
          trigger: element,
          start: `top ${(1 - threshold) * 100}%`,
          once: true,
        },
      },
    )

    return () => {
      animation.scrollTrigger?.kill()
      animation.kill()
    }
  }, [delay, direction, distance, duration, reverse, threshold])

  return <div ref={ref} className={`rb-animated-content ${className}`} {...props}>{children}</div>
}
