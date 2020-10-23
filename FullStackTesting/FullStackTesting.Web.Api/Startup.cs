using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using FullStackTesting.Web.Api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using FullStackTesting.Web.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.AspNetCore.Identity;
using CoveCommerce.Domain.User;
using CoveCommerce.Store.Dapper;
using CoveCommerce.Domain;

namespace FullStackTesting.Web.Api
{
    public class Startup
    {
        private readonly string _spaSourcePath;
        private readonly string _corsPolicyName;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _spaSourcePath = Configuration.GetValue<string>("SPA:SourcePath");
            _corsPolicyName = Configuration.GetValue<string>("CORS:PolicyName");
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register the in-memory db (Data is seeded in Main method of the Program.cs now).
            services.AddDbContext<AppDbContext>(context => context.UseInMemoryDatabase("EmployeeMemoryDB"));

            services.AddScoped<IUserStore<User>, UserStore>();
            services.AddScoped<IRoleStore<Role>, RoleStore>();
            services.AddScoped<ICCUserStore<User>, UserStore>();
            services.AddScoped<IUserEmailStore<User>, UserStore>();
            services.AddScoped<IUserPasswordStore<User>, UserStore>();

            // Registered a scoped EmployeeRepository service (DI into EmployeeController)
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            // Add AllowAny CORS policy
            services.AddCors(c => c.AddPolicy(_corsPolicyName,
                options => options.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

            ConfigureIdentity(services);

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Res.Claims.General, policy => policy.RequireClaim(Res.Claims.Audience, Res.Claims.General));
            });

            // Register RazorPages/Controllers
            services.AddControllers();

            // IMPORTANT CONFIG CHANGE IN 3.0 - 'Async' suffix in action names get stripped by default - so, to access them by full name with 'Async' part - opt out of this feature'.
            services.AddMvc(options => options.SuppressAsyncSuffixInActionNames = false);

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration => configuration.RootPath = $"{_spaSourcePath}/dist");
        }

        public virtual void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<User, Role>(i =>
            {
                //do requirements here ...
                //need to be able to override this at client level...
                //i.Password.RequireDigit = true;

            })
            //for resetpassword, etc -- https://stackoverflow.com/questions/35434427/adddefaulttokenproviders-whats-that-and-how-to-use-those-default-providers
            .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(config =>
            {
            });

            services.AddAuthentication(options => // JwtBearerDefaults.AuthenticationScheme
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = options.DefaultSignInScheme = options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("BACE11A5C27E9A9753FB4BFB3DB1B769D8E77EFB4FA4363A41FC49DA519A14447A95311481D2C8CB42C4A4C")),
                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                        ValidAudience = "Api",
                        ValidIssuer = "http://localhost:5000",
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateAudience = true,
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors(_corsPolicyName);

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    var exDetails = new ExceptionDetails((int)HttpStatusCode.InternalServerError, error?.Error.Message);

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = exDetails.StatusCode;
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Application-Error", exDetails.Message);
                    context.Response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");

                    await context.Response.WriteAsync(exDetails.ToString());
                });
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();

                endpoints.MapFallbackToFile("/index.html");

            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });
        }
    }
}
