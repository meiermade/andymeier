import { rocket } from './datastar-rocket.1.0.4.js';

rocket('fve-popover', {
  mode: 'light', scope: false,
  setup: ({ host, $$, effect, cleanup }) => {
    const events = new AbortController();
    let escape = false;
    const content = () => [...host.querySelectorAll('[data-fve-popover-content]')].find(el => el.closest('fve-popover') === host);
    const trigger = () => {
      const panel = content();
      return panel && host.ownerDocument.getElementById(panel.dataset.fvePopoverTrigger);
    };
    $$('open', !!content()?.matches(':popover-open'));
    const synchronize = () => {
      const control = trigger();
      if (control) control.setAttribute('aria-expanded', String($$.open));
    };
    host.addEventListener('click', event => {
      const control = trigger();
      if (control?.contains(event.target) && (control.disabled || control.getAttribute('aria-disabled') === 'true')) event.preventDefault();
    }, { signal: events.signal, capture: true });
    host.addEventListener('keydown', event => {
      if (event.key === 'Escape' && !event.defaultPrevented && content()?.contains(event.target)) escape = true;
    }, { signal: events.signal });
    host.addEventListener('toggle', event => {
      if (event.target !== content()) return;
      $$.open = event.newState === 'open';
      synchronize();
      if ($$.open && host.getAttribute('focus-on-open') === 'true') queueMicrotask(() => {
        const panel = content();
        if (host.isConnected && panel?.matches(':popover-open')) panel.querySelector('input:not(:disabled),select:not(:disabled),textarea:not(:disabled),button:not(:disabled),a[href],[tabindex="0"]')?.focus();
      });
      if (!$$.open && escape) { escape = false; trigger()?.focus(); }
    }, { signal: events.signal, capture: true });
    host.addEventListener('datastar-scope-children', () => { $$.open = !!content()?.matches(':popover-open'); synchronize(); }, { signal: events.signal });
    effect(synchronize);
    cleanup(() => events.abort());
  },
});
