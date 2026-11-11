import { rocket } from './datastar-rocket.1.0.4.js';

const base = '[&_svg]:size-[var(--fve-control-icon-size,1rem)] [&_svg]:shrink-0 inline-flex min-h-[var(--fve-control-min-height)] items-center justify-center gap-2 rounded-[var(--fve-radius-control)] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)] font-medium shadow-xs outline-none transition-[color,background-color,border-color,box-shadow,translate] duration-100 motion-reduce:transition-none motion-safe:[&:not(:disabled):not([aria-disabled=true]):not([aria-busy=true]):not([aria-haspopup]):not([data-fve-keyboard-pressed])]:active:translate-y-px motion-safe:[&:not(:disabled):not([aria-disabled=true]):not([aria-busy=true]):not([aria-haspopup])]:data-[fve-keyboard-pressed=true]:translate-y-px focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[var(--fve-color-focus)] focus-visible:ring-offset-[var(--fve-surface)] forced-colors:focus-visible:outline-solid forced-colors:focus-visible:outline-2 forced-colors:focus-visible:-outline-offset-2 forced-colors:focus-visible:outline-[Highlight] disabled:pointer-events-none disabled:opacity-50 relative [&[aria-busy=true]>[data-fve-button-content]]:opacity-0 [&[aria-busy=true]>[data-fve-button-spinner]]:flex';
const appearances = {
  solid: 'bg-[var(--fve-color-solid)] text-[var(--fve-color-on-solid)] enabled:hover:bg-[var(--fve-color-hover)] enabled:active:bg-[var(--fve-color-active)]',
  soft: 'bg-[var(--fve-color-soft)] text-[var(--fve-color-text)] enabled:hover:bg-[var(--fve-color-soft-hover)] enabled:active:bg-[var(--fve-color-soft-active)]',
  outline: '[--fve-control-border-width:1px] border border-[color-mix(in_srgb,var(--fve-color-border)_20%,transparent)] bg-transparent text-[var(--fve-color-text)] enabled:hover:bg-[var(--fve-color-soft-hover)] enabled:active:bg-[var(--fve-color-soft-active)]',
  ghost: 'bg-transparent text-[var(--fve-color-text)] enabled:hover:bg-[var(--fve-color-soft-hover)] enabled:active:bg-[var(--fve-color-soft-active)]',
};
const geometry = 'min-w-[var(--fve-control-min-height)] p-[max(0px,calc((var(--fve-control-min-height)-var(--fve-control-line-height))/2-var(--fve-control-border-width,0px)))]';
const sizes = {
  '': '',
  'extra-small': 'fve-control-extra-small [--fve-control-min-height:1.5rem] [--fve-control-icon-size:0.75rem] [--fve-control-padding-block:0.25rem] [--fve-control-padding-inline:0.5rem] [--fve-control-font-size:0.75rem] [--fve-control-line-height:1rem]',
  small: 'fve-control-small [--fve-control-min-height:1.75rem] [--fve-control-icon-size:1rem] [--fve-control-padding-block:0.25rem] [--fve-control-padding-inline:0.5rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]',
  medium: 'fve-control-medium [--fve-control-min-height:2rem] [--fve-control-icon-size:1rem] [--fve-control-padding-block:0.375rem] [--fve-control-padding-inline:0.625rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]',
  large: 'fve-control-large [--fve-control-min-height:2.25rem] [--fve-control-icon-size:1.25rem] [--fve-control-padding-block:0.5rem] [--fve-control-padding-inline:0.75rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]',
  'extra-large': 'fve-control-extra-large [--fve-control-min-height:2.5rem] [--fve-control-icon-size:1.25rem] [--fve-control-padding-block:0.625rem] [--fve-control-padding-inline:0.875rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]',
};
const fallbacks = {
  primary: ['var(--fve-brand-text)', '#ffffff', 'var(--fve-brand-subtle)', 'var(--fve-brand-text)', '#101828', 'var(--fve-brand-subtle)'],
  secondary: ['#6d28d9', '#ffffff', '#f5f3ff', '#c4b5fd', '#101828', '#2e1065'],
  success: ['#15803d', '#ffffff', '#f0fdf4', '#86efac', '#101828', '#052e16'],
  warning: ['#92400e', '#ffffff', '#fffbeb', '#fde68a', '#101828', '#451a03'],
  error: ['#b91c1c', '#ffffff', '#fef2f2', '#fca5a5', '#101828', '#450a0a'],
  info: ['#1d4ed8', '#ffffff', '#eff6ff', '#93c5fd', '#101828', '#172554'],
  neutral: ['#374151', '#ffffff', 'var(--fve-surface-subtle)', '#e5e7eb', '#101828', 'var(--fve-surface-subtle)'],
};
const fields = { solid: 'solid', solidHover: 'hover', solidActive: 'active', onSolid: 'on-solid', soft: 'soft', softHover: 'soft-hover', softActive: 'soft-active', text: 'text', border: 'border', focus: 'focus' };
function mode(solid, onSolid, soft, mix) {
  return { solid, solidHover: `color-mix(in srgb, ${solid} 85%, ${mix})`, solidActive: `color-mix(in srgb, ${solid} 70%, ${mix})`, onSolid, soft,
    softHover: `color-mix(in srgb, ${soft} 90%, ${solid})`, softActive: `color-mix(in srgb, ${soft} 80%, ${solid})`, text: solid, border: solid, focus: solid };
}
function palette(name) {
  const [a,b,c,d,e,f] = fallbacks[name];
  const tokens = values => Object.fromEntries(Object.entries(values).map(([key, value]) => [key, `var(--fve-${name}-${fields[key]}, ${value})`]));
  return { light: tokens(mode(a,b,c,'#000000')), dark: tokens(mode(d,e,f,'#ffffff')) };
}
const validPalette = value => value && ['light', 'dark'].every(mode => value[mode] && Object.keys(fields).every(key => typeof value[mode][key] === 'string' && value[mode][key].trim()));
const nativeAttributes = new Set(['name', 'value', 'form', 'formaction', 'formmethod', 'formenctype', 'formtarget', 'formnovalidate', 'autofocus', 'tabindex', 'title', 'accesskey', 'hidden', 'lang', 'dir', 'popovertarget', 'popovertargetaction', 'command', 'commandfor']);

