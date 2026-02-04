using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContactManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactManagement.Api.Tests;

public class ContactsApiTests : IClassFixture<ContactManagementApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ContactsApiTests(ContactManagementApplicationFactory factory)
    {
        _client = factory.CreateClientWithJsonAccept();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithPagedResult()
    {
        var response = await _client.GetAsync("/api/contacts?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ContactDto>>(json, JsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetById_Returns404_WhenContactNotFound()
    {
        var response = await _client.GetAsync($"/api/contacts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithContact()
    {
        var request = new CreateContactRequest
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = $"it-{Guid.NewGuid():N}@example.com",
            Phone = "+380555123456"
        };
        var response = await _client.PostAsJsonAsync("/api/contacts", request);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contact = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(contact);
        Assert.Equal(request.FirstName, contact.FirstName);
        Assert.Equal(request.LastName, contact.LastName);
        Assert.Equal(request.Email, contact.Email);
        Assert.NotEqual(Guid.Empty, contact.Id);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var request = new CreateContactRequest
        {
            FirstName = "First",
            LastName = "User",
            Email = email
        };
        var create1 = await _client.PostAsJsonAsync("/api/contacts", request);
        create1.EnsureSuccessStatusCode();

        var create2 = await _client.PostAsJsonAsync("/api/contacts", request);
        Assert.Equal(HttpStatusCode.Conflict, create2.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenContactExists()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Original",
            LastName = "Name",
            Email = $"upd-{Guid.NewGuid():N}@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contacts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(created);

        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Updated",
            LastName = "Surname",
            Phone = "+380555123457"
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/contacts/{created.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.FirstName);
        Assert.Equal("Surname", updated.LastName);
        Assert.Equal("+380555123457", updated.Phone);
    }

    [Fact]
    public async Task Update_Returns404_WhenContactNotFound()
    {
        var updateRequest = new UpdateContactRequest { FirstName = "A", LastName = "B" };
        var response = await _client.PutAsJsonAsync($"/api/contacts/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenContactExists()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "ToDelete",
            LastName = "Contact",
            Email = $"del-{Guid.NewGuid():N}@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/contacts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/contacts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/contacts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns404_WhenContactNotFound()
    {
        var response = await _client.DeleteAsync($"/api/contacts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
