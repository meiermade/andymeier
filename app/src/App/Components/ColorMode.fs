namespace App.Components

open System
open System.Text.Json
open FSharp.ViewEngine
open type Html

/// <category>theming</category>
[<RequireQualifiedAccess>]
type ColorMode =
    | System
    | Light
    | Dark

/// <summary>Document appearance setup, independent of the controls used to change it.</summary>
/// <category>theming</category>
[<RequireQualifiedAccess>]
module ColorMode =
    let value =
        function
        | ColorMode.System -> "system"
        | ColorMode.Light -> "light"
        | ColorMode.Dark -> "dark"

    /// Mark native system/light/dark radios for activation before Datastar initializes.
    /// The application supplies labels, grouping, presentation and enhanced selection bindings.
    let nativeChoiceAttributes = [ _attr ("data-fve-color-mode-choice", "true") ]

    /// Include once in the document head before the stylesheet. The application owns the
    /// localStorage key and nonce; controls call window.fveColorMode and observe fve-color-mode.
    let assetsWithNonce storageKey defaultMode nonce =
        if String.IsNullOrWhiteSpace storageKey then
            invalidArg (nameof storageKey) "A product-owned preference storage key is required."

        let source =
            """
(() => {
  const valid = new Set(['system', 'light', 'dark']);
  const media = matchMedia('(prefers-color-scheme: dark)');
  window.fveColorMode = {
    storageKey: __KEY__, defaultMode: __DEFAULT__,
    current() {
      try {
        if (parent !== window && parent.location.origin === location.origin && frameElement?.hasAttribute('data-docs-preview-src')) {
          return parent.fveColorMode?.current() ?? (parent.document.documentElement.classList.contains('dark') ? 'dark' : 'light');
        }
        const stored = localStorage.getItem(this.storageKey);
        return valid.has(stored) ? stored : this.defaultMode;
      } catch { return this.defaultMode; }
    },
    apply(mode) {
      const selected = valid.has(mode) ? mode : this.defaultMode;
      const dark = selected === 'dark' || (selected === 'system' && media.matches);
      document.documentElement.classList.toggle('dark', dark);
      document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
      document.documentElement.dataset.colorMode = selected;
      return selected;
    },
    set(mode) {
      const selected = this.apply(mode);
      try { localStorage.setItem(this.storageKey, selected); } catch {}
      window.dispatchEvent(new CustomEvent('fve-color-mode', {detail: {mode: selected}}));
      return selected;
    },
    refresh() {
      const selected = this.apply(this.current());
      window.dispatchEvent(new CustomEvent('fve-color-mode', {detail: {mode: selected}}));
    }
  };
  window.fveColorMode.apply(window.fveColorMode.current());
  const applyNativeChoice = event => {
    const input = event.target;
    if (input instanceof HTMLInputElement && input.type === 'radio' && input.checked && input.closest('[data-fve-color-mode-choice]') && valid.has(input.value)) window.fveColorMode.set(input.value);
  };
  document.addEventListener('change', applyNativeChoice);
  document.addEventListener('click', applyNativeChoice);
  document.addEventListener('keydown', event => { if (event.key === ' ') applyNativeChoice(event); });
  media.addEventListener('change', () => { if (window.fveColorMode.current() === 'system') window.fveColorMode.refresh(); });
  window.addEventListener('storage', event => { if (event.key === null || event.key === window.fveColorMode.storageKey) window.fveColorMode.refresh(); });
})();
"""

        script {
            match nonce with
            | Some value -> _attr ("nonce", value)
            | None -> ()

            raw (
                source
                    .Replace("__KEY__", JsonSerializer.Serialize(storageKey))
                    .Replace("__DEFAULT__", JsonSerializer.Serialize(value defaultMode))
            )
        }

    /// Initialize document appearance before the stylesheet when no nonce is required.
    let assets storageKey defaultMode =
        assetsWithNonce storageKey defaultMode None
