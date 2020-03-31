using System;
using Utf8Json;

namespace AnyCache.Providers.Redis
{
    /// <summary>
    /// Stores an object with a known type
    /// </summary>
    /// <remarks>
    /// Redis is a string-only store, so we must wrap any serialized data going into it because it lacks type information
    /// </remarks>
    internal sealed class RedisDataEntry
    {
        /// <summary>
        /// Json serialized data object
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The original type of the <seealso cref="Data"/> object
        /// </summary>
        public Type Type { get; set; }

        public RedisDataEntry() { }

        public RedisDataEntry(string dataAsJson, Type type)
        {
            Data = dataAsJson;
            Type = type;
        }

        public RedisDataEntry(object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Type = data.GetType();
            // automatically serialize this object to json
            Data = JsonSerializer.ToJsonString(data);
        }

        public override string ToString()
        {
            return $"{Type.Name} => {Data}";
        }
    }
}
