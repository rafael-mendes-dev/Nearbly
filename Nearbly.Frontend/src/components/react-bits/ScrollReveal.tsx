import { useEffect, useMemo, useRef, type ReactNode } from 'react'
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import './react-bits.css'

gsap.registerPlugin(ScrollTrigger)

export function ScrollReveal({ children, className = '' }: { children: ReactNode; className?: string }) {
  const ref = useRef<HTMLDivElement>(null)
  const content = useMemo(() => typeof children === 'string'
    ? children.split(/(\s+)/).map((word, index) => /^\s+$/.test(word) ? word : <span className="rb-word" key={index}>{word}</span>)
    : children, [children])

  useEffect(() => {
    if (!ref.current || typeof children !== 'string' || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const words = ref.current.querySelectorAll('.rb-word')
    const animation = gsap.fromTo(words, { opacity: 0.18, filter: 'blur(7px)', y: 8 }, {
      opacity: 1,
      filter: 'blur(0px)',
      y: 0,
      stagger: 0.04,
      scrollTrigger: { trigger: ref.current, start: 'top 82%', end: 'bottom 58%', scrub: true },
    })
    return () => {
      animation.scrollTrigger?.kill()
      animation.kill()
    }
  }, [children])

  return <div ref={ref} className={`rb-scroll-reveal ${className}`}>{content}</div>
}
