import { TilesRenderer } from "3d-tiles-renderer";
import { Group, Mesh, Vector3 } from "three";
import { GLTFLoader } from "three/examples/jsm/loaders/GLTFLoader.js";
import { CameraManager } from "../engine/CameraManager";
import { ControlsManager } from "../engine/ControlsManager";
import { FallbackFactory } from "../engine/FallbackFactory";
import { InteractionManager } from "../engine/InteractionManager";
import { LODModelLoader } from "../engine/LODModelLoader";
import { RendererManager } from "../engine/RendererManager";
import { ResourceDisposer } from "../engine/ResourceDisposer";
import { SceneManager } from "../engine/SceneManager";
import { StatusManager } from "../engine/StatusManager";
import {
  createPocRuntimeDiagnostics,
  type PocRuntimeDiagnostics,
} from "./pocDiagnostics";
import {
  classifyPocTilesFailure,
  createPocTilesState,
  defaultPocTilesConfig,
  transitionPocTilesState,
  type PocTilesState,
} from "./pocTilesRuntime";
import { setPocObjectWorldZ } from "./pocWorldZ";
import { collectPocGlbNodeRecords } from "./pocGlbNodes";
import { getPocCameraPreset, type PocCameraPresetName } from "./pocCameraPresets";
import {
  resolvePocPerformanceScenario,
  type PocPerformanceScenarioConfig,
} from "./pocPerformanceScenario";

const syntheticPocLifterUrl = "/poc-3dtiles/poc-lifter/poc-lifter.gltf";

export interface PocTilesSceneCallbacks {
  onTilesStateChange: (state: PocTilesState) => void;
  onGlbStatusChange: (message: string) => void;
  onGlbSelectionChange: (name: string | undefined) => void;
  onDiagnosticsChange: (diagnostics: PocRuntimeDiagnostics) => void;
}

export interface PocPerformanceSnapshot {
  scenario: PocPerformanceScenarioConfig["name"];
  diagnostics: PocRuntimeDiagnostics;
  renderer: {
    calls: number;
    triangles: number;
    points: number;
    lines: number;
    geometries: number;
    textures: number;
  };
  sceneObjects: number;
  glbMeshes: number;
  tileMeshes: number;
  materialCount: number;
  renderLoopStarts: number;
  controlsUpdates: number;
  diagnosticsPublishes: number;
}

export interface PocTilesSceneOptions {
  performanceScenario?: PocPerformanceScenarioConfig;
}

export class PocTilesScene {
  private readonly sceneManager = new SceneManager();
  private readonly cameraManager: CameraManager;
  private readonly rendererManager: RendererManager;
  private readonly controlsManager: ControlsManager;
  private readonly modelLoader = new LODModelLoader();
  private readonly syntheticPocLoader = new GLTFLoader();
  private readonly statusManager = new StatusManager();
  private readonly fallbackFactory = new FallbackFactory();
  private interactionManager?: InteractionManager;
  private glbRoot?: Group;
  private tiles?: TilesRenderer;
  private animationFrameId = 0;
  private tilesLoadGeneration = 0;
  private disposed = false;
  private tilesState = createPocTilesState();
  private diagnostics: PocRuntimeDiagnostics;
  private loadStartedAt = 0;
  private frameCount = 0;
  private fpsSampleStartedAt = 0;
  private lastDiagnosticsPublishedAt = 0;
  private renderLoopStarts = 0;
  private controlsUpdates = 0;
  private diagnosticsPublishes = 0;
  private readonly performanceScenario: PocPerformanceScenarioConfig;

  constructor(
    private readonly container: HTMLElement,
    private readonly callbacks: PocTilesSceneCallbacks,
    options: PocTilesSceneOptions = {},
  ) {
    this.performanceScenario = options.performanceScenario ?? resolvePocPerformanceScenario(null)!;
    this.cameraManager = new CameraManager(container);
    this.rendererManager = new RendererManager(container);
    this.controlsManager = new ControlsManager(
      this.cameraManager.camera,
      this.rendererManager.renderer.domElement,
      () => undefined,
    );
    this.diagnostics = createPocRuntimeDiagnostics({
      browser: typeof navigator === "undefined" ? "unknown" : navigator.appName,
      userAgent: typeof navigator === "undefined" ? "unknown" : navigator.userAgent,
      webglAvailable: Boolean(this.rendererManager.renderer.getContext()),
      canvasCount: this.container.querySelectorAll("canvas").length,
      rendererCount: 1,
      glbUrl: "/models/lifter.glb",
    });
    window.addEventListener("resize", this.handleResize);
  }

