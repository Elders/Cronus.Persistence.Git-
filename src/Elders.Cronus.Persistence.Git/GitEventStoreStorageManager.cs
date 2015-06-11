using System;
using System.IO;
using Elders.Cronus.EventStore;
using LibGit2Sharp;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class GitEventStoreStorageManager : IEventStoreStorageManager
    {
        private readonly IGitEventStoreRepositoryNameStrategy repositoryNameStrategy;
        private readonly string location;
        private readonly string boundedContext;

        public GitEventStoreStorageManager(string location, IGitEventStoreRepositoryNameStrategy repositoryNameStrategy, string boundedContext)
        {
            this.boundedContext = boundedContext;
            this.location = location;
            this.repositoryNameStrategy = repositoryNameStrategy;
        }

        public void CreateEventsStorage()
        {
            var name = repositoryNameStrategy.GetEventsRepositoryName(boundedContext);
            string repository = Path.Combine(location, name);
            if (Directory.Exists(repository))
                return;

            Repository.Init(repository);
            using (var repo = new Repository(repository))
            {
                Signature author = new Signature("Elders.Cronus", "oss.elders@gmail.com", DateTime.Now);
                Signature committer = author;
                Commit commit = repo.Commit(boundedContext + " was initialized. https://github.com/Elders", author, committer, new CommitOptions() { AllowEmptyCommit = true });
            }
        }

        public void CreateStorage()
        {
            CreateEventsStorage();
            CreateSnapshotsStorage();
        }

        public void CreateSnapshotsStorage()
        {
            //var createSnapshotsTable = String.Format(CreateSnapshotsTableTemplate, tableNameStrategy.GetSnapshotsTableName()).ToLower();
            //session.Execute(createSnapshotsTable);
        }
    }
}
