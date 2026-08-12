import { useEffect, useRef, type ReactNode } from 'react'
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import './react-bits.css'

gsap.registerPlugin(ScrollTrigger)

export function Stepper({ children }: { children: ReactNode }) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const element = ref.current
    if (!element || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const steps = element.querySelectorAll('.flow-step')
    const animation = gsap.fromTo(
      steps.length ? steps : element.children,
      { x: -22 },
      {
        x: 0,
        duration: 0.55,
        stagger: 0.08,
        ease: 'power3.out',
        scrollTrigger: { trigger: element, start: 'top 86%', once: true },
      },
    )

    return () => {
      animation.scrollTrigger?.kill()
      animation.revert()
    }
  }, [])

  return <div ref={ref} className="rb-stepper">{children}</div>
}
