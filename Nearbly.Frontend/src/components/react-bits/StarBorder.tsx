import type { ComponentPropsWithoutRef, ElementType, ReactNode } from 'react'
import './react-bits.css'

type StarBorderProps<T extends ElementType> = ComponentPropsWithoutRef<T> & {
  as?: T
  children: ReactNode
  color?: string
  speed?: string
}

export function StarBorder<T extends ElementType = 'a'>({ as, children, className = '', color = '#13BBEF', speed = '5s', ...props }: StarBorderProps<T>) {
  const Component = as ?? 'a'
  return (
    <Component className={`rb-star-border ${className}`} {...props}>
      <span className="rb-star rb-star-top" style={{ background: `radial-gradient(circle, ${color}, transparent 62%)`, animationDuration: speed }} />
      <span className="rb-star rb-star-bottom" style={{ background: `radial-gradient(circle, ${color}, transparent 62%)`, animationDuration: speed }} />
      <span className="rb-star-content">{children}</span>
    </Component>
  )
}
