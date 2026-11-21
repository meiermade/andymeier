import { rocket } from './datastar-rocket.1.0.4.js';

// Shared scrolling geometry; surrounding groups and calendars retain their own semantics.
rocket('fve-scroll-region', {
  mode: 'light', scope: false,
  setup: ({ host, cleanup }) => {
    const events = new AbortController(), options = { signal: events.signal };
    host.addEventListener('focusin', event => {
      if (host.hasAttribute('follow-focus') && event.target !== host && !event.target.closest('[role=menu]')) event.target.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    }, options);
    host.addEventListener('keydown', event => {
      if (event.defaultPrevented || event.target !== host || event.altKey || event.ctrlKey || event.metaKey || event.shiftKey) return;
      const horizontal = event.key === 'ArrowLeft' || event.key === 'ArrowRight';
      if (!horizontal && (host.getAttribute('scroll-axis') !== 'both' || !['ArrowUp', 'ArrowDown'].includes(event.key))) return;
      if (horizontal ? host.scrollWidth <= host.clientWidth : host.scrollHeight <= host.clientHeight) return;
      event.preventDefault();
      const step = Number(host.getAttribute('scroll-step') || 64) * (event.key === 'ArrowLeft' || event.key === 'ArrowUp' ? -1 : 1);
      host.scrollBy({ left: horizontal ? step : 0, top: horizontal ? 0 : step });
    }, options);
    cleanup(() => events.abort());
  },
});
