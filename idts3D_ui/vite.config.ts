import { defineConfig } from "vite";
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
});
