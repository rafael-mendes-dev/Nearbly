import { useEffect } from "react";

type BarbaPage = { container: HTMLElement; html?: string };

declare global {
  interface Window {
    __nearblyBarbaActive?: boolean;
  }
}

export default function BarbaTransitions() {
  useEffect(() => {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    if (window.__nearblyBarbaActive) return;
    window.__nearblyBarbaActive = true;
    let barba:
      { init: (options: unknown) => void; destroy: () => void } | undefined;
    let disposed = false;
    const curtain = document.createElement("div");
    curtain.id = "nearbly-page-transition";
    curtain.className = "page-transition";
    curtain.setAttribute("aria-hidden", "true");
    curtain.innerHTML = '<img src="/brand/logo-mark-white.svg" alt=""><span></span>';
    document.body.appendChild(curtain);
    let curtainAnimation: Animation | undefined;
    const mobile = window.matchMedia("(max-width: 700px)").matches;
    const leaveDuration = mobile ? 280 : 480;
    const enterDuration = mobile ? 380 : 620;

    const updateDocumentHead = (html?: string) => {
      if (!html) return;
      const nextDocument = new DOMParser().parseFromString(html, "text/html");
      document.title = nextDocument.title;
      ["description", "theme-color"].forEach((name) => {
        const current = document.head.querySelector<HTMLMetaElement>(`meta[name="${name}"]`);
        const next = nextDocument.head.querySelector<HTMLMetaElement>(`meta[name="${name}"]`);
        if (current && next) current.content = next.content;
      });
      const currentCanonical = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
      const nextCanonical = nextDocument.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
      if (currentCanonical && nextCanonical) currentCanonical.href = nextCanonical.href;
    };

    void import("@barba/core").then(({ default: controller }) => {
      if (disposed) return;
      barba = controller;
      barba.init({
        transitions: [
          {
            name: "nearbly-signal-wipe",
            leave: async ({ current }: { current: BarbaPage }) => {
              document.querySelectorAll<HTMLDetailsElement>(".mobile-menu[open]").forEach(menu => menu.open = false);
              curtainAnimation?.cancel();
              curtain.style.transform = "translateY(100%)";
              curtainAnimation = curtain.animate(
                [{ transform: "translateY(100%)" }, { transform: "translateY(0)" }],
                { duration: leaveDuration, easing: "cubic-bezier(.7,0,.2,1)", fill: "forwards" },
              );
              await Promise.all([
                curtainAnimation.finished,
                current.container.animate([{ opacity: 1 }, { opacity: 0.35 }], {
                  duration: mobile ? 220 : 380,
                  easing: "ease-out",
                  fill: "forwards",
                }).finished,
              ]);
            },
            enter: async ({ next }: { next: BarbaPage }) => {
              updateDocumentHead(next.html);
              next.container.style.opacity = "1";
              curtain.style.transform = "translateY(0)";
              curtainAnimation?.cancel();
              curtainAnimation = curtain.animate(
                [{ transform: "translateY(0)" }, { transform: "translateY(-100%)" }],
                { duration: enterDuration, easing: "cubic-bezier(.75,0,.15,1)", fill: "forwards" },
              );
              await curtainAnimation.finished;
              curtainAnimation.cancel();
              curtain.style.transform = "translateY(100%)";
            },
          },
        ],
        views: [
          {
            namespace: "marketing",
            beforeEnter: ({ next }: { next: BarbaPage }) => {
              updateDocumentHead(next.html);
              window.scrollTo({ top: 0, behavior: "auto" });
              document
                .querySelector<HTMLElement>("h1")
                ?.focus({ preventScroll: true });
            },
          },
        ],
      });
    });
    return () => {
      disposed = true;
      curtainAnimation?.cancel();
      barba?.destroy();
      curtain.remove();
      window.__nearblyBarbaActive = false;
    };
  }, []);

  return null;
}
