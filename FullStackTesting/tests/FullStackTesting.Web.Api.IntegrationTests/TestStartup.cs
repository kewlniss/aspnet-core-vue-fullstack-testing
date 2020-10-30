using CoveCommerce.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;

namespace FullStackTesting.Web.Api.IntegrationTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureIdentity(IServiceCollection services)
        {
            // base.ConfigureIdentity(services);
            services.AddScoped<UserManager<User>>();
            services.AddIdentity<User, Role>(a =>
            {
            })
               .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test Scheme";
                options.DefaultChallengeScheme = "Test Scheme";
                options.DefaultForbidScheme = "Test Scheme";
                options.DefaultScheme = options.DefaultSignInScheme = options.DefaultSignOutScheme = "Test Scheme";

            }).AddTestAuth(o => { });
        }
    }
}
