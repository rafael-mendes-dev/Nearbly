import { useEffect, useRef, type ReactNode } from 'react'
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import './react-bits.css'

gsap.registerPlugin(ScrollTrigger)

export function AnimatedList({ children, className = '' }: { children: ReactNode; className?: string }) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const element = ref.current
    if (!element || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const items = element.querySelectorAll('.flow-step, [data-list-item]')
    const animation = gsap.fromTo(
      items.length ? items : element.children,
      { y: 22 },
      {
        y: 0,
        duration: 0.48,
        stagger: 0.09,
        ease: 'power3.out',
        scrollTrigger: { trigger: element, start: 'top 86%', once: true },
      },
    )

    return () => {
      animation.scrollTrigger?.kill()
      animation.revert()
    }
  }, [])

  return <div ref={ref} className={`rb-animated-list ${className}`}>{children}</div>
}
