﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSCore.Cache;
using CQRSCore.Domain;
using CQRSCore.Tests.Substitutes;
using Xunit;

namespace CQRSCore.Tests.Cache
{
    public class When_saving_same_aggregate_in_parallel
    {
        private CacheRepository _rep1;
        private CacheRepository _rep2;
        private TestAggregate _aggregate;
        private TestInMemoryEventStore _testStore;
        private TestMemoryCache _cache;

        public When_saving_same_aggregate_in_parallel()
        {
            _cache = new TestMemoryCache();
            // This will clear the cache between runs.
            //var cacheKeys = _cache. (.Select(kvp => kvp.Key).ToList();
            //foreach (var cacheKey in cacheKeys)
            //    MemoryCache.Default.Remove(cacheKey);

            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore), _testStore, _cache);
            _rep2 = new CacheRepository(new Repository(_testStore), _testStore, _cache);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _rep1.Save(_aggregate);

            var t1 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep1.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep1.Save(aggregate);
                                      }
                                  });

            var t2 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep2.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep2.Save(aggregate);
                                      }
                                  });
            var t3 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep2.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep2.Save(aggregate);
                                      }
                                  });
            t1.Start();
            t2.Start();
            t3.Start();

            Task.WaitAll(t1, t2, t3);
        }

        [Fact]
        public void Should_not_get_more_than_one_event_with_same_id()
        {
            Assert.Equal(_testStore.Events.Count, _testStore.Events.Select(x => x.Version).Distinct().Count());
        }

        [Fact]
        public void Should_save_all_events()
        {
            Assert.Equal(301, _testStore.Events.Count);
        }
    }
}