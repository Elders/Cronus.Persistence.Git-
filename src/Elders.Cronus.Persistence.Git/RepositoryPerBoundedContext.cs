using System;
using System.Collections.Concurrent;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class RepositoryPerBoundedContext : IGitEventStoreRepositoryNameStrategy
    {
        private readonly ConcurrentDictionary<string, string> eventsRepositoryName = new ConcurrentDictionary<string, string>();

        public string GetEventsRepositoryName(AggregateCommit aggregateCommit)
        {
            // mynkow if(Environment.GetEnvironmentVariable("ForceCronusChecks"))
            // if (boundedContext)

            var boundedContext = aggregateCommit.BoundedContext;
            return GetEventsRepositoryName(boundedContext);
        }

        public string GetEventsRepositoryName(string boundedContext)
        {
            string tableName;
            if (!eventsRepositoryName.TryGetValue(boundedContext, out tableName))
            {
                tableName = String.Format("{0}Events", boundedContext).ToLowerInvariant();
                eventsRepositoryName.TryAdd(boundedContext, tableName);
            }
            return tableName;
        }
    }
}