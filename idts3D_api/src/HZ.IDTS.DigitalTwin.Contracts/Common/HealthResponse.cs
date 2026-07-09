namespace HZ.IDTS.DigitalTwin.Contracts.Common;

public sealed record HealthResponse(string Status, DateTimeOffset ServerTimeUtc);
