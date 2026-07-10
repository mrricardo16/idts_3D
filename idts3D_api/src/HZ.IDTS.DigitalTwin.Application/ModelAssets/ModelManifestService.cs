using System.Net;
using System.Text.Json;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed class ModelManifestService : IModelManifestService
{
    private const string MonitorMode = "monitor";
    private const string EditMode = "edit";

    private static readonly ManifestVector3Response DefaultPosition = new(0, 0, 0);
    private static readonly ManifestVector3Response DefaultRotationDeg = new(0, 0, 0);
    private static readonly ManifestVector3Response DefaultScale = new(1, 1, 1);

    private readonly IModelAssetRepository _repository;

    public ModelManifestService(IModelAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<ModelManifestResult> GetManifestAsync(
        GetModelManifestRequest request,
        CancellationToken cancellationToken)
    {
        var mode = NormalizeMode(request.Mode);
        if (mode is null)
        {
            return Failure(
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationFailed,
                "mode 参数无效。",
                new[] { new ApiErrorItem("mode", "只允许 monitor 或 edit。") });
        }

        if (!await _repository.AssetExistsByIdAsync(request.AssetId, cancellationToken))
        {
            return Failure(
                HttpStatusCode.NotFound,
                ErrorCode.AssetNotFound,
                "model asset 不存在。",
                new[] { new ApiErrorItem("assetId", $"未找到 assetId={request.AssetId}。") });
        }

        var manifestData = await _repository.GetModelManifestAsync(
            request.AssetId,
            request.VersionId,
            mode == MonitorMode,
            cancellationToken);

        if (manifestData is null)
        {
            var field = request.VersionId.HasValue ? "versionId" : "assetId";
            var message = request.VersionId.HasValue
                ? $"未找到 versionId={request.VersionId.Value}。"
                : "未找到可用 asset version。";

            return Failure(
                HttpStatusCode.NotFound,
                ErrorCode.VersionNotFound,
                "asset version 不存在。",
                new[] { new ApiErrorItem(field, message) });
        }

        if (!IsVersionAllowed(mode, manifestData.VersionStatus))
        {
            return Failure(
                HttpStatusCode.Conflict,
                ErrorCode.VersionStatusInvalid,
                mode == MonitorMode
                    ? "monitor 模式只能读取 Published 版本。"
                    : "edit 模式只能读取 Draft / Ready / Published 版本。",
                new[] { new ApiErrorItem("versionStatus", $"当前版本为 {manifestData.VersionStatus}。") });
        }

        var response = new ModelManifestResponse(
            manifestData.AssetId,
            manifestData.VersionId,
            manifestData.AssetCode,
            manifestData.AssetName,
            manifestData.VersionStatus.ToString(),
            BuildLevels(manifestData.Variants),
            ReadTransformOrDefault(manifestData.ManifestJson),
            manifestData.MovableParts.Select(ToMovablePartResponse).ToList());

        return new ModelManifestResult(
            HttpStatusCode.OK,
            ApiResponse<ModelManifestResponse>.Ok(response));
    }

    private static string? NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return MonitorMode;
        }

        var normalizedMode = mode.Trim();
        if (string.Equals(normalizedMode, MonitorMode, StringComparison.OrdinalIgnoreCase))
        {
            return MonitorMode;
        }

        return string.Equals(normalizedMode, EditMode, StringComparison.OrdinalIgnoreCase)
            ? EditMode
            : null;
    }

    private static bool IsVersionAllowed(string mode, VersionStatus versionStatus)
    {
        return mode switch
        {
            MonitorMode => versionStatus == VersionStatus.Published,
            EditMode => versionStatus is VersionStatus.Draft or VersionStatus.Ready or VersionStatus.Published,
            _ => false
        };
    }

    private static ModelManifestLevelsResponse BuildLevels(
        IReadOnlyList<ModelManifestVariantData> variants)
    {
        return new ModelManifestLevelsResponse(
            FindVariantUrl(variants, VariantLevel.source),
            FindVariantUrl(variants, VariantLevel.high),
            FindVariantUrl(variants, VariantLevel.medium),
            FindVariantUrl(variants, VariantLevel.low),
            FindVariantUrl(variants, VariantLevel.proxy));
    }

    private static string? FindVariantUrl(
        IReadOnlyList<ModelManifestVariantData> variants,
        VariantLevel variantLevel)
    {
        return variants.FirstOrDefault(x => x.VariantLevel == variantLevel)?.FileUrl;
    }

    private static ModelTransformResponse ReadTransformOrDefault(string? manifestJson)
    {
        if (string.IsNullOrWhiteSpace(manifestJson))
        {
            return DefaultTransform();
        }

        try
        {
            using var document = JsonDocument.Parse(manifestJson);
            if (!document.RootElement.TryGetProperty("transform", out var transformElement))
            {
                return DefaultTransform();
            }

            return new ModelTransformResponse(
                ReadVectorOrDefault(transformElement, "position", DefaultPosition),
                ReadVectorOrDefault(transformElement, "rotationDeg", DefaultRotationDeg),
                ReadVectorOrDefault(transformElement, "scale", DefaultScale));
        }
        catch (JsonException)
        {
            return DefaultTransform();
        }
    }

    private static ModelTransformResponse DefaultTransform()
    {
        return new ModelTransformResponse(
            DefaultPosition,
            DefaultRotationDeg,
            DefaultScale);
    }

    private static ManifestVector3Response ReadVectorOrDefault(
        JsonElement parentElement,
        string propertyName,
        ManifestVector3Response defaultValue)
    {
        if (!parentElement.TryGetProperty(propertyName, out var vectorElement))
        {
            return defaultValue;
        }

        return new ManifestVector3Response(
            ReadDecimalOrDefault(vectorElement, "x", defaultValue.X),
            ReadDecimalOrDefault(vectorElement, "y", defaultValue.Y),
            ReadDecimalOrDefault(vectorElement, "z", defaultValue.Z));
    }

    private static decimal ReadDecimalOrDefault(
        JsonElement element,
        string propertyName,
        decimal defaultValue)
    {
        return element.TryGetProperty(propertyName, out var valueElement) &&
            valueElement.ValueKind == JsonValueKind.Number &&
            valueElement.TryGetDecimal(out var value)
            ? value
            : defaultValue;
    }

    private static ModelManifestMovablePartResponse ToMovablePartResponse(
        ModelManifestMovablePartData movablePart)
    {
        return new ModelManifestMovablePartResponse(
            movablePart.PartId,
            movablePart.PartCode,
            movablePart.BusinessName,
            movablePart.MotionType.ToString(),
            movablePart.AxisMode.ToString(),
            movablePart.Axis.ToString(),
            movablePart.Targets
                .Select(x => new ModelManifestMotionTargetResponse(
                    x.TargetId,
                    x.TargetCode,
                    x.TargetName,
                    x.TargetValue))
                .ToList());
    }

    private static ModelManifestResult Failure(
        HttpStatusCode statusCode,
        string code,
        string message,
        IReadOnlyList<ApiErrorItem> errors)
    {
        return new ModelManifestResult(
            statusCode,
            ApiResponse<ModelManifestResponse>.Failure(code, message, errors));
    }
}
