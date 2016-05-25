using System;
using CQRSCore.Domain;

namespace CQRSCore.Tests.Substitutes
{
    public class TestAggregate : AggregateRoot, IEquatable<TestAggregate>, IEquatable<object>
    {
        private TestAggregate() { }
        public TestAggregate(Guid id)
        {
            Id = id;
            ApplyChange(new TestAggregateCreated());
        }

        public int DidSomethingCount;

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }

        public void DoSomethingElse()
        {
            ApplyChange(new TestAggregateDidSomeethingElse());
        }

        public void Apply(TestAggregateDidSomething e)
        {
            DidSomethingCount++;
        }

        public bool Equals(TestAggregate other)
        {
            return this.Id.ToString() == other.Id.ToString()
                && this.Version == other.Version 
                && this.DidSomethingCount == other.DidSomethingCount;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o is TestAggregate) return Equals(o as TestAggregate);
            return false;
        }
    }
}
