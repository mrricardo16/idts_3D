using Microsoft.Extensions.Configuration;

namespace HZ.IDTS.DigitalTwin.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; init; } = string.Empty;

    public string PublicBaseUrl { get; init; } = "/assets";

    public IReadOnlySet<string> AllowedExtensions { get; init; } = new HashSet<string>(
        new[] { ".glb" },
        StringComparer.OrdinalIgnoreCase);

    public long MaxFileSizeBytes { get; init; } = 500L * 1024L * 1024L;

    public static FileStorageOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(SectionName);
        var allowedExtensions = section
            .GetSection("AllowedExtensions")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToArray();

        var maxFileSizeMb = ReadLong(section["MaxFileSizeMb"], 500L);

        return new FileStorageOptions
        {
            RootPath = section["RootPath"]?.Trim() ?? string.Empty,
            PublicBaseUrl = NormalizePublicBaseUrl(section["PublicBaseUrl"]),
            AllowedExtensions = new HashSet<string>(
                allowedExtensions.Length == 0 ? new[] { ".glb" } : allowedExtensions,
                StringComparer.OrdinalIgnoreCase),
            MaxFileSizeBytes = maxFileSizeMb * 1024L * 1024L
        };
    }

    private static long ReadLong(string? value, long defaultValue)
    {
        return long.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : defaultValue;
    }

    private static string NormalizePublicBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/assets";
        }

        var trimmed = value.Trim().TrimEnd('/');
        return trimmed.StartsWith("/", StringComparison.Ordinal)
            ? trimmed
            : "/" + trimmed;
    }
}
