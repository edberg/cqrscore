using System;
using CQRSCore.Cache;
using CQRSCore.Tests.Substitutes;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CQRSCore.Tests.Cache
{
    public class When_saving_fails
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;
        private TestMemoryCache _cache;

        public When_saving_fails()
        {
            _testRep = new TestRepository();
            _cache = new TestMemoryCache();
            _rep = new CacheRepository(_testRep, new TestInMemoryEventStore(), _cache);
            _aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid());
            _aggregate.DoSomething();
            try {  _rep.Save(_aggregate, 100); }  catch (Exception){}
        }

        [Fact]
        public void Should_evict_old_object_from_cache()
        {
            var aggregate = _cache.Get(_aggregate.Id.ToString());
            Assert.Null(aggregate);
        }
    }
}