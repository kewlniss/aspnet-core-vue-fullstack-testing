using FullStackTesting.Web.Api.Persistence;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;

namespace FullStackTesting.Web.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        public TestServer TestServer { get; }
        public HttpClient Client { get; }

        public CustomWebApplicationFactory()
        {
            var webHostBuilder = CreateWebHostBuilder()
                .UseEnvironment("tests")
                .UseUrls("http://*:9876")
                //don't use my test's content root, use the real one instead...
                .UseContentRoot(Path.GetFullPath("../../../../../FullStackTesting.Web.Api"));

            webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
            });

            TestServer = new TestServer(webHostBuilder);
            Client = TestServer.CreateClient();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = WebHost.CreateDefaultBuilder(null)
                .UseStartup<TestStartup>();
            //.ConfigureKestrel((context, options) => options.ConfigureEndpoints());


            ConfigureWebHost(builder);

            return builder;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add a database context (AppDbContext) using an in-memory database for testing.
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("EmployeeMemoryDB");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Create a scope (with the built service provider) to obtain a reference to the database contexts
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var appDb = scopedServices.GetRequiredService<AppDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                    // Ensure the database is created.
                    appDb.Database.EnsureCreated();

                    try
                    {
                        // Add testing data for memoryDB
                        AppDbSeedData.LoadSeedData(appDb);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred seeding the database with test data. Error: {ex?.Message}");
                    }
                }
            });
        }
    }
}
