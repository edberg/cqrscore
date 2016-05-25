using System;
using CQRSCore.Cache;
using CQRSCore.Tests.Substitutes;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;

namespace CQRSCore.Tests.Cache
{
    public class When_getting_aggregate
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;

        public When_getting_aggregate()
        {
            _rep = new CacheRepository(new TestRepository(), new TestEventStore(), new TestMemoryCache());
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Fact]
        public void Should_get_aggregate()
        {
            Assert.NotNull(_aggregate);
        }

        [Fact]
        public void Should_get_same_aggregate_on_second_try()
        {
            var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate, aggregate);
        }

        [Fact]
        public void Should_update_if_version_changed_in_event_store()
        {
            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(3, aggregate.Version);
        }

        [Fact]
        public void Should_get_same_aggregate_from_different_cache_repository()
        {
            var rep = new CacheRepository(new TestRepository(), new TestInMemoryEventStore(), new TestMemoryCache());
            var aggregate = rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate, aggregate);
        }
    }
}