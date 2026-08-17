import { defineConfig } from 'astro/config'
import cloudflare from '@astrojs/cloudflare'
import react from '@astrojs/react'

export default defineConfig({
  output: 'server',
  // The app doesn't use astro:assets <Image> or Astro.session, so skip the Cloudflare
  // Images and KV bindings the adapter otherwise wires up by default for those features.
  session: false,
  adapter: cloudflare({ imageService: 'passthrough' }),
  integrations: [react()],
  vite: {
    ssr: {
      noExternal: ['react-router-dom', 'react-router'],
    },
    define: {
      'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV ?? 'development'),
    },
  },
})
