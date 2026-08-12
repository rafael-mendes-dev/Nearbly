import { motion, useScroll, useTransform } from 'motion/react'
import { useRef } from 'react'
import { useHydratedReducedMotion } from '../react-bits/useHydratedReducedMotion'

interface ParallaxMediaProps {
  src: string
  srcSet?: string
  sizes?: string
  alt: string
  className?: string
  eager?: boolean
}

export default function ParallaxMedia({
  src,
  srcSet,
  sizes = '100vw',
  alt,
  className = '',
  eager = false,
}: ParallaxMediaProps) {
  const ref = useRef<HTMLDivElement>(null)
  const reducedMotion = useHydratedReducedMotion()
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start end', 'end start'] })
  const y = useTransform(scrollYProgress, [0, 1], ['-4%', '4%'])
  const scale = useTransform(scrollYProgress, [0, 1], [1.08, 1.03])

  return (
    <motion.div ref={ref} className={`parallax-media ${className}`} style={reducedMotion ? undefined : { y, scale }}>
      <img
        src={src}
        srcSet={srcSet}
        sizes={sizes}
        alt={alt}
        loading={eager ? 'eager' : 'lazy'}
        fetchPriority={eager ? 'high' : 'auto'}
      />
    </motion.div>
  )
}
