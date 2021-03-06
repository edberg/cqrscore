﻿using CQRSCore.Domain;
using CQRSCore.Tests.Substitutes;
using Xunit;

namespace CQRSCore.Tests.Domain
{
    public class When_saving_using_repository_with_event_publisher
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
        private ISession _session;
        private Repository _rep;

        public When_saving_using_repository_with_event_publisher()
        {
            _eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
#pragma warning disable 618
            _rep = new Repository(_eventStore, _eventPublisher);
#pragma warning restore 618
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Fact]
        public void Should_publish_events()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.Equal(1, _eventPublisher.Published);
        }
    }
}