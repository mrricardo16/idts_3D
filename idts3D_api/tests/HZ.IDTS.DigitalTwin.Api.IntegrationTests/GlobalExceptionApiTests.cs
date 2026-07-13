namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests;

public sealed class GlobalExceptionApiTests
{
    [Fact]
    public async Task GetManifest_WhenFakeThrows_ReturnsSanitizedInternalErrorResponse()
    {
        using var factory = new DigitalTwinApiFactory();
        factory.ManifestService.ExceptionToThrow = new InvalidOperationException("forbidden exception detail");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/model-assets/101/manifest?mode=edit");
        var body = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(ErrorCode.InternalError, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("Internal server error.", document.RootElement.GetProperty("message").GetString());
        Assert.DoesNotContain("forbidden exception detail", body, StringComparison.Ordinal);
    }
}