  async init(): Promise<void> {
    this.publishDiagnostics();
    if (this.performanceScenario.glb === "real") {
      await this.loadGlbLayer();
      if (this.disposed) {
        return;
      }
    } else if (this.performanceScenario.glb === "synthetic") {
      await this.loadSyntheticPocLifter();
    } else {
      this.callbacks.onGlbStatusChange("GLB 已按 POC 性能场景禁用。");
    }

    this.applyCameraPreset("factory-exterior");
    if (this.performanceScenario.loadTiles) {
      this.loadTiles(defaultPocTilesConfig.tilesetUrl);
    }
    this.startRenderLoop();
  }

  loadFailureFixture(): void {
    this.loadTiles(defaultPocTilesConfig.missingTilesetUrl);
  }

  loadChildFailureFixture(): void {
    this.loadTiles(defaultPocTilesConfig.childMissingTilesetUrl);
  }

  loadInvalidJsonFixture(): void {
    this.loadTiles(defaultPocTilesConfig.invalidJsonTilesetUrl);
  }

  reloadLocalFixture(): void {
    this.loadTiles(defaultPocTilesConfig.tilesetUrl);
  }

  async loadSyntheticPocLifter(): Promise<void> {
    try {
      const loaded = await this.syntheticPocLoader.loadAsync(syntheticPocLifterUrl);
      if (this.disposed) {
        ResourceDisposer.disposeObject3D(loaded.scene);
        return;
      }

      loaded.scene.name = "poc-lifter-root";
      loaded.scene.userData.pocSyntheticFixture = true;
      this.attachGlbRoot(
        loaded.scene,
        syntheticPocLifterUrl,
        "已加载合成 POC worldZ 夹具：/poc-3dtiles/poc-lifter/poc-lifter.gltf",
      );
    } catch (error) {
      this.callbacks.onGlbStatusChange(`合成 POC 夹具加载失败：${String(error)}`);
    }
  }

  setSyntheticPlatformWorldZ(targetZ: number): number | undefined {
    if (!this.glbRoot?.userData.pocSyntheticFixture) {
      return undefined;
    }

    const platform = this.glbRoot.getObjectByName("lifter-platform");
    if (!platform) {
      return undefined;
    }

    const actualZ = setPocObjectWorldZ(platform, targetZ);
    this.updateDiagnostics({ worldZ: actualZ });
    return actualZ;
  }

  applyCameraPreset(name: PocCameraPresetName): PocCameraPresetName {
    const preset = getPocCameraPreset(name);
    this.controlsManager.setView(
      new Vector3(...preset.position),
      new Vector3(...preset.target),
    );
    return name;
  }

  getDiagnostics(): PocRuntimeDiagnostics {
    return {
      ...this.diagnostics,
      networkErrors: [...this.diagnostics.networkErrors],
      parseErrors: [...this.diagnostics.parseErrors],
      lifecycle: [...this.diagnostics.lifecycle],
    };
  }

  getPerformanceSnapshot(): PocPerformanceSnapshot {
    const materialIds = new Set<string>();
    const countMeshes = (root: Group | undefined): number => {
      let meshes = 0;
      root?.traverse((object) => {
        if (object instanceof Mesh) {
          meshes += 1;
          const materials = Array.isArray(object.material) ? object.material : [object.material];
          materials.forEach((material) => materialIds.add(material.uuid));
        }
      });
      return meshes;
    };
    let sceneObjects = 0;
    this.sceneManager.scene.traverse(() => {
      sceneObjects += 1;
    });
    const info = this.rendererManager.renderer.info;

    return {
      scenario: this.performanceScenario.name,
      diagnostics: this.getDiagnostics(),
      renderer: {
        calls: info.render.calls,
        triangles: info.render.triangles,
        points: info.render.points,
        lines: info.render.lines,
        geometries: info.memory.geometries,
        textures: info.memory.textures,
      },
      sceneObjects,
      glbMeshes: countMeshes(this.glbRoot),
      tileMeshes: countMeshes(this.tiles?.group),
      materialCount: materialIds.size,
      renderLoopStarts: this.renderLoopStarts,
      controlsUpdates: this.controlsUpdates,
      diagnosticsPublishes: this.diagnosticsPublishes,
    };
  }

