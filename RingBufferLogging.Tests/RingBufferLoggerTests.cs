using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace RingBufferLogging.Tests
{
    [TestFixture]
    public class RingBufferLoggerTests
    {
        private IKeyValueStore _store;
        private RingBufferLogger _logger;
        private const string Prefix = "test";
        private const int BufferSize = 3;

        [SetUp]
        public void SetUp()
        {
            _store = Substitute.For<IKeyValueStore>();
            _logger = new RingBufferLogger(_store, Prefix, BufferSize);
        }

        [Test]
        public void Log_WritesEntryToStore()
        {
            _store.StringIncrement(Arg.Any<string>()).Returns(0L);
            _logger.Log(LogLevel.Information, new EventId(1), "msg", null, (s, e) => s.ToString());
            _store.Received().StringSet(Arg.Is<string>(k => k.StartsWith(Prefix)), Arg.Any<string>());
        }

        [Test]
        public void GetLogEntries_ReturnsEntriesInOrder()
        {
            // Simulate 3 log entries
            var entries = new[]
            {
                new LogEntry { Timestamp = DateTimeOffset.UtcNow, LogLevel = LogLevel.Information, EventId = 1, Message = "A" },
                new LogEntry { Timestamp = DateTimeOffset.UtcNow, LogLevel = LogLevel.Warning, EventId = 2, Message = "B" },
                new LogEntry { Timestamp = DateTimeOffset.UtcNow, LogLevel = LogLevel.Error, EventId = 3, Message = "C" }
            };
            _store.StringGet(Arg.Is<string>(k => k.EndsWith(":position"))).Returns("2");
            for (int i = 0; i < BufferSize; i++)
            {
                _store.StringGet($"{Prefix}:entry:{(3 + i) % BufferSize}")
                    .Returns(entries[i].ToJson());
            }
            var result = _logger.GetLogEntries();
            NUnit.Framework.Assert.That(result.Count, NUnit.Framework.Is.EqualTo(BufferSize));
            NUnit.Framework.Assert.That(result[0].Message, NUnit.Framework.Is.EqualTo("A"));
            NUnit.Framework.Assert.That(result[1].Message, NUnit.Framework.Is.EqualTo("B"));
            NUnit.Framework.Assert.That(result[2].Message, NUnit.Framework.Is.EqualTo("C"));
        }

        [Test]
        public void Clear_DeletesAllEntriesAndResetsPosition()
        {
            _logger.Clear();
            for (int i = 0; i < BufferSize; i++)
            {
                _store.Received().KeyDelete($"{Prefix}:entry:{i}");
            }
            _store.Received().StringSet($"{Prefix}:position", "-1");
        }

        [Test]
        public void GetLogCount_ReturnsCorrectCount()
        {
            _store.KeyExists(Arg.Any<string>()).Returns(true, false, true);
            var count = _logger.GetLogCount();
            NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(2));
        }
    }
}
