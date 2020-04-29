using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace AnyCache.Providers.Redis
{
    /// <summary>
    /// Redis cache provider for LazyCache
    /// </summary>
    public class RedisCacheProvider : ICacheStorageProvider
    {
        /// <summary>
        /// Default cache expiry if none is specified
        /// </summary>
        private const int DefaultCacheExpirySeconds = 120;

        /// <summary>
        /// Redis server connection string
        /// </summary>
        private const string DefaultConfiguration = "localhost:6379";

        /// <summary>
        /// Redis database 
        /// </summary>
        private const int DataBase = 2;

        /// <summary>
        /// The Redis configuration string
        /// </summary>
        private static string _configuration = DefaultConfiguration;

        /// <summary>
        /// Get the Redis connection multiplexer once
        /// </summary>
        private static readonly Lazy<IConnectionMultiplexer> _redis = new Lazy<IConnectionMultiplexer>(
            () =>
            {
                var redis = ConnectionMultiplexer.Connect(_configuration);
                redis.PreserveAsyncOrder = false;
                return redis;
            });

        /// <summary>
        /// Redis db instance
        /// </summary>
        private IDatabase Db => _redis.Value.GetDatabase(DataBase);

        /// <summary>
        /// Redis cache provider
        /// </summary>
        /// <param name="configuration"></param>
        public RedisCacheProvider(string configuration)
        {
            if (!string.IsNullOrEmpty(configuration))
                _configuration = configuration;
        }

        /// <summary>
        /// Redis cache provider
        /// </summary>
        /// <param name="configuration"></param>
        public RedisCacheProvider(RedisConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration.ToString();
        }

        /// <summary>
        /// Get object from cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            var cacheEntry = InternalGet(key);
            if (cacheEntry != null)
                return JsonConvert.DeserializeObject(cacheEntry.Data, cacheEntry.Type);

            return null;
        }

        /// <summary>
        /// Get object from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var cacheEntry = InternalGet(key);
            if (cacheEntry != null)
                return JsonConvert.DeserializeObject<T>(cacheEntry.Data);

            return default(T);
        }

        /// <summary>
        /// Get or create object from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public T GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
        {
            var cachedItem = InternalGet(key);
            if (cachedItem == null)
            {
                var policy = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(DefaultCacheExpirySeconds) };
                var redisCacheEntry = new RedisCacheEntry();
                redisCacheEntry.Key = key;
                redisCacheEntry.SetAbsoluteExpiration(policy.AbsoluteExpirationRelativeToNow.Value);
                var typedItem = factory.Invoke(redisCacheEntry);

                var cacheEntry = InternalSet(key, typedItem, policy);
                return JsonConvert.DeserializeObject<T>(cacheEntry.Data);
            }
            return JsonConvert.DeserializeObject<T>(cachedItem.Data);
        }

        /// <summary>
        /// Get or create object from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheEntry, Task<T>> factory)
        {
            var cachedItem = InternalGet(key);
            if (cachedItem == null)
            {
                var policy = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(DefaultCacheExpirySeconds) };
                var redisCacheEntry = new RedisCacheEntry();
                redisCacheEntry.Key = key;
                redisCacheEntry.SetAbsoluteExpiration(policy.AbsoluteExpirationRelativeToNow.Value);
                var typedItem = await factory.Invoke(redisCacheEntry);

                var cacheEntry = InternalSet(key, typedItem, policy);
                return JsonConvert.DeserializeObject<T>(cacheEntry.Data);
            }
            return JsonConvert.DeserializeObject<T>(cachedItem.Data);
        }

        /// <summary>
        /// Remove object from cache
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            Db.KeyDelete(key);
        }

        /// <summary>
        /// Set object in cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="policy"></param>
        public void Set(string key, object item, MemoryCacheEntryOptions policy)
        {
            InternalSet(key, item, policy);
        }

        private RedisDataEntry InternalSet(string key, object item, MemoryCacheEntryOptions policy)
        {
            if (item == null)
                return null;
            if (policy == null)
                policy = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(DefaultCacheExpirySeconds) };

            var dataEntry = new RedisDataEntry(item);
            // serialize this entry as json string encoded as a RedisDataEntry
            var serializedObject = JsonConvert.SerializeObject(dataEntry);

            var expireWhen = TimeSpan.FromSeconds(DefaultCacheExpirySeconds);
            if (policy.AbsoluteExpiration.HasValue)
                expireWhen = DateTime.Now.Subtract(policy.AbsoluteExpiration.Value.DateTime);
            if (policy.AbsoluteExpirationRelativeToNow.HasValue)
                expireWhen = policy.AbsoluteExpirationRelativeToNow.Value;
            if (policy.SlidingExpiration.HasValue)
                expireWhen = policy.SlidingExpiration.Value;

            Db.StringSet(key, serializedObject, expireWhen);

            return dataEntry;
        }

        private RedisDataEntry InternalGet(string key)
        {
            var content = Db.StringGet(key);

            if (!content.IsNull && content.HasValue && !string.IsNullOrEmpty(content))
            {
                // deserialize entry from json back to RedisDataEntry
                var dataEntry = JsonConvert.DeserializeObject<RedisDataEntry>(content.ToString());
                return dataEntry;
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
    }
}
