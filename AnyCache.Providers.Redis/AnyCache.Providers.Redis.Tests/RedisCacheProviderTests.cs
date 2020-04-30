using AnyCache.Providers.Redis.Tests.Models;
using NUnit.Framework;
using System;

namespace AnyCache.Providers.Redis.Tests
{
    [TestFixture]
    public class RedisCacheProviderTests
    {
        [Test]
        [Ignore("Redis isn't installed on AppVeyor, skip test")]
        public void Should_Store_Redis()
        {
            var key = nameof(Should_Store_Redis);
            var data = new TestModel {
                Id = 123,
                Name = "Test Data",
                Description = "Test description"
            };
            var provider = new RedisCacheProvider("localhost:6379,resolvedns=1,password=password,connectTimeout=1000");

            // fetch (should not exist)
            var getData = provider.Get(key);
            Assert.IsNull(getData);
            
            // store
            provider.Set(key, data, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Now.AddSeconds(5) });

            // fetch
            getData = provider.Get(key);
            Assert.NotNull(getData);
        }
    }
}
