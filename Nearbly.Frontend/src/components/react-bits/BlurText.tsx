import { gsap } from 'gsap'
import { useEffect, useRef } from 'react'

interface BlurTextProps {
  text: string
  className?: string
  delay?: number
  animateBy?: 'words' | 'letters'
}

export function BlurText({ text, className = '', delay = 70, animateBy = 'words' }: BlurTextProps) {
  const ref = useRef<HTMLSpanElement>(null)
  const parts = animateBy === 'words' ? text.split(' ') : text.split('')

  useEffect(() => {
    const element = ref.current
    if (!element || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return

    const animation = gsap.fromTo(
      element.children,
      { filter: 'blur(12px)', y: 16 },
      {
        filter: 'blur(0px)',
        y: 0,
        duration: 0.55,
        stagger: delay / 1000,
        ease: 'power3.out',
      },
    )

    return () => {
      animation.revert()
    }
  }, [delay, text])

  return (
    <span ref={ref} className={`rb-blur-text ${className}`} aria-label={text}>
      {parts.map((part, index) => (
        <span aria-hidden="true" key={`${part}-${index}`}>
          {part}{animateBy === 'words' && index < parts.length - 1 ? '\u00a0' : ''}
        </span>
      ))}
    </span>
  )
}
