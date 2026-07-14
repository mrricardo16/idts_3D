using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests;

public sealed class MotionTargetApiTests
{
    [Fact]
    public async Task Given_MotionTargetGetRoute_When_RequestIsValid_Then_ReturnsOk()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/movable-parts/101/motion-targets?enabled=true&mode=edit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(101, factory.MotionTargetService.LastGetRequest!.PartId);
        Assert.True(factory.MotionTargetService.LastGetRequest.Enabled);
        Assert.Equal("edit", factory.MotionTargetService.LastGetRequest.Mode);
    }

    [Fact]
    public async Task Given_GetWithoutMode_When_RequestingTargets_Then_BindsNullModeForServiceDefaulting()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/movable-parts/101/motion-targets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(factory.MotionTargetService.LastGetRequest!.Mode);
    }

    [Fact]
    public async Task Given_ValidCreateJson_When_PostingTarget_Then_BindsPartAndBody()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync("/api/movable-parts/101/motion-targets", Request());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(101, factory.MotionTargetService.LastPartId);
        Assert.Equal("F1", factory.MotionTargetService.LastCreateRequest!.TargetCode);
    }

    [Fact]
    public async Task Given_ConflictResult_When_GettingTargets_Then_Returns409()
    {
        using var factory = new DigitalTwinApiFactory();
        factory.MotionTargetService.ListResult = new(HttpStatusCode.Conflict, ApiResponse<MotionTargetListResponse>.Failure(ErrorCode.VersionStatusInvalid, "blocked", Array.Empty<ApiErrorItem>()));
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/movable-parts/101/motion-targets");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Given_ValidUpdateJson_When_PuttingTarget_Then_BindsRouteAndBody()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);

        var response = await client.PutAsJsonAsync("/api/movable-parts/101/motion-targets/303", Request());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(101, factory.MotionTargetService.LastPartId);
        Assert.Equal(303, factory.MotionTargetService.LastTargetId);
        Assert.Equal("F1", factory.MotionTargetService.LastUpdateRequest!.TargetCode);
    }

    [Fact]
    public async Task Given_DeleteRoute_When_DeletingTarget_Then_BindsIdentifiersAndReturnsDeleted()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);

        var response = await client.DeleteAsync("/api/movable-parts/101/motion-targets/303");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeleteMotionTargetResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(101, factory.MotionTargetService.LastPartId);
        Assert.Equal(303, factory.MotionTargetService.LastTargetId);
        Assert.True(body!.Data!.Deleted);
    }

    [Fact]
    public async Task Given_ServiceThrows_When_RequestingTargets_Then_GlobalExceptionMiddlewareReturns500()
    {
        using var factory = new DigitalTwinApiFactory();
        factory.MotionTargetService.ExceptionToThrow = new InvalidOperationException("test");
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/movable-parts/101/motion-targets");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
    private static CreateMotionTargetRequest Request() => new("F1", "Floor 1", 2, null, null, 2, 1, true);
}
