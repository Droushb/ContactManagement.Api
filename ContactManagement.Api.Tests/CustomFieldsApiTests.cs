using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContactManagement.DTOs;
using ContactManagement.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactManagement.Api.Tests;

public class CustomFieldsApiTests : IClassFixture<ContactManagementApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CustomFieldsApiTests(ContactManagementApplicationFactory factory)
    {
        _client = factory.CreateClientWithJsonAccept();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var response = await _client.GetAsync("/api/customfields");
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<CustomFieldDto>>(JsonOptions);
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetById_Returns404_WhenCustomFieldNotFound()
    {
        var response = await _client.GetAsync($"/api/customfields/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithCustomField()
    {
        var request = new CreateCustomFieldRequest
        {
            Name = "TestField_" + Guid.NewGuid().ToString("N")[..8],
            FieldType = CustomFieldType.String
        };
        var response = await _client.PostAsJsonAsync("/api/customfields", request);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var field = await response.Content.ReadFromJsonAsync<CustomFieldDto>(JsonOptions);
        Assert.NotNull(field);
        Assert.Equal(request.Name, field.Name);
        Assert.Equal(request.FieldType, field.FieldType);
        Assert.NotEqual(Guid.Empty, field.Id);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenCustomFieldExists()
    {
        var createRequest = new CreateCustomFieldRequest
        {
            Name = "Original_" + Guid.NewGuid().ToString("N")[..8],
            FieldType = CustomFieldType.Int
        };
        var createResponse = await _client.PostAsJsonAsync("/api/customfields", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomFieldDto>(JsonOptions);
        Assert.NotNull(created);

        var updateRequest = new UpdateCustomFieldRequest
        {
            Name = "Updated_" + created.Name,
            FieldType = CustomFieldType.Bool
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/customfields/{created.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = await updateResponse.Content.ReadFromJsonAsync<CustomFieldDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(CustomFieldType.Bool, updated.FieldType);
    }

    [Fact]
    public async Task Update_Returns404_WhenCustomFieldNotFound()
    {
        var updateRequest = new UpdateCustomFieldRequest
        {
            Name = "Missing",
            FieldType = CustomFieldType.String
        };
        var response = await _client.PutAsJsonAsync($"/api/customfields/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenCustomFieldExists()
    {
        var createRequest = new CreateCustomFieldRequest
        {
            Name = "ToDelete_" + Guid.NewGuid().ToString("N")[..8],
            FieldType = CustomFieldType.String
        };
        var createResponse = await _client.PostAsJsonAsync("/api/customfields", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomFieldDto>(JsonOptions);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/customfields/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/customfields/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns404_WhenCustomFieldNotFound()
    {
        var response = await _client.DeleteAsync($"/api/customfields/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