  dispose(): void {
    if (this.disposed) {
      return;
    }

    this.disposed = true;
    this.tilesLoadGeneration += 1;
    this.updateTilesState({ type: "dispose" });
    window.removeEventListener("resize", this.handleResize);
    window.cancelAnimationFrame(this.animationFrameId);
    this.animationFrameId = 0;
    this.interactionManager?.clear();
    this.interactionManager = undefined;

    if (this.tiles) {
      this.tiles.group.removeFromParent();
      this.tiles.dispose();
      this.tiles = undefined;
    }

    if (this.glbRoot) {
      ResourceDisposer.disposeObject3D(this.glbRoot, { removeFromParent: true });
      this.glbRoot = undefined;
    }

    this.controlsManager.dispose();
    this.sceneManager.dispose();
    this.rendererManager.dispose();
    this.updateDiagnostics({
      animationLoopActive: false,
      canvasCount: 0,
      rendererCount: 0,
      loadedTiles: 0,
      activeTiles: 0,
      drawCalls: 0,
    });
  }

  private async loadGlbLayer(): Promise<void> {
    const devices = this.statusManager.getDevices();
    const loaded = await this.modelLoader.loadDefault(devices);
    if (this.disposed) {
      if (loaded.root) {
        ResourceDisposer.disposeObject3D(loaded.root);
      }
      return;
    }

    const root = loaded.root ?? this.fallbackFactory.create(devices);
    this.attachGlbRoot(
      root,
      loaded.url ?? "POC fallback geometry",
      loaded.root
        ? `GLB 已加载：${loaded.url ?? "未记录路径"}`
        : `GLB 加载失败，已使用当前工程几何体 fallback：${loaded.message}`,
    );
  }

  private attachGlbRoot(root: Group, url: string, message: string): void {
    this.interactionManager?.clear();
    this.interactionManager = undefined;
    if (this.glbRoot) {
      ResourceDisposer.disposeObject3D(this.glbRoot, { removeFromParent: true });
    }

    this.glbRoot = root;
    const glbNodes = collectPocGlbNodeRecords(root);
    this.sceneManager.scene.add(root);
    this.interactionManager = new InteractionManager(
      this.rendererManager.renderer.domElement,
      this.cameraManager.camera,
      root,
      (uuid) => {
        const selected = this.glbRoot?.getObjectByProperty("uuid", uuid);
        const name = selected?.name || undefined;
        this.callbacks.onGlbSelectionChange(name);
        this.updateDiagnostics({ selectedObject: name ?? null });
      },
      [root],
    );
    this.callbacks.onGlbStatusChange(message);
    this.callbacks.onGlbSelectionChange(undefined);
    this.updateDiagnostics({ glbUrl: url, selectedObject: null, worldZ: null, glbNodes });
  }

