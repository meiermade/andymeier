import { rocket } from './datastar-rocket.1.0.4.js';

rocket('fve-select', {
  mode: 'light', scope: false,
  setup: ({ host, $$, effect, actions, cleanup }) => {
    const events = new AbortController(), options = { signal: events.signal };
    const readSettings = () => JSON.parse(host.getAttribute('settings'));
    let settings = readSettings(), baseline = settings.initial, request, debounce, frame;
    let pointerX = NaN, pointerY = NaN;
    const fieldId = host.dataset.fveSelectField, instance = host.dataset.fveSelect;
    const trigger = () => host.querySelector('[data-fve-select-trigger-control]');
    const popup = () => [...host.querySelectorAll('[data-fve-select-content]')].find(el => el.closest('fve-select') === host);
    const search = () => popup()?.querySelector('input[type=search]');
    const listbox = () => popup()?.querySelector('[data-fve-select-options]');
    const allOptions = () => [...(listbox()?.querySelectorAll('[data-fve-option-value]') || [])];
    const set = (node, key, value) => {
      if (!node) return;
      if (value == null) { if (node.hasAttribute(key)) node.removeAttribute(key); }
      else if (node.getAttribute(key) !== String(value)) node.setAttribute(key, String(value));
    };
    const hidden = (node, value) => { if (node && node.hidden !== value) node.hidden = value; };
    const text = (node, value) => { if (node && node.textContent !== value) node.textContent = value; };
    const name = () => {
      const control = trigger();
      return control?.getAttribute('aria-labelledby')?.split(/\s+/).map(id => document.getElementById(id)?.textContent || '').join(' ') || control?.getAttribute('aria-label') || [...(control?.labels || [])].map(label => label.textContent.trim()).join(' ') || settings.label;
    };
    const nativeControl = () => host.querySelector('[data-fve-select-native]');
    const promoteNative = () => {
      const native = nativeControl();
      if (!native || !trigger()) return;
      native.id = fieldId + '-native'; trigger().id = host.dataset.fveSelectTrigger;
      native.disabled = true; hidden(native.parentElement, true); hidden(trigger(), false);
    };
    const native = nativeControl();
    let initial = [...host.querySelectorAll('[data-fve-select-values] input')].map(field => ({ value: field.value, label: field.dataset.fveChoiceLabel || settings.initial.find(item => item.value === field.value)?.label || '' })).filter(item => settings.multiple || item.label);
    if (native) {
      const focused = document.activeElement === native;
      const item = native.selectedOptions[0];
      initial = item && (item.value || settings.initial.some(choice => choice.value === item.value)) ? [{ value: item.value, label: item.textContent }] : [];
      promoteNative();
      if (focused) trigger().focus();
    }
    $$('selected', initial); $$('query', settings.query); $$('open', false);
    $$('active', ''); $$('typeahead', ''); $$('typeaheadTime', 0); $$('pending', false);
    const ready = () => !$$.pending && !popup()?.hasAttribute('data-fve-select-loading') && (settings.search !== 'remote' || $$.query === popup()?.dataset.fveSelectQuery);
    const visible = () => allOptions().filter(item => !item.disabled && !item.hidden);
    const abort = () => { window.clearTimeout(debounce); request?.controller.abort(); request = undefined; if (host.isConnected) $$.pending = false; };
    const position = () => {
      const panel = popup(), control = trigger();
      if (!panel?.isConnected || !control || !panel.matches(':popover-open')) return;
      const scroll = listbox() || panel, bounds = control.getBoundingClientRect(), viewport = window.visualViewport;
      const top = (viewport?.offsetTop || 0) + 8, bottom = (viewport?.offsetTop || 0) + (viewport?.height || innerHeight) - 8;
      panel.style.positionArea = settings.position; panel.style.margin = '0.25rem 0';
      panel.style.top = 'auto'; panel.style.bottom = 'auto'; panel.style.left = 'auto'; panel.style.right = 'auto';
      panel.style.width = 'anchor-size(width)'; panel.style.maxHeight = '';
      const limit = parseFloat(getComputedStyle(panel).maxHeight), padding = parseFloat(getComputedStyle(panel).paddingTop) || 0;
      const inset = parseFloat(getComputedStyle(document.documentElement).fontSize) / 4;
      panel.style.maxHeight = Math.max(0, Math.min(limit, bottom - top)) + 'px';
      if (settings.aligned && !matchMedia('(pointer: coarse)').matches) {
        panel.style.width = Math.max(0, bounds.width - inset * 2) + 'px'; scroll.scrollTop = 0;
        const selected = panel.querySelector('[role=option][aria-selected=true]') || panel.querySelector('[role=option]:not(:disabled)');
        if (selected) {
          const item = selected.getBoundingClientRect(), box = panel.getBoundingClientRect();
          const height = box.height, center = (bounds.top + bounds.bottom) / 2, itemCenter = item.top - box.top + item.height / 2;
          const minOffset = Math.max(item.height / 2 + padding, itemCenter - (scroll.scrollHeight - scroll.clientHeight), center - (bottom - height));
          const maxOffset = Math.min(height - item.height / 2 - padding, itemCenter, center - top);
          if (minOffset <= maxOffset) {
            const offset = Math.max(minOffset, Math.min(maxOffset, itemCenter));
            const leftEdge = (viewport?.offsetLeft || 0) + 8, rightEdge = (viewport?.offsetLeft || 0) + (viewport?.width || innerWidth) - 8;
            const width = Math.min(Math.max(0, bounds.width - inset * 2), Math.max(0, rightEdge - leftEdge));
            panel.style.positionArea = 'none'; panel.style.margin = '0'; panel.style.width = width + 'px';
            panel.style.top = (center - offset) + 'px'; panel.style.left = Math.max(leftEdge, Math.min(bounds.left + inset, rightEdge - width)) + 'px';
            scroll.scrollTop = itemCenter - offset; return;
          }
        }
      }
      panel.style.width = 'anchor-size(width)';
      const below = Math.max(0, bottom - bounds.bottom), above = Math.max(0, bounds.top - top), useBelow = below >= panel.offsetHeight || below >= above;
      panel.style.positionArea = useBelow ? settings.position : settings.position.replace('block-end', 'block-start');
      panel.style.maxHeight = Math.max(0, Math.min(limit, (useBelow ? below : above) - 4)) + 'px';
      const selected = panel.querySelector('[role=option][aria-selected=true]');
      if (selected) { const itemTop = selected.getBoundingClientRect().top - scroll.getBoundingClientRect().top + scroll.scrollTop; scroll.scrollTop = Math.max(0, itemTop + selected.offsetHeight / 2 - scroll.clientHeight / 2); }
    };
    const positionSoon = () => { cancelAnimationFrame(frame); frame = requestAnimationFrame(position); };
    const synchronize = () => {
      if (!host.isConnected) return;
      promoteNative();
      const panel = popup(), control = trigger(), label = name(), selected = $$.selected;
      // Keep the disabled native fallback current for a subsequent takeover on reconnect.
      const native = nativeControl();
      if (native && native.value !== (selected[0]?.value || '')) native.value = selected[0]?.value || '';
      const values = host.querySelector('[data-fve-select-values]');
      const needed = settings.multiple ? selected : selected.length ? selected : [{ value: '', label: '' }];
      const fields = [...values.querySelectorAll('input')];
      needed.forEach((choice, index) => {
        const field = fields[index] || document.createElement('input');
        if (!fields[index]) { field.type = 'hidden'; values.append(field); }
        if (!settings.multiple) field.id = fieldId + '-value';
        if (field.name !== settings.name) field.name = settings.name;
        if (field.value !== choice.value) field.value = choice.value;
        set(field, 'data-fve-choice-label', choice.label);
        if (field.disabled !== settings.disabled) field.disabled = settings.disabled;
      });
      for (const field of fields.slice(needed.length)) field.remove();
      const summary = selected.length ? selected.slice(0, 2).map(item => item.label).join(', ') + (selected.length > 2 ? ' + ' + (selected.length - 2) + ' more' : '') : settings.placeholder;
      text(host.querySelector('[data-fve-select-label]'), settings.multiple ? summary : (selected.length ? selected[0].label : (settings.search === 'none' ? settings.placeholder : '')));
      text(host.querySelector('[data-fve-select-announcement]'), selected.length + ' selected');
      set(control, 'aria-expanded', $$.open);
      const active = $$.open && document.getElementById($$.active) ? $$.active : null;
      if (settings.search === 'none' && !settings.multiple) set(control, 'aria-activedescendant', active);
      if (!panel) return;
      panel.id ||= fieldId + '-popup'; listbox().id ||= fieldId + '-options'; search().id ||= fieldId + '-search';
      set(control, 'aria-controls', settings.search === 'none' ? listbox().id : panel.id);
      if (settings.search === 'none') set(control, 'popovertarget', panel.id);
      set(panel, 'aria-label', label + ' options'); set(listbox(), 'aria-label', label);
      set(listbox(), 'aria-multiselectable', settings.multiple ? 'true' : null); set(listbox(), 'aria-required', settings.required);
      set(listbox(), 'aria-busy', ready() ? null : 'true'); set(listbox(), 'aria-activedescendant', active);
      const query = $$.query.trim().toLowerCase();
      for (const item of allOptions()) {
        item.id ||= fieldId + '-option-v' + [...new TextEncoder().encode(item.dataset.fveOptionValue)].map(byte => byte.toString(16).padStart(2, '0')).join('');
        const disabled = item.hasAttribute('data-fve-option-disabled') || settings.unavailable || settings.blocked.includes(item.dataset.fveOptionValue);
        if (item.disabled !== disabled) item.disabled = disabled;
        const chosen = selected.some(choice => choice.value === item.dataset.fveOptionValue);
        set(item, 'aria-disabled', disabled); set(item, 'aria-selected', chosen); set(item, 'data-active', $$.active === item.id ? 'true' : null);
        hidden(item, !ready() || (settings.search === 'static' && !item.dataset.fveOptionLabel.includes(query)));
        hidden(item.querySelector('[data-fve-select-check]'), !chosen);
      }
      hidden(panel.querySelector('[data-fve-select-search-frame]'), settings.search === 'none');
      hidden(panel.querySelector('[data-fve-select-multiple-actions]'), !settings.multiple);
      set(search(), 'aria-label', 'Search ' + label); set(search(), 'aria-controls', listbox().id);
      set(search(), 'aria-busy', ready() ? null : 'true'); set(search(), 'aria-activedescendant', active);
      if (search().disabled !== settings.unavailable) search().disabled = settings.unavailable;
      if (search().value !== $$.query) search().value = $$.query;
      const clearSearch = panel.querySelector('[data-fve-select-action=clear-search]');
      set(clearSearch, 'aria-label', 'Clear Search ' + label); clearSearch.disabled = settings.unavailable; hidden(clearSearch, !$$.query);
      const all = panel.querySelector('[data-fve-select-action=all]'), clear = panel.querySelector('[data-fve-select-action=clear]');
      const actionLabel = settings.search === 'remote' ? 'Select all results' : 'Select all';
      text(all, actionLabel); set(all, 'aria-label', actionLabel + ' ' + label);
      all.disabled = settings.unavailable || !ready() || !visible().some(option => !selected.some(item => item.value === option.dataset.fveOptionValue));
      set(clear, 'aria-label', 'Clear ' + label); clear.disabled = settings.unavailable || !selected.length;
      hidden(panel.querySelector('[data-fve-select-busy]'), ready()); hidden(panel.querySelector('[data-fve-select-error]'), !ready());
      const anyMatch = allOptions().some(item => settings.search !== 'static' || item.dataset.fveOptionLabel.includes(query));
      hidden(panel.querySelector('[data-fve-select-empty]'), !ready() || anyMatch);
      if ($$.open && !panel.matches(':popover-open') && !settings.unavailable) { panel.showPopover({ source: control }); positionSoon(); }
      else if ((!$$.open || settings.unavailable) && panel.matches(':popover-open')) panel.hidePopover();
    };
    const changed = choices => {
      const previous = JSON.stringify($$.selected);
      $$.selected = choices; synchronize();
      if (previous === JSON.stringify(choices)) return;
      if (!settings.multiple) host.querySelector('[data-fve-select-values] input').dispatchEvent(new Event('change', { bubbles: true }));
      else host.dispatchEvent(new Event('change', { bubbles: true }));
      host.dispatchEvent(new CustomEvent('fve-select-change', { bubbles: true, detail: { value: choices[0]?.value || '', values: choices.map(item => item.value), selected: choices } }));
    };
    const requestSearch = (endpoint, refresh = false) => {
      if (settings.search !== 'remote' || settings.unavailable || !host.isConnected || (!$$.open && !refresh)) return;
      abort();
      const controller = new AbortController(), current = { controller, field: search() };
      request = current; $$.pending = true;
      const payload = { [instance + '_query']: $$.query, [instance + '_value']: !settings.multiple && $$.selected.length ? $$.selected[0].value : '', [instance + '_selected']: $$.selected.map(item => ({ value: item.value, label: item.label })) };
      actions.get(endpoint || settings.endpoint, { payload, requestCancellation: controller }).finally(() => {
        if (request !== current) return;
        request = undefined; if (host.isConnected) $$.pending = false;
      });
    };
    const focusContent = () => (settings.search === 'none' ? listbox() : search())?.focus();
    const activate = item => { $$.active = item?.id || ''; queueMicrotask(() => { if (host.isConnected && $$.open) document.getElementById($$.active)?.scrollIntoView({ block: 'nearest' }); }); };
    const preferred = () => visible().find(item => item.getAttribute('aria-selected') === 'true') || visible()[0];
    const open = () => { if (settings.search === 'none' && !settings.multiple) trigger().focus(); $$.open = true; synchronize(); if (settings.search !== 'none' || settings.multiple) focusContent(); if (settings.search === 'remote' && $$.query !== popup()?.dataset.fveSelectQuery) requestSearch(); };
    const choose = item => {
      if (!item || item.disabled || settings.unavailable || !ready()) return;
      $$.active = item.id; $$.typeahead = '';
      const value = item.dataset.fveOptionValue, label = item.dataset.fveOptionText;
      if (settings.multiple) { changed($$.selected.some(item => item.value === value) ? $$.selected.filter(item => item.value !== value) : [...$$.selected, { value, label }]); focusContent(); }
      else { changed([{ value, label }]); $$.query = ''; $$.open = false; synchronize(); trigger().focus(); }
    };
    host.addEventListener('input', event => {
      // A composed control may update the native fallback without committing a user change.
      if (event.target === nativeControl()) {
        const item = event.target.selectedOptions[0];
        $$.selected = item ? [{ value: item.value, label: item.textContent }] : [];
        synchronize(); return;
      }
      if (event.target !== search() || settings.unavailable) return;
      const query = event.target.value;
      abort(); $$.query = query; $$.active = ''; $$.open = true;
      if (settings.search === 'remote') { $$.pending = true; debounce = window.setTimeout(() => requestSearch(), 250); }
    }, options);
    host.addEventListener('click', event => {
      const item = event.target.closest('[data-fve-option-value]');
      if (settings.unavailable && (trigger()?.contains(event.target) || popup()?.contains(event.target)) || item?.disabled) { event.preventDefault(); event.stopPropagation(); }
    }, { ...options, capture: true });
    host.addEventListener('mousedown', event => { if (event.target.closest('[data-fve-option-value]')) event.preventDefault(); }, options);
    host.addEventListener('click', event => {
      if (event.defaultPrevented || event.target.closest('fve-select') !== host) return;
      if (trigger()?.contains(event.target)) { event.preventDefault(); if ($$.open) $$.open = false; else { open(); if (settings.search === 'none' && !settings.multiple) activate(preferred()); } return; }
      const item = event.target.closest('[data-fve-option-value]');
      if (item) { choose(item); return; }
      const action = event.target.closest('[data-fve-select-action]');
      if (!action || settings.unavailable) return;
      switch (action.dataset.fveSelectAction) {
        case 'clear-search': search().value = ''; search().dispatchEvent(new Event('input', { bubbles: true })); search().dispatchEvent(new Event('change', { bubbles: true })); search().focus(); break;
        case 'all': if (ready()) changed([...$$.selected, ...visible().filter(option => !$$.selected.some(item => item.value === option.dataset.fveOptionValue)).map(item => ({ value: item.dataset.fveOptionValue, label: item.dataset.fveOptionText }))]); focusContent(); break;
        case 'clear': changed([]); focusContent(); break;
        case 'retry': requestSearch(action.dataset.fveSelectEndpoint); focusContent(); break;
      }
    }, options);
    host.addEventListener('fve-select-search', event => { if (event.target.closest('fve-select') === host) { event.stopPropagation(); requestSearch(event.detail?.endpoint); } }, options);
    host.addEventListener('fve-select-refresh', event => { if (event.target === host) requestSearch(event.detail?.endpoint, true); }, options);
    host.addEventListener('keydown', event => {
      if (event.defaultPrevented || event.isComposing || settings.unavailable || event.target.closest('fve-select') !== host) return;
      const searching = settings.search !== 'none', onTrigger = event.target === trigger();
      if (searching && onTrigger) {
        if (['Enter', ' ', 'ArrowDown'].includes(event.key)) { event.preventDefault(); open(); }
        else if (event.key === 'Escape' && $$.open) { event.preventDefault(); $$.open = false; }
        return;
      }
      if (searching && event.target !== search() || !searching && !onTrigger && event.target !== listbox()) return;
      const available = visible(), index = available.findIndex(item => item.id === $$.active), key = event.key;
      if (key === 'Escape' && $$.open) { event.preventDefault(); $$.open = false; $$.typeahead = ''; synchronize(); trigger().focus(); }
      else if (key === 'Tab' && $$.open) { if (!searching) { if (!settings.multiple) choose(available[index]); else trigger().focus(); } if (!settings.multiple || !searching) $$.open = false; }
      else if (event.altKey && key === 'ArrowUp' && $$.open && !searching) { event.preventDefault(); if (!settings.multiple) choose(available[index]); else { $$.open = false; synchronize(); trigger().focus(); } }
      else if (key === 'ArrowDown' || key === 'ArrowUp') {
        event.preventDefault(); const wasOpen = $$.open; open();
        if (event.altKey) { if (!wasOpen) activate(preferred()); return; }
        if (searching) activate(available.at(((index < 0 ? key === 'ArrowDown' ? -1 : 0 : index) + (key === 'ArrowDown' ? 1 : -1) + available.length) % available.length));
        else if (!wasOpen) activate(key === 'ArrowDown' ? preferred() : visible()[0]);
        else activate(available[Math.max(0, Math.min(available.length - 1, (index < 0 ? 0 : index + (key === 'ArrowDown' ? 1 : -1))))]);
      } else if ((key === 'Home' || key === 'End') && (!searching || $$.open)) { event.preventDefault(); open(); activate(key === 'Home' ? visible()[0] : visible().at(-1)); }
      else if (!searching && (key === 'PageUp' || key === 'PageDown')) { event.preventDefault(); open(); activate(visible()[Math.max(0, Math.min(visible().length - 1, index < 0 ? 0 : index + (key === 'PageDown' ? 10 : -10)))]); }
      else if (key === 'Enter' || !searching && key === ' ') { event.preventDefault(); if ($$.open && $$.active) choose(available[index]); else { open(); activate(preferred()); } }
      else if (!searching && key.length === 1 && key !== ' ' && !event.altKey && !event.ctrlKey && !event.metaKey) {
        event.preventDefault(); $$.typeahead = Date.now() - $$.typeaheadTime > 700 ? key.toLowerCase() : $$.typeahead + key.toLowerCase(); $$.typeaheadTime = Date.now(); open();
        const text = [...$$.typeahead].every(character => character === $$.typeahead[0]) ? $$.typeahead[0] : $$.typeahead, start = Math.max(0, index + 1), choices = visible();
        activate([...choices.slice(start), ...choices.slice(0, start)].find(item => item.dataset.fveOptionLabel.startsWith(text)) || document.getElementById($$.active));
      }
    }, options);
    const moved = event => event.pointerType !== 'touch' && (event.clientX !== pointerX || event.clientY !== pointerY);
    host.addEventListener('pointermove', event => { const item = event.target.closest('[data-fve-option-value]'); if (item && !item.disabled && ready() && moved(event)) $$.active = item.id; }, options);
    host.addEventListener('pointerout', event => { const item = event.target.closest('[data-fve-option-value]'); if (item && !item.contains(event.relatedTarget) && moved(event) && $$.active === item.id) $$.active = ''; }, options);
    window.addEventListener('pointermove', event => { pointerX = event.clientX; pointerY = event.clientY; }, options);
    host.addEventListener('beforetoggle', event => {
      if (event.target !== popup()) return;
      $$.open = event.newState === 'open';
      if (!$$.open) { $$.active = ''; $$.typeahead = ''; abort(); }
    }, { ...options, capture: true });
    window.addEventListener('resize', positionSoon, options);
    window.addEventListener('scroll', event => { if (!(event.target instanceof Node && popup()?.contains(event.target))) positionSoon(); }, { ...options, capture: true });
    window.addEventListener('pagehide', abort, options);
    const reconcile = () => {
      if (!host.isConnected) return;
      const next = readSettings();
      if (JSON.stringify(next.initial) !== JSON.stringify(settings.initial)) { baseline = next.initial; $$.selected = next.initial; }
      if (next.query !== settings.query) $$.query = next.query;
      settings = next;
      if (request && (!request.field.isConnected || settings.unavailable)) abort();
      if (settings.unavailable) $$.open = false;
      synchronize();
    };
    const observer = new MutationObserver(reconcile);
    observer.observe(host, { subtree: true, childList: true, attributes: true, attributeFilter: ['settings', 'aria-label', 'aria-labelledby', 'data-fve-select-query', 'data-fve-select-loading', 'data-fve-option-disabled'] });
    host.addEventListener('datastar-scope-children', reconcile, options);
    host.closest('form')?.addEventListener('reset', event => queueMicrotask(() => { if (!event.defaultPrevented && host.isConnected) { abort(); $$.selected = baseline; $$.query = settings.query; $$.open = false; synchronize(); } }), options);
    effect(synchronize);
    cleanup(() => { events.abort(); observer.disconnect(); abort(); cancelAnimationFrame(frame); });
  },
});
