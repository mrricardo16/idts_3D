using System.ComponentModel.DataAnnotations;

namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record ChangeVersionStatusRequest(
    [param: StringLength(500)] string? Remark);
