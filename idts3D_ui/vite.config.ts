import { defineConfig } from "vite";
import { resolve } from "node:path";
import vue from "@vitejs/plugin-vue";
import { createPocStrict404Middleware } from "./src/poc/pocStrict404Middleware";

const pocStrict404Middleware = createPocStrict404Middleware();

const pocStrict404Plugin = {
  name: "poc-strict-404-fixture",
  configureServer(server: { middlewares: { use: typeof pocStrict404Middleware } }) {
    server.middlewares.use(pocStrict404Middleware);
  },
  configurePreviewServer(server: { middlewares: { use: typeof pocStrict404Middleware } }) {
    server.middlewares.use(pocStrict404Middleware);
  },
};

export default defineConfig({
  plugins: [vue(), pocStrict404Plugin],
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, "index.html"),
        "poc-3dtiles": resolve(__dirname, "poc-3dtiles.html"),
      },
    },
  },
});
