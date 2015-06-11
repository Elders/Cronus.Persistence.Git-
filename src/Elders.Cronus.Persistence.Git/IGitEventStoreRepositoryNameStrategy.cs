using Elders.Cronus.EventStore;

namespace Elders.Cronus.Persistence.Cassandra
{
    public interface IGitEventStoreRepositoryNameStrategy
    {
        string GetEventsRepositoryName(AggregateCommit aggregateCommit);
        string GetEventsRepositoryName(string boundedContext);
    }
}