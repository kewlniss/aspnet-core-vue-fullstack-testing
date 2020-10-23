using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FullStackTesting.Web.Api.IntegrationTests
{
    [CollectionDefinition("Database and Test Server collection")]
    public class DatabaseAndServerCollection : ICollectionFixture<DatabaseFixture>, ICollectionFixture<CustomWebApplicationFactory>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
