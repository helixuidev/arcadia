/**
 * HelixUI Tailwind CSS 4.x Plugin
 *
 * Maps HelixUI design tokens to Tailwind utilities so you can use
 * classes like `bg-helix-primary`, `text-helix-muted`, `shadow-helix-md`, etc.
 *
 * Usage in tailwind.config.js:
 *   import helixPlugin from 'helixui/tailwind/plugin'
 *   export default { plugins: [helixPlugin] }
 */

const plugin = require("tailwindcss/plugin");

module.exports = plugin(function ({ addBase }) {
  // The actual token values come from the CSS files.
  // This plugin simply ensures Tailwind knows about HelixUI's CSS custom properties.
  addBase({
    ":root": {
      fontFamily: "var(--helix-font-sans)",
    },
  });
}, {
  theme: {
    extend: {
      colors: {
        helix: {
          primary: {
            DEFAULT: "var(--helix-color-primary)",
            hover: "var(--helix-color-primary-hover)",
            active: "var(--helix-color-primary-active)",
            subtle: "var(--helix-color-primary-subtle)",
          },
          secondary: {
            DEFAULT: "var(--helix-color-secondary)",
            hover: "var(--helix-color-secondary-hover)",
            active: "var(--helix-color-secondary-active)",
            subtle: "var(--helix-color-secondary-subtle)",
          },
          surface: {
            DEFAULT: "var(--helix-color-surface)",
            raised: "var(--helix-color-surface-raised)",
            overlay: "var(--helix-color-surface-overlay)",
            sunken: "var(--helix-color-surface-sunken)",
          },
          text: {
            DEFAULT: "var(--helix-color-text)",
            muted: "var(--helix-color-text-muted)",
            inverse: "var(--helix-color-text-inverse)",
            disabled: "var(--helix-color-text-disabled)",
          },
          border: {
            DEFAULT: "var(--helix-color-border)",
            hover: "var(--helix-color-border-hover)",
            focus: "var(--helix-color-border-focus)",
          },
          danger: {
            DEFAULT: "var(--helix-color-danger)",
            hover: "var(--helix-color-danger-hover)",
            subtle: "var(--helix-color-danger-subtle)",
          },
          warning: {
            DEFAULT: "var(--helix-color-warning)",
            hover: "var(--helix-color-warning-hover)",
            subtle: "var(--helix-color-warning-subtle)",
          },
          success: {
            DEFAULT: "var(--helix-color-success)",
            hover: "var(--helix-color-success-hover)",
            subtle: "var(--helix-color-success-subtle)",
          },
          info: {
            DEFAULT: "var(--helix-color-info)",
            hover: "var(--helix-color-info-hover)",
            subtle: "var(--helix-color-info-subtle)",
          },
        },
      },
      spacing: {
        "helix-xs": "var(--helix-spacing-xs)",
        "helix-sm": "var(--helix-spacing-sm)",
        "helix-md": "var(--helix-spacing-md)",
        "helix-lg": "var(--helix-spacing-lg)",
        "helix-xl": "var(--helix-spacing-xl)",
      },
      fontFamily: {
        "helix-sans": "var(--helix-font-sans)",
        "helix-mono": "var(--helix-font-mono)",
      },
      fontSize: {
        "helix-xs": "var(--helix-text-xs)",
        "helix-sm": "var(--helix-text-sm)",
        "helix-base": "var(--helix-text-base)",
        "helix-lg": "var(--helix-text-lg)",
        "helix-xl": "var(--helix-text-xl)",
        "helix-2xl": "var(--helix-text-2xl)",
        "helix-3xl": "var(--helix-text-3xl)",
        "helix-4xl": "var(--helix-text-4xl)",
      },
      boxShadow: {
        "helix-xs": "var(--helix-shadow-xs)",
        "helix-sm": "var(--helix-shadow-sm)",
        "helix-md": "var(--helix-shadow-md)",
        "helix-lg": "var(--helix-shadow-lg)",
        "helix-xl": "var(--helix-shadow-xl)",
      },
      borderRadius: {
        "helix-sm": "var(--helix-radius-sm)",
        "helix-md": "var(--helix-radius-md)",
        "helix-lg": "var(--helix-radius-lg)",
        "helix-xl": "var(--helix-radius-xl)",
        "helix-full": "var(--helix-radius-full)",
      },
      transitionDuration: {
        "helix-fast": "var(--helix-duration-fast)",
        "helix-normal": "var(--helix-duration-normal)",
        "helix-slow": "var(--helix-duration-slow)",
      },
      transitionTimingFunction: {
        "helix-default": "var(--helix-ease-default)",
        "helix-in": "var(--helix-ease-in)",
        "helix-out": "var(--helix-ease-out)",
      },
      zIndex: {
        "helix-dropdown": "var(--helix-z-dropdown)",
        "helix-sticky": "var(--helix-z-sticky)",
        "helix-overlay": "var(--helix-z-overlay)",
        "helix-modal": "var(--helix-z-modal)",
        "helix-popover": "var(--helix-z-popover)",
        "helix-toast": "var(--helix-z-toast)",
        "helix-tooltip": "var(--helix-z-tooltip)",
      },
      screens: {
        "helix-sm": "640px",
        "helix-md": "768px",
        "helix-lg": "1024px",
        "helix-xl": "1280px",
        "helix-2xl": "1536px",
      },
    },
  },
});
