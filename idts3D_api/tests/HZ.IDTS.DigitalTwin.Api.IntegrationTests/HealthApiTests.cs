namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests;

public sealed class HealthApiTests
{
    [Fact]
    public async Task GetHealth_ReturnsHealthyApiResponse()
    {
        using var factory = new DigitalTwinApiFactory();
        using var client = CreateClient(factory);
        var beforeRequest = DateTimeOffset.UtcNow;

        var response = await client.GetAsync("/api/health");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(ErrorCode.Ok, document.RootElement.GetProperty("code").GetString());
        Assert.Equal("Healthy", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        var serverTime = document.RootElement.GetProperty("data").GetProperty("serverTimeUtc").GetDateTimeOffset();
        Assert.InRange(serverTime, beforeRequest, DateTimeOffset.UtcNow.AddSeconds(1));
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
}