  private loadTiles(url: string): void {
    if (this.disposed) {
      return;
    }

    this.tilesLoadGeneration += 1;
    const generation = this.tilesLoadGeneration;
    if (this.tiles) {
      this.tiles.group.removeFromParent();
      this.tiles.dispose();
    }

    this.loadStartedAt = performance.now();
    this.updateDiagnostics({
      tilesetUrl: url,
      loadStartTime: new Date().toISOString(),
      firstVisibleTime: null,
      readyTime: null,
      loadDurationMs: null,
      loadedTiles: 0,
      activeTiles: 0,
    });
    this.updateTilesState({ type: "start" });
    const tiles = new TilesRenderer(url);
    tiles.setCamera(this.cameraManager.camera);
    tiles.setResolutionFromRenderer(this.cameraManager.camera, this.rendererManager.renderer);
    tiles.addEventListener("load-root-tileset", () => {
      if (!this.disposed && generation === this.tilesLoadGeneration) {
        this.updateTilesState({ type: "rootLoaded" });
      }
    });
    tiles.addEventListener("load-model", () => {
      if (!this.disposed && generation === this.tilesLoadGeneration) {
        const readyAt = new Date().toISOString();
        this.updateDiagnostics({
          firstVisibleTime: this.diagnostics.firstVisibleTime ?? readyAt,
          readyTime: readyAt,
          loadDurationMs: performance.now() - this.loadStartedAt,
        });
        this.updateTilesState({ type: "contentLoaded" });
      }
    });
    tiles.addEventListener("load-error", ({ error, url: failedUrl }) => {
      if (!this.disposed && generation === this.tilesLoadGeneration) {
        const failure = classifyPocTilesFailure(
          String(failedUrl),
          error instanceof Error ? error : new Error(String(error)),
        );
        this.updateDiagnostics({
          networkErrors:
            failure.kind === "http" || failure.kind === "child"
              ? [...this.diagnostics.networkErrors, failure.message]
              : this.diagnostics.networkErrors,
          parseErrors:
            failure.kind === "parse"
              ? [...this.diagnostics.parseErrors, failure.message]
              : this.diagnostics.parseErrors,
        });
        this.updateTilesState({ type: "fail", ...failure });
      }
    });

    this.tiles = tiles;
    this.sceneManager.scene.add(tiles.group);
  }

  private updateTilesState(event: Parameters<typeof transitionPocTilesState>[1]): void {
    this.tilesState = transitionPocTilesState(this.tilesState, event);
    this.callbacks.onTilesStateChange(this.tilesState);
    this.updateDiagnostics({ tilesPhase: this.tilesState.phase });
  }

  private readonly handleResize = (): void => {
    this.cameraManager.resize(this.container);
    this.rendererManager.resize(this.container);
    this.tiles?.setResolutionFromRenderer(this.cameraManager.camera, this.rendererManager.renderer);
    this.updateDiagnostics({ canvasCount: this.container.querySelectorAll("canvas").length });
  };

  private startRenderLoop(): void {
    if (this.animationFrameId || this.disposed) {
      return;
    }

    this.fpsSampleStartedAt = performance.now();
    this.renderLoopStarts += 1;
    this.updateDiagnostics({ animationLoopActive: true });
    const render = (): void => {
      this.animationFrameId = 0;
      if (this.disposed) {
        return;
      }

      this.controlsManager.update();
      this.controlsUpdates += 1;
      this.cameraManager.camera.updateMatrixWorld();
      this.tiles?.update();
      this.rendererManager.render(this.sceneManager.scene, this.cameraManager.camera);
      this.recordFrameDiagnostics();
      this.animationFrameId = window.requestAnimationFrame(render);
    };

    this.animationFrameId = window.requestAnimationFrame(render);
  }

  private recordFrameDiagnostics(): void {
    this.frameCount += 1;
    const now = performance.now();
    const elapsed = now - this.fpsSampleStartedAt;
    if (elapsed < 1000) {
      return;
    }

    const performanceWithMemory = performance as Performance & {
      memory?: { usedJSHeapSize?: number };
    };
    this.updateDiagnostics({
      fps: (this.frameCount * 1000) / elapsed,
      loadedTiles: this.tiles?.visibleTiles.size ?? 0,
      activeTiles: this.tiles?.activeTiles.size ?? 0,
      drawCalls: this.rendererManager.renderer.info.render.calls,
      memoryMb:
        typeof performanceWithMemory.memory?.usedJSHeapSize === "number"
          ? performanceWithMemory.memory.usedJSHeapSize / (1024 * 1024)
          : null,
      canvasCount: this.container.querySelectorAll("canvas").length,
    });
    this.frameCount = 0;
    this.fpsSampleStartedAt = now;
  }

  private updateDiagnostics(values: Partial<PocRuntimeDiagnostics>): void {
    this.diagnostics = { ...this.diagnostics, ...values };
    const now = performance.now();
    if (now - this.lastDiagnosticsPublishedAt >= this.performanceScenario.diagnosticsPublishIntervalMs) {
      this.publishDiagnostics();
    }
  }

  private publishDiagnostics(): void {
    this.lastDiagnosticsPublishedAt = performance.now();
    this.diagnosticsPublishes += 1;
    this.callbacks.onDiagnosticsChange(this.getDiagnostics());
  }
}
