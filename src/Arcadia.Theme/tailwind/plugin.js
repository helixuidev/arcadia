/**
 * Arcadia Tailwind CSS 4.x Plugin
 *
 * Maps Arcadia design tokens to Tailwind utilities so you can use
 * classes like `bg-arcadia-primary`, `text-arcadia-muted`, `shadow-arcadia-md`, etc.
 *
 * Usage in tailwind.config.js:
 *   import helixPlugin from 'helixui/tailwind/plugin'
 *   export default { plugins: [helixPlugin] }
 */

const plugin = require("tailwindcss/plugin");

module.exports = plugin(function ({ addBase }) {
  // The actual token values come from the CSS files.
  // This plugin simply ensures Tailwind knows about Arcadia's CSS custom properties.
  addBase({
    ":root": {
      fontFamily: "var(--arcadia-font-sans)",
    },
  });
}, {
  theme: {
    extend: {
      colors: {
        helix: {
          primary: {
            DEFAULT: "var(--arcadia-color-primary)",
            hover: "var(--arcadia-color-primary-hover)",
            active: "var(--arcadia-color-primary-active)",
            subtle: "var(--arcadia-color-primary-subtle)",
          },
          secondary: {
            DEFAULT: "var(--arcadia-color-secondary)",
            hover: "var(--arcadia-color-secondary-hover)",
            active: "var(--arcadia-color-secondary-active)",
            subtle: "var(--arcadia-color-secondary-subtle)",
          },
          surface: {
            DEFAULT: "var(--arcadia-color-surface)",
            raised: "var(--arcadia-color-surface-raised)",
            overlay: "var(--arcadia-color-surface-overlay)",
            sunken: "var(--arcadia-color-surface-sunken)",
          },
          text: {
            DEFAULT: "var(--arcadia-color-text)",
            muted: "var(--arcadia-color-text-muted)",
            inverse: "var(--arcadia-color-text-inverse)",
            disabled: "var(--arcadia-color-text-disabled)",
          },
          border: {
            DEFAULT: "var(--arcadia-color-border)",
            hover: "var(--arcadia-color-border-hover)",
            focus: "var(--arcadia-color-border-focus)",
          },
          danger: {
            DEFAULT: "var(--arcadia-color-danger)",
            hover: "var(--arcadia-color-danger-hover)",
            subtle: "var(--arcadia-color-danger-subtle)",
          },
          warning: {
            DEFAULT: "var(--arcadia-color-warning)",
            hover: "var(--arcadia-color-warning-hover)",
            subtle: "var(--arcadia-color-warning-subtle)",
          },
          success: {
            DEFAULT: "var(--arcadia-color-success)",
            hover: "var(--arcadia-color-success-hover)",
            subtle: "var(--arcadia-color-success-subtle)",
          },
          info: {
            DEFAULT: "var(--arcadia-color-info)",
            hover: "var(--arcadia-color-info-hover)",
            subtle: "var(--arcadia-color-info-subtle)",
          },
        },
      },
      spacing: {
        "arcadia-xs": "var(--arcadia-spacing-xs)",
        "arcadia-sm": "var(--arcadia-spacing-sm)",
        "arcadia-md": "var(--arcadia-spacing-md)",
        "arcadia-lg": "var(--arcadia-spacing-lg)",
        "arcadia-xl": "var(--arcadia-spacing-xl)",
      },
      fontFamily: {
        "arcadia-sans": "var(--arcadia-font-sans)",
        "arcadia-mono": "var(--arcadia-font-mono)",
      },
      fontSize: {
        "arcadia-xs": "var(--arcadia-text-xs)",
        "arcadia-sm": "var(--arcadia-text-sm)",
        "arcadia-base": "var(--arcadia-text-base)",
        "arcadia-lg": "var(--arcadia-text-lg)",
        "arcadia-xl": "var(--arcadia-text-xl)",
        "arcadia-2xl": "var(--arcadia-text-2xl)",
        "arcadia-3xl": "var(--arcadia-text-3xl)",
        "arcadia-4xl": "var(--arcadia-text-4xl)",
      },
      boxShadow: {
        "arcadia-xs": "var(--arcadia-shadow-xs)",
        "arcadia-sm": "var(--arcadia-shadow-sm)",
        "arcadia-md": "var(--arcadia-shadow-md)",
        "arcadia-lg": "var(--arcadia-shadow-lg)",
        "arcadia-xl": "var(--arcadia-shadow-xl)",
      },
      borderRadius: {
        "arcadia-sm": "var(--arcadia-radius-sm)",
        "arcadia-md": "var(--arcadia-radius-md)",
        "arcadia-lg": "var(--arcadia-radius-lg)",
        "arcadia-xl": "var(--arcadia-radius-xl)",
        "arcadia-full": "var(--arcadia-radius-full)",
      },
      transitionDuration: {
        "arcadia-fast": "var(--arcadia-duration-fast)",
        "arcadia-normal": "var(--arcadia-duration-normal)",
        "arcadia-slow": "var(--arcadia-duration-slow)",
      },
      transitionTimingFunction: {
        "arcadia-default": "var(--arcadia-ease-default)",
        "arcadia-in": "var(--arcadia-ease-in)",
        "arcadia-out": "var(--arcadia-ease-out)",
      },
      zIndex: {
        "arcadia-dropdown": "var(--arcadia-z-dropdown)",
        "arcadia-sticky": "var(--arcadia-z-sticky)",
        "arcadia-overlay": "var(--arcadia-z-overlay)",
        "arcadia-modal": "var(--arcadia-z-modal)",
        "arcadia-popover": "var(--arcadia-z-popover)",
        "arcadia-toast": "var(--arcadia-z-toast)",
        "arcadia-tooltip": "var(--arcadia-z-tooltip)",
      },
      screens: {
        "arcadia-sm": "640px",
        "arcadia-md": "768px",
        "arcadia-lg": "1024px",
        "arcadia-xl": "1280px",
        "arcadia-2xl": "1536px",
      },
    },
  },
});
