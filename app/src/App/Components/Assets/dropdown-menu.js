import { rocket } from './datastar-rocket.1.0.4.js';

rocket('fve-dropdown-menu', {
  mode: 'light', scope: false,
  setup: ({ host, $$, cleanup }) => {
    const events = new AbortController(), options = { signal: events.signal };
    const menu = () => [...host.querySelectorAll('[role=menu]')].find(el => el.closest('fve-dropdown-menu') === host);
    const trigger = () => host.ownerDocument.getElementById(menu()?.getAttribute('aria-labelledby'));
    const items = () => [...menu().querySelectorAll(':is([role=menuitem],[role=menuitemradio],[role=menuitemcheckbox]):not([aria-disabled=true])')].filter(item => item.getClientRects().length);
    const unavailable = () => !trigger() || trigger().disabled || trigger().getAttribute('aria-disabled') === 'true';
    let pointerX = NaN, pointerY = NaN, position;
    $$('open', false); $$('typeahead', ''); $$('typeaheadTime', 0);
    const clearPosition = () => {
      if (!position) return;
      position = undefined;
      const panel = menu();
      if (panel) { panel.style.setProperty('position-area', panel.dataset.fvePositionArea); panel.style.removeProperty('left'); panel.style.removeProperty('top'); }
    };
    const preparePosition = () => {
      if (!trigger()?.closest('[data-fve-sticky-cell=true]')) return;
      menu().style.removeProperty('position-area');
      position = () => {
        const panel = menu(), bounds = trigger()?.getBoundingClientRect();
        if (!panel?.isConnected || !bounds) return;
        const width = panel.offsetWidth, height = panel.offsetHeight, padding = 16, gap = parseFloat(getComputedStyle(panel).marginTop);
        const preferredLeft = host.getAttribute('alignment') === 'start' ? bounds.left : bounds.right - width;
        const left = Math.min(Math.max(padding, preferredLeft), window.innerWidth - width - padding);
        const below = bounds.bottom + gap, preferredTop = below + height + padding <= window.innerHeight ? below : bounds.top - height - gap;
        const top = Math.min(Math.max(padding, preferredTop), window.innerHeight - height - padding);
        panel.style.left = `${left}px`; panel.style.top = `${top - gap}px`;
      };
      position();
    };
    const hide = (restore = false) => {
      if (menu()?.matches(':popover-open')) menu().hidePopover();
      clearPosition(); $$.typeahead = '';
      if (restore) trigger()?.focus();
    };
    const show = focus => {
      if (unavailable()) return;
      if (!menu().matches(':popover-open')) { menu().showPopover({ source: trigger() }); preparePosition(); }
      if (focus === 'first') items()[0]?.focus();
      else if (focus === 'last') items().at(-1)?.focus();
      else menu().focus({ preventScroll: true });
    };
    // This guard persists independently of caller __once or callback bindings.
    host.addEventListener('click', event => {
      if (trigger()?.contains(event.target) && unavailable()) event.preventDefault();
      const item = event.target.closest('[data-fve-menu-label]');
      if (item?.getAttribute('aria-disabled') === 'true') { event.preventDefault(); event.stopPropagation(); }
    }, { ...options, capture: true });
    host.addEventListener('click', event => {
      if (event.defaultPrevented) return;
      if (trigger()?.contains(event.target)) {
        event.preventDefault(); $$.typeahead = '';
        menu().matches(':popover-open') ? hide() : show(event.detail === 0 ? 'first' : 'panel');
      } else if (event.target.closest('[data-fve-menu-label]:not([role=menuitemcheckbox])')) hide(true);
    }, options);
    host.addEventListener('keydown', event => {
      if (event.defaultPrevented) return;
      if (trigger()?.contains(event.target)) {
        if (unavailable()) return;
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') { event.preventDefault(); show(event.key === 'ArrowDown' ? 'first' : 'last'); }
        return;
      }
      if (!menu()?.contains(event.target)) return;
      const available = items(), index = available.indexOf(document.activeElement);
      if (event.key === 'Escape') { event.preventDefault(); hide(true); }
      else if (event.key === 'Tab') hide();
      else if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault(); const start = index < 0 ? (event.key === 'ArrowDown' ? -1 : 0) : index;
        available.at((start + (event.key === 'ArrowDown' ? 1 : -1) + available.length) % available.length)?.focus();
      } else if (event.key === 'Home' || event.key === 'End') { event.preventDefault(); (event.key === 'Home' ? available[0] : available.at(-1))?.focus(); }
      else if ((event.key === 'Enter' || event.key === ' ') && available.includes(document.activeElement)) { event.preventDefault(); document.activeElement.click(); }
      else if (!event.ctrlKey && !event.metaKey && !event.altKey && event.key.length === 1 && event.key !== ' ') {
        event.preventDefault(); const now = Date.now();
        $$.typeahead = now - $$.typeaheadTime > 700 ? event.key.toLowerCase() : $$.typeahead + event.key.toLowerCase(); $$.typeaheadTime = now;
        const text = [...$$.typeahead].every(character => character === $$.typeahead[0]) ? $$.typeahead[0] : $$.typeahead;
        const start = Math.max(0, index + 1);
        [...available.slice(start), ...available.slice(0, start)].find(item => item.dataset.fveMenuLabel.startsWith(text))?.focus();
      }
    }, options);
    const moved = event => event.clientX !== pointerX || event.clientY !== pointerY;
    host.addEventListener('pointermove', event => {
      const item = event.target.closest('[data-fve-menu-label]');
      if (!event.defaultPrevented && event.pointerType !== 'touch' && moved(event) && item?.getAttribute('aria-disabled') !== 'true') item?.focus({ preventScroll: true });
    }, options);
    host.addEventListener('pointerout', event => {
      const item = event.target.closest('[data-fve-menu-label]');
      if (item && !item.contains(event.relatedTarget) && event.pointerType !== 'touch' && moved(event) && document.activeElement === item) menu()?.focus({ preventScroll: true });
    }, options);
    window.addEventListener('pointermove', event => { pointerX = event.clientX; pointerY = event.clientY; }, options);
    window.addEventListener('scroll', () => position?.(), { ...options, capture: true });
    window.addEventListener('resize', () => position?.(), options);
    host.addEventListener('datastar-scope-children', () => {
      $$.open = menu()?.matches(':popover-open') || false;
      trigger()?.setAttribute('aria-expanded', String($$.open));
    }, options);
    host.addEventListener('beforetoggle', event => {
      if (event.target !== menu()) return;
      $$.open = event.newState === 'open'; trigger()?.setAttribute('aria-expanded', String($$.open));
      if (!$$.open) $$.typeahead = '';
    }, { ...options, capture: true });
    host.addEventListener('toggle', event => { if (event.target === menu() && event.newState === 'closed') clearPosition(); }, { ...options, capture: true });
    cleanup(() => { events.abort(); clearPosition(); });
  },
});
