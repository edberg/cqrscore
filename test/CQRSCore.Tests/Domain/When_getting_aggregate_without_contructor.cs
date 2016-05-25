using System;
using CQRSCore.Domain;
using CQRSCore.Domain.Exception;
using CQRSCore.Tests.Substitutes;
using Xunit;

namespace CQRSCore.Tests.Domain
{
    public class When_getting_aggregate_without_contructor
    {
	    private ISession _session;

        public When_getting_aggregate_without_contructor()
        {
            var eventStore = new TestInMemoryEventStore();
            _session = new Session(new Repository(eventStore));
        }

        [Fact]
        public void Should_throw_missing_parameterless_constructor_exception()
        {
            Assert.Throws<MissingParameterLessConstructorException>(() => _session.Get<TestAggregateNoParameterLessConstructor>(Guid.NewGuid()));
        }
    }
}