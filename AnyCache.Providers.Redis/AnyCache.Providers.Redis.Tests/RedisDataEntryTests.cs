using AnyCache.Providers.Redis.Tests.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AnyCache.Providers.Redis.Tests
{
    [TestFixture]
    public class RedisDataEntryTests
    {
        [Test]
        public void Should_CreateSerialized_DataEntry()
        {
            var model = new TestModel
            {
                Id = 1,
                Name = "Test",
                Description = "Test Description"
            };
            var entry = new RedisDataEntry(model);
            Assert.AreEqual(typeof(TestModel), entry.Type);
            Assert.NotNull(entry.Data);
        }

        [Test]
        public void Should_CreatePreSerialized_DataEntry()
        {
            var model = new TestModel
            {
                Id = 1,
                Name = "Test",
                Description = "Test Description"
            };
            var entry = new RedisDataEntry(JsonConvert.SerializeObject(model), typeof(TestModel));
            Assert.AreEqual(typeof(TestModel), entry.Type);
            Assert.NotNull(entry.Data);
        }
    }
}
