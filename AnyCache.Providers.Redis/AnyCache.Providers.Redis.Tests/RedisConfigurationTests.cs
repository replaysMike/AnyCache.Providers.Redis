using NUnit.Framework;

namespace AnyCache.Providers.Redis.Tests
{
    [TestFixture]
    public class RedisConfigurationTests
    {
        [Test]
        public void Should_CreateConfig()
        {
            var config = new RedisConfiguration("fakehost")
            {
                ConnectTimeout = 5000,
                User = "FakeUser",
                Password = "FakePassword",
                AllowAdmin = false
            };
            var configStr = config.ToString();
            Assert.AreEqual("fakehost,abortConnect=true,allowAdmin=false,connectRetry=3,connectTimeout=5000,configCheckSeconds=60,password=FakePassword,user=FakeUser,proxy=None,responseTimeout=5000,syncTimeout=5000,asyncTimeout=5000,writeBuffer=4096", configStr);
        }
    }
}
