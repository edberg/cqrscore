using System;
using CQRSCore.Events;

namespace CQRSCore.Tests.Substitutes
{
    public class TestAggregateCreated : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}