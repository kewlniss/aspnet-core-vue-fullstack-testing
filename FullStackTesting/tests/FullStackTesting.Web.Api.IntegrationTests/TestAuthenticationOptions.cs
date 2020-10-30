using CoveCommerce.Domain;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace FullStackTesting.Web.Api.IntegrationTests
{
    public class TestAuthenticationOptions : AuthenticationSchemeOptions
    {
        static int id = 2;
        public TestAuthenticationOptions()
        {
        }
        public virtual ClaimsIdentity Identity { get; } = new ClaimsIdentity(new Claim[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", id.ToString()), // Guid.NewGuid().ToString()),
            new Claim(Res.Claims.General, Res.Claims.Audience)
        },
            "test");
    }
}
