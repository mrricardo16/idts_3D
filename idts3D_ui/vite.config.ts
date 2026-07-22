import { defineConfig, type Plugin } from "vite";
import { resolve } from "node:path";
import vue from "@vitejs/plugin-vue";
import {
  createPocCacheMiddleware,
  createPocVersionedAsset,
  type PocCacheMiddleware,
} from "./pocCacheMiddleware";
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

let pocCacheMiddleware: PocCacheMiddleware | undefined;

const pocCachePlugin: Plugin = {
  name: "poc-versioned-model-cache",
  async configResolved() {
    const [lifter, v1, v2] = await Promise.all([
      createPocVersionedAsset({
        asset: "lifter",
        sourcePath: resolve(__dirname, "public/models/lifter.glb"),
        contentType: "model/gltf-binary",
      }),
      createPocVersionedAsset({
        asset: "lifter",
        sourcePath: resolve(__dirname, "public/poc-3dtiles/minimal/minimal.gltf"),
        contentType: "model/gltf+json",
      }),
      createPocVersionedAsset({
        asset: "lifter",
        sourcePath: resolve(__dirname, "public/poc-3dtiles/poc-lifter/poc-lifter.gltf"),
        contentType: "model/gltf+json",
      }),
    ]);
    pocCacheMiddleware = createPocCacheMiddleware({
      assets: [lifter, v1, v2],
      testVersionManifests: { v1: v1.manifest, v2: v2.manifest },
    });
  },
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!pocCacheMiddleware) {
        next();
        return;
      }
      pocCacheMiddleware(request, response, next);
    });
  },
  configurePreviewServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!pocCacheMiddleware) {
        next();
        return;
      }
      pocCacheMiddleware(request, response, next);
    });
  },
};

export default defineConfig({
  plugins: [vue(), pocStrict404Plugin, pocCachePlugin],
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, "index.html"),
        "poc-3dtiles": resolve(__dirname, "poc-3dtiles.html"),
      },
    },
  },
});
