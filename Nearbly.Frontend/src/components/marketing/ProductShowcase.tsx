import { ArrowUpRight, Camera, MapPin, MessageCircle, Utensils } from 'lucide-react'
import { motion, useMotionValue, useScroll, useSpring, useTransform } from 'motion/react'
import { useMemo, useRef } from 'react'
import { Stack } from '../react-bits/Stack'
import { useHydratedReducedMotion } from '../react-bits/useHydratedReducedMotion'

const LinkRow = ({ icon: Icon, children }: { icon: typeof MessageCircle; children: string }) => (
  <div className="showcase-link"><span><Icon size={15} /></span>{children}<ArrowUpRight size={14} /></div>
)

export default function ProductShowcase() {
  const stageRef = useRef<HTMLDivElement>(null)
  const reducedMotion = useHydratedReducedMotion()
  const { scrollYProgress } = useScroll({ target: stageRef, offset: ['start end', 'end start'] })
  const cardY = useTransform(scrollYProgress, [0, 1], [52, -64])
  const gridY = useTransform(scrollYProgress, [0, 1], [80, -90])
  const pointerX = useSpring(useMotionValue(0), { stiffness: 100, damping: 20 })
  const pointerY = useSpring(useMotionValue(0), { stiffness: 100, damping: 20 })
  const rotateX = useTransform(pointerY, [-0.5, 0.5], [4, -4])
  const rotateY = useTransform(pointerX, [-0.5, 0.5], [-6, 6])
  const cards = useMemo(() => [
    <div className="showcase-page" key="page">
      <div className="showcase-page-top"><div className="showcase-avatar">CC</div><strong>Café Central</strong><small>café, encontros e bons links</small></div>
      <div className="showcase-links"><LinkRow icon={Utensils}>Conheça nosso cardápio</LinkRow><LinkRow icon={Camera}>Siga no Instagram</LinkRow><LinkRow icon={MessageCircle}>Fale no WhatsApp</LinkRow><LinkRow icon={MapPin}>Como chegar</LinkRow></div>
    </div>,
    <div className="showcase-analytics" key="analytics"><span>Últimos 30 dias</span><strong>1.284</strong><small>acessos à sua página</small><div className="showcase-bars"><i /><i /><i /><i /><i /><i /><i /></div><div className="showcase-result"><b>68%</b><span>dos acessos viraram cliques</span></div></div>,
    <div className="showcase-display" key="display"><img src="/brand/logo-mark-white.svg" alt="" /><strong>Aproxime seu celular</strong><p>ou escaneie o QR Code</p><div className="showcase-qr"><span /></div><small>Powered by Nearbly</small></div>,
  ], [])

  return (
    <div
      ref={stageRef}
      className="product-stage"
      onPointerMove={event => {
        if (reducedMotion || event.pointerType === 'touch') return
        const rect = event.currentTarget.getBoundingClientRect()
        pointerX.set((event.clientX - rect.left) / rect.width - 0.5)
        pointerY.set((event.clientY - rect.top) / rect.height - 0.5)
      }}
      onPointerLeave={() => {
        pointerX.set(0)
        pointerY.set(0)
      }}
    >
      <motion.div className="stage-grid" style={reducedMotion ? undefined : { y: gridY }} aria-hidden="true" />
      <motion.div
        className="showcase-stack"
        style={reducedMotion ? undefined : {
          y: cardY,
          rotateX,
          rotateY,
        }}
      >
        <Stack cards={cards} />
      </motion.div>
      <p className="showcase-hint">Arraste para explorar</p>
    </div>
  )
}
