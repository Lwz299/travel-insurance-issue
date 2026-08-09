using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelInsurance.IntegrationTests;

public class SeedAccountApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SeedAccountApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Development");
        }).CreateClient();
    }

    [Theory]
    [InlineData("superadmin@altadamun-ins.test")]
    [InlineData("admin@altadamun-ins.test")]
    [InlineData("agency@altadamun-ins.test")]
    [InlineData("customer@altadamun-ins.test")]
    [InlineData("reinsurer@altadamun-ins.test")]
    public async Task SeedAccounts_CanLogin(string email)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Test@123456",
            device = "integration-test"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
    }

    [Fact]
    public async Task SuperAdmin_CanListCompanies()
    {
        var token = await LoginAsync("superadmin@altadamun-ins.test");

        var request = new HttpRequestMessage(HttpMethod.Get, "/companies");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_CanGetSamplePlan()
    {
        var token = await LoginAsync("superadmin@altadamun-ins.test");

        var request = new HttpRequestMessage(HttpMethod.Get, "/plans/d1000000-0000-0000-0000-000000000001");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotManagePlans()
    {
        var token = await LoginAsync("customer@altadamun-ins.test");

        var request = new HttpRequestMessage(HttpMethod.Get, "/plans");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_CanIssuePolicyDirectly()
    {
        var token = await LoginAsync("superadmin@altadamun-ins.test");
        var request = new HttpRequestMessage(HttpMethod.Post, "/policy-issuance/direct")
        {
            Content = JsonContent.Create(new
            {
                planId = "d1000000-0000-0000-0000-000000000001",
                destinationCountryId = "c0000000-0000-0000-0000-000000000004",
                travelStart = "2026-08-01",
                travelEnd = "2026-08-07",
                passengers = new[]
                {
                    new { fullName = "Integration Traveller", dateOfBirth = "1990-01-01", passportNumber = "IT-001" }
                },
                settlement = new { method = "AgencyWallet", providerCode = "DUMMY" }
            })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await _client.SendAsync(request);

        var responseText = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, responseText);
        var body = await response.Content.ReadFromJsonAsync<PolicyResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("Direct", body!.IssuanceSource);
        Assert.Equal("Issued", body.Status);
    }

    [Fact]
    public async Task AcceptedQuote_PaymentIsIdempotent_AndCompletesSuccessfully()
    {
        var token = await LoginAsync("superadmin@altadamun-ins.test");
        var createQuote = new HttpRequestMessage(HttpMethod.Post, "/quotes")
        {
            Content = JsonContent.Create(new
            {
                planId = "d1000000-0000-0000-0000-000000000001",
                destinationCountryId = "c0000000-0000-0000-0000-000000000004",
                travelStart = "2026-08-10", travelEnd = "2026-08-16",
                passengers = new[] { new { fullName = "Payment Traveller", dateOfBirth = "1990-01-01", passportNumber = $"PAY-{Guid.NewGuid():N}" } }
            })
        };
        createQuote.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var quoteResponse = await _client.SendAsync(createQuote);
        quoteResponse.EnsureSuccessStatusCode();
        var quote = await quoteResponse.Content.ReadFromJsonAsync<QuoteResponse>(JsonOptions);
        Assert.NotNull(quote);

        var accept = new HttpRequestMessage(HttpMethod.Post, $"/quotes/{quote!.Id}/accept");
        accept.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.OK, (await _client.SendAsync(accept)).StatusCode);

        var idempotencyKey = Guid.NewGuid().ToString();
        var first = await CreatePaymentAsync(token, quote.Id, idempotencyKey);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<CreatePaymentResponse>(JsonOptions);
        Assert.NotNull(firstBody);
        Assert.Equal("Paid", firstBody!.Payment.Status);
        Assert.Equal("Succeeded", firstBody.Attempt.Status);

        var second = await CreatePaymentAsync(token, quote.Id, idempotencyKey);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<CreatePaymentResponse>(JsonOptions);
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.Payment.Id, secondBody!.Payment.Id);
        Assert.Equal(firstBody.Attempt.Id, secondBody.Attempt.Id);
    }

    [Fact]
    public async Task Agency_CannotReadQuoteCreatedByAnotherUser()
    {
        var superAdminToken = await LoginAsync("superadmin@altadamun-ins.test");
        var create = new HttpRequestMessage(HttpMethod.Post, "/quotes")
        {
            Content = JsonContent.Create(new
            {
                planId = "d1000000-0000-0000-0000-000000000001",
                destinationCountryId = "c0000000-0000-0000-0000-000000000004",
                travelStart = "2026-08-01", travelEnd = "2026-08-07",
                passengers = new[] { new { fullName = "Quote Owner", dateOfBirth = "1990-01-01" } }
            })
        };
        create.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", superAdminToken);
        var createResponse = await _client.SendAsync(create);
        createResponse.EnsureSuccessStatusCode();
        var quote = await createResponse.Content.ReadFromJsonAsync<QuoteResponse>(JsonOptions);

        var agencyToken = await LoginAsync("agency@altadamun-ins.test");
        var read = new HttpRequestMessage(HttpMethod.Get, $"/quotes/{quote!.Id}");
        read.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", agencyToken);
        var readResponse = await _client.SendAsync(read);

        Assert.Equal(HttpStatusCode.NotFound, readResponse.StatusCode);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Test@123456",
            device = "integration-test"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return body!.AccessToken;
    }

    private async Task<HttpResponseMessage> CreatePaymentAsync(string token, Guid quoteId, string idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(new { quoteId, method = "ElectronicGateway", providerCode = "DUMMY", purpose = "PolicyIssuance" })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return await _client.SendAsync(request);
    }

    private sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class PolicyResponse
    {
        public string IssuanceSource { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private sealed class QuoteResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class CreatePaymentResponse
    {
        public PaymentResponse Payment { get; set; } = new();
        public PaymentAttemptResponse Attempt { get; set; } = new();
    }

    private sealed class PaymentResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class PaymentAttemptResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
