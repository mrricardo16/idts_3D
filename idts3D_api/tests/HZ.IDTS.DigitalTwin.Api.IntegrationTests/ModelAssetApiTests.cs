namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests;

public sealed class ModelAssetApiTests
{
    [Fact]
    public async Task GetManifest_BindsRouteAndQueryAndReturnsSuccessResponse()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/model-assets/101/manifest?versionId=202&mode=edit");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(101, document.RootElement.GetProperty("data").GetProperty("assetId").GetInt64());
        Assert.Equal(202, document.RootElement.GetProperty("data").GetProperty("versionId").GetInt64());
        Assert.Equal("edit", factory.ManifestService.LastRequest?.Mode);
        Assert.Equal(101, factory.ManifestService.LastRequest?.AssetId);
        Assert.Equal(202, factory.ManifestService.LastRequest?.VersionId);
    }

    [Fact]
    public async Task GetManifest_WhenFakeReturnsAssetNotFound_Propagates404FailureResponse()
    {
        using var factory = new DigitalTwinApiFactory();
        factory.ManifestService.Result = new ModelManifestResult(
            HttpStatusCode.NotFound,
            ApiResponse<ModelManifestResponse>.Failure(
                ErrorCode.AssetNotFound,
                "Asset was not found.",
                new[] { new ApiErrorItem("assetId", "101") }));
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/model-assets/101/manifest?mode=edit");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(ErrorCode.AssetNotFound, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("assetId", document.RootElement.GetProperty("errors")[0].GetProperty("field").GetString());
    }

    [Fact]
    public async Task Publish_WhenFakeReturnsVersionStatusConflict_Propagates409AndBindsJsonBody()
    {
        using var factory = new DigitalTwinApiFactory();
        factory.AssetVersionLifecycleService.Result = new AssetVersionStatusChangeResult(
            HttpStatusCode.Conflict,
            ApiResponse<AssetVersionResponse>.Failure(
                ErrorCode.VersionStatusInvalid,
                "Version status is invalid.",
                new[] { new ApiErrorItem("versionStatus", "Draft") }));
        using var client = CreateClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/model-assets/101/versions/202/publish",
            new { remark = "publish test" });
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("publish", factory.AssetVersionLifecycleService.LastOperation);
        Assert.Equal(101, factory.AssetVersionLifecycleService.LastAssetId);
        Assert.Equal(202, factory.AssetVersionLifecycleService.LastVersionId);
        Assert.Equal("publish test", factory.AssetVersionLifecycleService.LastRequest?.Remark);
    }

    [Fact]
    public async Task Upload_WhenFileIsMissing_ReturnsValidationFailureWithoutCallingService()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);
        using var form = new MultipartFormDataContent
        {
            { new StringContent("TEST-GLB"), "assetCode" },
            { new StringContent("Test GLB"), "assetName" },
            { new StringContent("device_glb"), "assetType" },
            { new StringContent("glb"), "sourceFileType" }
        };

        var response = await client.PostAsync("/api/model-assets/upload", form);
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("file", document.RootElement.GetProperty("errors")[0].GetProperty("field").GetString());
        Assert.Equal(0, factory.UploadService.CallCount);
    }

    [Fact]
    public async Task Upload_BindsMultipartFormAndPassesInMemoryFileToFakeService()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);
        var fileBytes = new byte[] { 0x67, 0x6C, 0x54, 0x46 };
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", "test.glb");
        form.Add(new StringContent("TEST-GLB"), "assetCode");
        form.Add(new StringContent("Test GLB"), "assetName");
        form.Add(new StringContent("device_glb"), "assetType");
        form.Add(new StringContent("glb"), "sourceFileType");

        var response = await client.PostAsync("/api/model-assets/upload", form);
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("TEST-GLB", document.RootElement.GetProperty("data").GetProperty("assetCode").GetString());
        Assert.Equal(1, factory.UploadService.CallCount);
        Assert.Equal("TEST-GLB", factory.UploadService.LastRequest?.AssetCode);
        Assert.Equal("test.glb", factory.UploadService.LastFileName);
        Assert.Equal(fileBytes.LongLength, factory.UploadService.LastFileLength);
        Assert.Equal(fileBytes, factory.UploadService.LastFileContent);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
}
