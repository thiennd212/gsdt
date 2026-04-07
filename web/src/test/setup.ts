import '@testing-library/jest-dom';

// jsdom does not implement scrollIntoView — polyfill for component tests
Element.prototype.scrollIntoView = () => {};

// jsdom does not implement ResizeObserver — polyfill for responsive components
globalThis.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
} as unknown as typeof globalThis.ResizeObserver;

// Ant Design requires window.matchMedia — jsdom does not provide it
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
});

// Suppress Ant Design warnings in test output
const originalError = console.error.bind(console);
beforeAll(() => {
  console.error = (...args: unknown[]) => {
    const msg = String(args[0] ?? '');
    // Suppress known Ant Design / React act() noise
    if (msg.includes('Warning:') && msg.includes('antd')) return;
    if (msg.includes('inside a test was not wrapped in act')) return;
    originalError(...args);
  };
});

afterAll(() => {
  console.error = originalError;
});
