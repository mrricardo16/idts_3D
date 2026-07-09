namespace HZ.IDTS.DigitalTwin.Contracts.Common;

public sealed record ApiResponse<T>(
    bool Success,
    string Code,
    string Message,
    T? Data,
    IReadOnlyList<ApiErrorItem> Errors)
{
    public static ApiResponse<T> Ok(T? data, string message = "")
    {
        return new ApiResponse<T>(
            true,
            ErrorCode.Ok,
            message,
            data,
            Array.Empty<ApiErrorItem>());
    }

    public static ApiResponse<T> Failure(
        string code,
        string message,
        IReadOnlyList<ApiErrorItem>? errors)
    {
        return new ApiResponse<T>(
            false,
            code,
            message,
            default,
            errors ?? Array.Empty<ApiErrorItem>());
    }
}
