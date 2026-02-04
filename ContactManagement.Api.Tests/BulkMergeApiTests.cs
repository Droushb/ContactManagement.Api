using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContactManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactManagement.Api.Tests;

public class BulkMergeApiTests : IClassFixture<ContactManagementApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BulkMergeApiTests(ContactManagementApplicationFactory factory)
    {
        _client = factory.CreateClientWithJsonAccept();
    }

    [Fact]
    public async Task Merge_ReturnsOk_WithEmptyResult_WhenNoContactIds()
    {
        var request = new BulkMergeRequest { ContactIds = new List<Guid>() };
        var response = await _client.PostAsJsonAsync("/api/contacts/merge", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BulkMergeResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.MergedContacts);
        Assert.Empty(result.MergedContacts);
        Assert.NotNull(result.MergedCountByEmail);
    }

    [Fact]
    public async Task Merge_ReturnsOk_WithEmptyResult_WhenSingleContact()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Solo",
            LastName = "Contact",
            Email = $"solo-{Guid.NewGuid():N}@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contacts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(created);

        var mergeRequest = new BulkMergeRequest { ContactIds = new List<Guid> { created.Id } };
        var mergeResponse = await _client.PostAsJsonAsync("/api/contacts/merge", mergeRequest);
        mergeResponse.EnsureSuccessStatusCode();
        var result = await mergeResponse.Content.ReadFromJsonAsync<BulkMergeResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result.MergedContacts);
    }

    [Fact]
    public async Task Merge_ReturnsOk_WithMergedContact_WhenTwoContactsSameEmail()
    {
        var email = $"merge-{Guid.NewGuid():N}@example.com";
        var create1 = new CreateContactRequest
        {
            FirstName = "First",
            LastName = "Contact",
            Email = email
        };
        var create2 = new CreateContactRequest
        {
            FirstName = "Second",
            LastName = "Merged",
            Email = email
        };
        var res1 = await _client.PostAsJsonAsync("/api/contacts", create1);
        res1.EnsureSuccessStatusCode();
        var c1 = await res1.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(c1);

        var res2 = await _client.PostAsJsonAsync("/api/contacts", create2);
        res2.EnsureSuccessStatusCode();
        var c2 = await res2.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(c2);

        var mergeRequest = new BulkMergeRequest { ContactIds = new List<Guid> { c1.Id, c2.Id } };
        var mergeResponse = await _client.PostAsJsonAsync("/api/contacts/merge", mergeRequest);
        mergeResponse.EnsureSuccessStatusCode();
        var result = await mergeResponse.Content.ReadFromJsonAsync<BulkMergeResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Single(result.MergedContacts);
        Assert.Equal(2, result.MergedCountByEmail.GetValueOrDefault(email.ToLowerInvariant(), 0));

        var merged = result.MergedContacts[0];
        Assert.Equal(email.ToLowerInvariant(), merged.Email.ToLowerInvariant());
        Assert.Equal("Second", merged.FirstName);
        Assert.Equal("Merged", merged.LastName);

        var getResponse = await _client.GetAsync($"/api/contacts/{c2.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        var getMaster = await _client.GetAsync($"/api/contacts/{c1.Id}");
        getMaster.EnsureSuccessStatusCode();
    }
}
