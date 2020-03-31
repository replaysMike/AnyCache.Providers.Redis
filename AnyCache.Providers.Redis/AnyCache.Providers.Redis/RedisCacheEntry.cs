using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace AnyCache.Providers.Redis
{
    internal class RedisCacheEntry : ICacheEntry
    {
        public object Key { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; }
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public override string ToString()
        {
            return Key?.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
            }
        }
    }
}
