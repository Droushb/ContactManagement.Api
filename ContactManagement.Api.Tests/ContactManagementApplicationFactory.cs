using System.Net.Http.Headers;
using ContactManagement.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContactManagement.Api.Tests;

public class ContactManagementApplicationFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ContactManagementDbContext>));
            services.RemoveAll(typeof(ContactManagementDbContext));
            services.AddDbContext<ContactManagementDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString("N"));
            });
        });
    }

    public HttpClient CreateClientWithJsonAccept()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}
