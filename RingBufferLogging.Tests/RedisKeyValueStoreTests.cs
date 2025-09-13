using System;
using NUnit.Framework;
using StackExchange.Redis;
using NSubstitute;

namespace RingBufferLogging.Tests
{
    [TestFixture]
    public class RedisKeyValueStoreTests
    {
        private IDatabase _db;
        private RedisKeyValueStore _store;
        private string _testKey;

        [SetUp]
        public void SetUp()
        {
            _db = Substitute.For<IDatabase>();
            _store = new RedisKeyValueStore(_db);
            _testKey = $"test:{Guid.NewGuid()}";
        }

        [Test]
        public void StringSet_And_StringGet_Works()
        {
            _db.StringSet(_testKey, "value").Returns(true);
            _db.StringGet(_testKey).Returns((RedisValue)"value");
            Assert.That(_store.StringSet(_testKey, "value"), Is.True);
            Assert.That(_store.StringGet(_testKey), Is.EqualTo("value"));
        }

        [Test]
        public void KeyExists_ReturnsTrueIfExists()
        {
            _db.KeyExists(_testKey).Returns(true);
            Assert.That(_store.KeyExists(_testKey), Is.True);
        }

        [Test]
        public void KeyDelete_RemovesKey()
        {
            _db.KeyDelete(_testKey).Returns(true);
            _db.KeyExists(_testKey).Returns(false);
            Assert.That(_store.KeyDelete(_testKey), Is.True);
            Assert.That(_store.KeyExists(_testKey), Is.False);
        }

        [Test]
        public void StringIncrement_IncrementsValue()
        {
            _db.StringIncrement(_testKey).Returns(1, 2);
            Assert.That(_store.StringIncrement(_testKey), Is.EqualTo(1));
            Assert.That(_store.StringIncrement(_testKey), Is.EqualTo(2));
        }
    }
}
