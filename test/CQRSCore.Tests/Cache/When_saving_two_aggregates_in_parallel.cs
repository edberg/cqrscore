﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSCore.Cache;
using CQRSCore.Domain;
using CQRSCore.Tests.Substitutes;
using Xunit;

namespace CQRSCore.Tests.Cache
{
    public class When_saving_two_aggregates_in_parallel
    {
        private CacheRepository _rep1;
        private TestAggregate _aggregate1;
        private TestInMemoryEventStore _testStore;
        private TestAggregate _aggregate2;
        private TestMemoryCache _cache;

        public When_saving_two_aggregates_in_parallel()
        {
            // This will clear the cache between runs.
            //var cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            //foreach (var cacheKey in cacheKeys)
            //    MemoryCache.Default.Remove(cacheKey);

            _cache = new TestMemoryCache();
            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore), _testStore, _cache);

            _aggregate1 = new TestAggregate(Guid.NewGuid());
            _aggregate2 = new TestAggregate(Guid.NewGuid());

            _rep1.Save(_aggregate1);
            _rep1.Save(_aggregate2);

            var t1 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep1.Get<TestAggregate>(_aggregate1.Id);
                                          aggregate.DoSomething();
                                          _rep1.Save(aggregate);
                                      }
                                  });

            var t2 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep1.Get<TestAggregate>(_aggregate2.Id);
                                          aggregate.DoSomething();
                                          _rep1.Save(aggregate);
                                      }
                                  });
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
        }

        [Fact]
        public void Should_not_get_more_than_one_event_with_same_id()
        {
            Assert.Equal(_testStore.Events.Count, _testStore.Events.Select(x => x.Version).Count());
        }

        [Fact]
        public void Should_save_all_events()
        {
            Assert.Equal(202, _testStore.Events.Count());
        }

        [Fact]
        public void Should_distibute_events_correct()
        {
            var aggregate1 = _rep1.Get<TestAggregate>(_aggregate2.Id);
            Assert.Equal(100, aggregate1.DidSomethingCount);
            var aggregate2 = _rep1.Get<TestAggregate>(_aggregate2.Id);
            Assert.Equal(100, aggregate2.DidSomethingCount);
        }
    }
}