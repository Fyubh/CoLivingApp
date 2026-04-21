/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                ios: {
                    bg: '#F2F2F7',
                    blue: '#007AFF',
                    green: '#34C759',
                    red: '#FF3B30',
                    orange: '#FF9500',
                    purple: '#AF52DE',
                    text: '#000000',
                    muted: '#8E8E93'
                }
            },
            backgroundColor: {
                'ios-card': 'rgba(255, 255, 255, 0.65)',
                'ios-card-solid': '#FFFFFF',
            },
            backdropBlur: {
                'ios': '20px',
            },
            borderRadius: {
                'ios': '28px',
                'ios-btn': '20px',
            },
            boxShadow: {
                'ios-card': '0 8px 32px rgba(0,0,0,0.04)',
                'ios-btn': '0 4px 15px rgba(0, 122, 255, 0.4)',
            }
        },
    },
    plugins: [],
}