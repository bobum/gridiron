/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'gridiron': {
          'primary': '#1e3a8a',    // Deep blue for primary actions
          'secondary': '#059669',  // Green for success/wins
          'accent': '#dc2626',     // Red for important highlights
        }
      }
    },
  },
  plugins: [],
}
