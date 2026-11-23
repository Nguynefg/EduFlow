// Cấu hình Tailwind: font chữ và bảng màu của EduFlow
tailwind.config = {
  darkMode: "class",
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Plus Jakarta Sans"', "sans-serif"],
        mono: ['"JetBrains Mono"', "monospace"],
      },
      colors: {
        // Màu chính: xanh hoàng gia (thay cho blue mặc định của Tailwind)
        blue: {
          50: "#eef3ff",
          100: "#dce6ff",
          200: "#bfd0ff",
          300: "#93b0ff",
          400: "#6088fb",
          500: "#3b63f2",
          600: "#2747e0",
          700: "#1f37c0",
          800: "#1f309b",
          900: "#1f2d7a",
          950: "#151c4a",
        },
        // Xanh navy đậm cho nền tối, ảnh bìa khóa học
        navy: {
          800: "#14205a",
          900: "#0d1640",
          950: "#080d29",
        },
        udemy: {
          dark: "#0f172a",
        },
      },
    },
  },
};
