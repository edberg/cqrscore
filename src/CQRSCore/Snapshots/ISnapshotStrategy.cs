using System;
using CQRSCore.Domain;

namespace CQRSCore.Snapshots
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}