# Scene Resource Manifest 设计

> 状态：**Draft for review**。这是非实施设计草案，不修改现有 API、DTO、TypeScript 或数据库。

## 1. 边界

Scene Manifest 描述一个场景如何组合资源；Model Manifest 描述一个 GLB 资产版本本身。正式 Scene Manifest 的目标结构是 baseLayers + devices：

- baseLayers：3D Tiles 等场景级静态底座资源与其放置关系。
- devices：GLB 动态设备实例、模型绑定和设备变换。

当前 scenes API 的 tilesets 空数组只是占位，不能当作本草案已经实施。

## 2. JSON 草案

~~~json
{
  "sceneId": "TBD",
  "schemaVersion": "TBD",
  "baseLayers": [
    {
      "layerId": "TBD",
      "kind": "3dtiles",
      "resourceUrl": "TBD",
      "transform": {
        "translation": ["TBD", "TBD", "TBD"],
        "rotation": "TBD",
        "scale": ["TBD", "TBD", "TBD"]
      },
      "status": "TBD"
    }
  ],
  "devices": [
    {
      "deviceId": "TBD",
      "manifestUrl": "TBD",
      "transform": "existing device transform contract"
    }
  ]
}
~~~

## 3. DTO 与 TypeScript 草案

候选 DTO / TypeScript 名称为 SceneResourceManifestDraft、BaseLayerDraft、DeviceManifestDraft、TransformDraft。字段、枚举、可空性、错误响应、版本兼容和旧 tilesets 的迁移策略均为 TBD，必须在 MVP-10A 前进行后端 DTO、API 契约、TypeScript interface 与 API Client 的一对一审查。

禁止在页面临时定义 Tiles 类型，禁止通过 device_instance / device_model_binding 表示 baseLayers。

## 4. 数据来源与映射候选

| 领域 | 候选职责 | 状态 |
|---|---|---|
| model_asset / asset_version | 资源身份、文件定位、版本与发布能力 | 可研究，未授权实施 |
| scene_resource 或 scene_layer | 静态底座资源和场景放置关系 | 可研究，表名和字段未冻结 |
| device_instance / device_model_binding | GLB 动态设备与模型绑定 | 既有职责，不可伪装静态底座 |

POC 使用受控本地配置或测试数据，不落库。正式数据库映射、Migration、权限、发布和回滚策略必须另行审核。

## 5. 版本与兼容

- schemaVersion、向后兼容窗口、旧 tilesets 的读取策略均为 TBD。
- 在正式实现前，旧客户端继续消费当前 devices 契约；不得借本草案改变运行响应。
- MVP-10A 必须把兼容选择、回归范围和回滚方案写入实施任务输出。