const newControls = new Set();
function settlePresentation(control) {
  const schedule = newControls.size === 0;
  newControls.add(control);
  if (schedule) queueMicrotask(() => {
    // Read the batch before cancelling: interleaving style reads/writes forces a layout per button.
    const animations = [...newControls].filter(control => control.isConnected).flatMap(control => control.getAnimations());
    newControls.clear();
    for (const animation of animations) if (animation instanceof CSSTransition) animation.cancel();
  });
}

rocket('fve-button', {
  mode: 'light', scope: false,
  renderOnPropChange: false,
  props: ({ oneOf, json, bool }) => ({
    color: oneOf(...Object.keys(fallbacks), 'custom').default('neutral'),
    variant: oneOf(...Object.keys(appearances)).default('outline'),
    size: oneOf(...Object.keys(sizes)).default(''),
    palette: json.default(null),
    disabled: bool.default(false),
    type: oneOf('button', 'submit', 'reset').default('button'),
  }),
  render: ({ html, host }) => {
    // Content is opaque HTML. Reuse our own content region, or fresh server-authored children after a morph.
    const content = host.querySelector(':scope > button[data-fve-button-control] > [data-fve-button-content]') ?? host;
    const children = [...content.childNodes];
    return html`<button data-fve-button-control type="button"><span data-fve-button-content class="inline-flex min-w-0 items-center justify-center gap-2">${children}</span><span data-fve-button-spinner aria-hidden="true" class="pointer-events-none absolute inset-0 hidden items-center justify-center"><span class="size-[var(--fve-control-icon-size,1rem)] shrink-0 animate-spin rounded-full border-2 border-current border-r-transparent motion-reduce:animate-none"></span></span></button>`;
  },
  setup: ({ host, props, observeProps, defineHostProp, render, cleanup }) => {
    let control;
    let presentedControl;
    let forwarded = new Set();
    let focused = false;
    const previousStyle = host.style.display === 'contents' ? host.querySelector(':scope > button[data-fve-button-control]')?.style : host.style;
    let display = previousStyle?.display ?? '';
    let displayPriority = previousStyle?.getPropertyPriority('display') ?? '';
    if (host.style.display !== 'none') host.style.display = 'contents';
    const sync = () => {
      if (!host.isConnected) return;
      // Datastar owns visibility. Never undo display:none or its style observer will undo us.
      if (host.style.display !== 'contents' && host.style.display !== 'none') {
        display = host.style.display;
        displayPriority = host.style.getPropertyPriority('display');
        host.style.display = 'contents';
      }
      control = host.querySelector(':scope > button[data-fve-button-control]');
      if (!control) return;
      control.type = props.type;
      control.disabled = props.disabled;
      control.dataset.fveGroupControl = props.variant === 'outline' ? 'outline' : 'plain';
      const controlId = host.getAttribute('data-fve-control-id');
      if (controlId) control.id = controlId;
      else control.removeAttribute('id');
      control.className = `${base} ${geometry} ${appearances[props.variant]} ${sizes[props.size]} ${host.getAttribute('class') ?? ''}`;
      const colors = props.color === 'custom' && validPalette(props.palette) ? props.palette : palette(props.color === 'custom' ? 'neutral' : props.color);
      control.style.cssText = '';
      for (const [key, name] of Object.entries(fields)) control.style.setProperty(`--fve-color-${name}`, `light-dark(${colors.light[key]},${colors.dark[key]})`);
      // Consumer styles are last, just as with ordinary native HTML. Display belongs to the host's DOM boundary.
      const authoredStyle = document.createElement('span').style;
      authoredStyle.cssText = host.getAttribute('style') ?? '';
      authoredStyle.removeProperty('display');
      control.style.cssText += `;${authoredStyle.cssText}`;
      if (display) control.style.setProperty('display', display, displayPriority);
      const next = new Set();
      for (const {name, value} of host.attributes) {
        if (!nativeAttributes.has(name) && !name.startsWith('aria-')) continue;
        next.add(name);
        if (control.getAttribute(name) !== value) control.setAttribute(name, value);
      }
      for (const name of forwarded) if (!next.has(name)) control.removeAttribute(name);
      forwarded = next;
      if (presentedControl !== control) {
        presentedControl = control;
        // New/rebuilt controls must not transition from their unstyled native defaults.
        settlePresentation(control);
      }
      if (props.disabled || control.getAttribute('aria-busy') === 'true') release();
      if (focused && !control.disabled && document.hasFocus() && document.activeElement === document.body) control.focus({ preventScroll: true });
    };
    const unavailable = () => props.disabled || control?.matches(':disabled') || host.getAttribute('aria-disabled') === 'true';
    // Keep a false marker after keyboard release: WebKit can retain :active while Space is held across Tab.
    const release = () => { if (control) control.dataset.fveKeyboardPressed = 'false'; };
    const on = (target, event, listener, options) => {
      target.addEventListener(event, listener, options);
      cleanup(() => target.removeEventListener(event, listener, options));
    };
    on(host, 'click', event => {
      if (unavailable()) { event.preventDefault(); event.stopImmediatePropagation(); }
    }, true);
    on(host, 'keydown', event => {
      if (![' ', 'Enter'].includes(event.key)) { if (event.key === 'Tab') focused = false; release(); return; }
      if (event.target === control && !unavailable() && [' ', 'Enter'].includes(event.key) && !event.altKey && !event.ctrlKey && !event.metaKey
        && !control.hasAttribute('aria-haspopup') && control.getAttribute('aria-busy') !== 'true') control.dataset.fveKeyboardPressed = 'true';
    });
    on(host, 'keyup', event => { if ([' ', 'Enter'].includes(event.key)) release(); });
    on(host, 'pointerdown', () => { if (control) delete control.dataset.fveKeyboardPressed; });
    on(host, 'focusin', event => { focused = event.target === control; });
    // Disabling/removing the focused native control temporarily moves focus to body. Do not steal an intentional move elsewhere.
    on(host, 'focusout', event => {
      if (event.relatedTarget) focused = false;
      else {
        const previous = control;
        // Chromium can emit blur before a morph detaches the native control. Distinguish that from an intentional blur.
        queueMicrotask(() => { if (host.contains(previous) && !previous.disabled && document.activeElement !== previous) focused = false; });
      }
      release();
    });
    on(document, 'focusin', event => { if (!host.contains(event.target)) focused = false; }, true);
    on(document, 'pointerdown', event => { if (!host.contains(event.target)) focused = false; }, true);
    on(window, 'blur', () => { focused = false; release(); });
    // Datastar owns the host's attributes/content. Rocket repairs only its generated internals after a server morph.
    on(host, 'datastar-scope-children', () => {
      render();
      sync();
      if (focused && document.hasFocus()) control?.focus({ preventScroll: true });
    });
    defineHostProp('focus', { value: options => control?.focus(options) });
    defineHostProp('blur', { value: () => { focused = false; control?.blur(); } });
    defineHostProp('click', { value: () => control?.click() });
    observeProps(sync);
    const observer = new MutationObserver(sync);
    observer.observe(host, { attributes: true });
    cleanup(() => observer.disconnect());
    queueMicrotask(sync);
  },
});
