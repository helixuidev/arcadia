/**
 * HelixUI Theme JS Interop
 * Handles system color scheme preference detection and persistence.
 */

/**
 * Returns the user's preferred color scheme from the OS.
 * @returns {"light" | "dark"}
 */
export function getSystemPreference() {
  return window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
}

/**
 * Registers a callback that fires when the system color scheme changes.
 * Returns a function to unregister the listener.
 * @param {DotNetObjectReference} dotNetRef - .NET object reference with InvokeThemeChanged method.
 * @returns {number} Listener ID for cleanup.
 */
let _listenerId = 0;
const _listeners = new Map();

export function onSystemPreferenceChanged(dotNetRef) {
  const id = ++_listenerId;
  const mql = window.matchMedia("(prefers-color-scheme: dark)");
  const handler = (e) => {
    dotNetRef.invokeMethodAsync("InvokeThemeChanged", e.matches ? "dark" : "light");
  };
  mql.addEventListener("change", handler);
  _listeners.set(id, { mql, handler });
  return id;
}

/**
 * Removes a system preference change listener.
 * @param {number} listenerId - The ID returned by onSystemPreferenceChanged.
 */
export function removeListener(listenerId) {
  const entry = _listeners.get(listenerId);
  if (entry) {
    entry.mql.removeEventListener("change", entry.handler);
    _listeners.delete(listenerId);
  }
}

/**
 * Saves the selected theme name to localStorage.
 * @param {string} themeName
 */
export function savePreference(themeName) {
  try {
    localStorage.setItem("helix-theme", themeName);
  } catch {
    // localStorage unavailable (SSR, private browsing) — silently ignore
  }
}

/**
 * Loads the saved theme name from localStorage.
 * @returns {string | null}
 */
export function loadPreference() {
  try {
    return localStorage.getItem("helix-theme");
  } catch {
    return null;
  }
}
