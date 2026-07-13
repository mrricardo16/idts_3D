using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Contracts.MovableParts;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests;

public sealed class MovablePartApiTests
{
    [Fact]
    public async Task Get_BindsQueryAndReturnsActiveResponse()
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        var response = await client.GetAsync("/api/model-assets/101/versions/202/movable-parts?enabled=true&mode=edit");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("active", document.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("bindingStatus").GetString());
        Assert.Equal(101, factory.MovablePartService.LastGetRequest!.AssetId);
        Assert.True(factory.MovablePartService.LastGetRequest.Enabled);
        Assert.Equal("edit", factory.MovablePartService.LastGetRequest.Mode);
    }

    [Fact]
    public async Task Post_BindsJsonAndPropagatesConflict()
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        factory.MovablePartService.WriteResult = new MovablePartResult(HttpStatusCode.Conflict, ApiResponse<MovablePartResponse>.Failure(ErrorCode.DuplicatePartCode, "duplicate", Array.Empty<ApiErrorItem>()));
        var response = await client.PostAsJsonAsync("/api/model-assets/101/versions/202/movable-parts", Request());
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorCode.DuplicatePartCode, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("platform", factory.MovablePartService.LastCreateRequest!.PartCode);
    }

    [Theory]
    [InlineData(400, ErrorCode.ValidationFailed)]
    [InlineData(404, ErrorCode.NotFound)]
    public async Task Post_PropagatesValidationAndNotFoundResponses(int statusCode, string errorCode)
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        factory.MovablePartService.WriteResult = new MovablePartResult((HttpStatusCode)statusCode, ApiResponse<MovablePartResponse>.Failure(errorCode, "failure", Array.Empty<ApiErrorItem>()));
        var response = await client.PostAsJsonAsync("/api/model-assets/101/versions/202/movable-parts", Request());
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal((HttpStatusCode)statusCode, response.StatusCode);
        Assert.Equal(errorCode, document.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_PropagatesVersionStatusConflict()
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        factory.MovablePartService.ListResult = new MovablePartListResult(HttpStatusCode.Conflict, ApiResponse<MovablePartListResponse>.Failure(ErrorCode.VersionStatusInvalid, "guard", Array.Empty<ApiErrorItem>()));
        var response = await client.GetAsync("/api/model-assets/101/versions/202/movable-parts");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Put_BindsPartIdAndReturnsSuccess()
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        var response = await client.PutAsJsonAsync("/api/model-assets/101/versions/202/movable-parts/303", Request());
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(303, factory.MovablePartService.LastPartId);
        Assert.Equal("active", document.RootElement.GetProperty("data").GetProperty("bindingStatus").GetString());
    }

    [Fact]
    public async Task Delete_ReturnsServiceResult()
    {
        using var factory = new DigitalTwinApiFactory(); using var client = CreateClient(factory);
        var response = await client.DeleteAsync("/api/model-assets/101/versions/202/movable-parts/303");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(303, factory.MovablePartService.LastPartId);
    }

    [Fact]
    public async Task Get_WhenServiceThrows_ReturnsGlobal500Response()
    {
        using var factory = new DigitalTwinApiFactory(); factory.MovablePartService.ExceptionToThrow = new InvalidOperationException("test"); using var client = CreateClient(factory);
        var response = await client.GetAsync("/api/model-assets/101/versions/202/movable-parts");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(ErrorCode.InternalError, document.RootElement.GetProperty("code").GetString());
    }

    private static CreateMovablePartRequest Request() => new("uuid-1", "/root/platform", "Platform", "platform", "linear", "world", "z", null, null, null, 0, 10, 0, 0, 1, true);
    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
}
