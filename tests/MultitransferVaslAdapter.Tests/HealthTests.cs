using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MultitransferVaslAdapter.Tests;

public class HealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public HealthTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Health_Ok()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
