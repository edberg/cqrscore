using System;
using CQRSCore.Domain;
using CQRSCore.Domain.Exception;
using CQRSCore.Tests.Substitutes;
using Xunit;

namespace CQRSCore.Tests.Domain
{
    public class When_getting_events_out_of_order
    {
	    private ISession _session;

        public When_getting_events_out_of_order()
        {
            var eventStore = new TestEventStoreWithBugs();
            _session = new Session(new Repository(eventStore));
        }

        [Fact]
        public void Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            Assert.Throws<EventsOutOfOrderException>(() => _session.Get<TestAggregate>(id, 3));
        }
    }
}