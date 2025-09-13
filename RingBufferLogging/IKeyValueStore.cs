using System;
using System.Threading.Tasks;

namespace RingBufferLogging
{
    /// <summary>
    /// Abstraction for a key-value store supporting basic CRUD operations.
    /// </summary>
    public interface IKeyValueStore
    {
        bool StringSet(string key, string value);
        string? StringGet(string key);
        bool KeyDelete(string key);
        bool KeyExists(string key);
        long StringIncrement(string key);
    }
}
