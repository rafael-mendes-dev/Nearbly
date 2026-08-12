declare module '@barba/core' {
  const barba: {
    init: (options: unknown) => void
    destroy: () => void
  }
  export default barba
}
