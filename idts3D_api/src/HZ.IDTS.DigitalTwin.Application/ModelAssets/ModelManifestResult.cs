using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed record ModelManifestResult(
    HttpStatusCode StatusCode,
    ApiResponse<ModelManifestResponse> Response);
