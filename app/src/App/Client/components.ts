// The shared component runtime also owns Datastar; do not load a second Datastar bundle.
import '../Components/Assets/button.js';
import '../Components/Assets/dropdown-menu.js';
import '../Components/Assets/popover.js';

window.addEventListener('fve-color-mode', () => {
  const renderMermaid = (window as Window & { renderMermaid?: (root: Document) => Promise<void> }).renderMermaid;
  void renderMermaid?.(document);
});
