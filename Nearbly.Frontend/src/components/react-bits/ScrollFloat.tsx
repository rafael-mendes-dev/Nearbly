import { useEffect, useRef, type ElementType } from 'react'
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import './react-bits.css'

gsap.registerPlugin(ScrollTrigger)

interface ScrollFloatProps {
  text: string
  className?: string
  as?: ElementType
}

export function ScrollFloat({ text, className = '', as: Tag = 'h2' }: ScrollFloatProps) {
  const ref = useRef<HTMLElement>(null)
  const words = text.split(' ')

  useEffect(() => {
    const element = ref.current
    if (!element || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return

    const characters = element.querySelectorAll<HTMLElement>('[data-rb-char]')
    const animation = gsap.fromTo(
      characters,
      { y: 12, scale: 0.985, rotateX: -12, transformOrigin: '50% 100%' },
      {
        y: 0,
        scale: 1,
        rotateX: 0,
        stagger: 0.018,
        ease: 'power3.out',
        scrollTrigger: {
          trigger: element,
          start: 'top 86%',
          end: 'top 38%',
          scrub: 1,
        },
      },
    )

    return () => {
      animation.scrollTrigger?.kill()
      animation.kill()
    }
  }, [text])

  return (
    <Tag ref={ref} className={`rb-scroll-float ${className}`} aria-label={text}>
      {words.map((word, wordIndex) => (
        <span className="rb-float-word" aria-hidden="true" key={`${word}-${wordIndex}`}>
          {[...word].map((character, characterIndex) => (
            <span data-rb-char className="rb-float-char" key={`${character}-${characterIndex}`}>
              {character}
            </span>
          ))}
          {wordIndex < words.length - 1 && <span>&nbsp;</span>}
        </span>
      ))}
    </Tag>
  )
}
