using System;
using StackExchange.Redis;

namespace RingBufferLogging
{
    /// <summary>
    /// Redis implementation of IKeyValueStore.
    /// </summary>
    public class RedisKeyValueStore : IKeyValueStore
    {
        private readonly IDatabase _db;
        public RedisKeyValueStore(IDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public bool StringSet(string key, string value) => _db.StringSet(key, value);
        public string? StringGet(string key)
        {
            var value = _db.StringGet(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        public bool KeyDelete(string key) => _db.KeyDelete(key);
        public bool KeyExists(string key) => _db.KeyExists(key);
        public long StringIncrement(string key) => _db.StringIncrement(key);
    }
}
