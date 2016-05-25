using System;
using CQRSCore.Cache;
using CQRSCore.Tests.Substitutes;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace CQRSCore.Tests.Cache
{
    public class When_getting_earlier_than_expected_events_from_event_store
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestMemoryCache _cache;

        public When_getting_earlier_than_expected_events_from_event_store()
        {
            _cache = new TestMemoryCache();
            _rep = new CacheRepository(new TestRepository(), new TestEventStoreWithBugs(), _cache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Fact]
        public void Should_evict_old_object_from_cache()
        {
            _rep.Get<TestAggregate>(_aggregate.Id);
            var aggregate = _cache.Get(_aggregate.Id.ToString());
            Assert.Equal(_aggregate, aggregate);
        }

        [Fact]
        public void Should_get_events_from_start()
        {
            var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(1, aggregate.Version);
        }
    }
}