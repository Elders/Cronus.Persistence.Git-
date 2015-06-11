using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore;
using Elders.Cronus.Serializer;
using LibGit2Sharp;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class GitEventStore : IEventStore
    {
        private readonly string location;
        private readonly IGitEventStoreRepositoryNameStrategy tableNameStrategy;
        private readonly ISerializer serializer;

        public GitEventStore(string location, IGitEventStoreRepositoryNameStrategy tableNameStrategy, ISerializer serializer)
        {
            this.location = location;
            this.tableNameStrategy = tableNameStrategy;
            this.serializer = serializer;
        }

        public void Append(AggregateCommit aggregateCommit)
        {
            string repository = Path.Combine(location, tableNameStrategy.GetEventsRepositoryName(aggregateCommit));

            Directory.CreateDirectory(Path.Combine(repository + Convert.ToBase64String(aggregateCommit.AggregateRootId)));

            string data = Convert.ToBase64String(SerializeEvent(aggregateCommit));
            File.WriteAllText(Path.Combine(repository + Convert.ToBase64String(aggregateCommit.AggregateRootId)), data);
            //using (var repo = new Repository(repository))
            //{
            //    var options = new CommitOptions() { AllowEmptyCommit = true };
            //    Signature author = new Signature("Elders.Cronus", "cronus@elders.com", DateTime.Now);
            //    Signature committer = author;

            //    if (TryCheckoutAggregateBranch(repo, aggregateCommit.AggregateRootId))
            //    {
            //        Commit commit = repo.Commit(data, author, committer, options);
            //    }
            //}

        }

        public EventStream Load(IAggregateRootId aggregateId)
        {
            List<AggregateCommit> aggregateCommits = new List<AggregateCommit>();
            string boundedContext = aggregateId.GetType().GetBoundedContext().BoundedContextName;
            string repositoryName = tableNameStrategy.GetEventsRepositoryName(boundedContext);
            string repository = Path.Combine(location, repositoryName);
            using (var repo = new Repository(repository))
            {
                if (TryCheckoutAggregateBranch(repo, aggregateId.RawId))
                {
                    aggregateCommits = (from cc in repo.Commits
                                        select DeserializeGitCommit(cc))
                                        .Take(repo.Commits.Count() - 1)
                                        .ToList();
                }
            }
            return new EventStream(aggregateCommits);
        }

        private bool TryCheckoutAggregateBranch(Repository repo, byte[] aggregateId)
        {
            string aggregateRootIdAsString = ToValidName(Convert.ToBase64String(aggregateId));
            var branch = repo.Branches.Where(x => x.Name == aggregateRootIdAsString).SingleOrDefault();
            //if (branch == null)

            //repo.Checkout(repo.Branches.Where(x => x.Name == "master").Single());
            branch = repo.CreateBranch(aggregateRootIdAsString, repo.Commits.Last().Sha);

            repo.Checkout(branch);
            return true;
        }
        private string ToValidName(string gay)
        {
            return gay.Replace(@"\", "f7a8b9a0-3d20-4dac-a548-7207589ea44c")
                .Replace("/", "849c42e5-afe5-4cdf-a813-6925728440be")
                .Replace(".lock", "49165164-4a65-47a3-91f2-cf06ad5ce24b")
                .Replace("~", "77917641-ff73-4fe8-9093-2db81a9add19")
                .Replace("^", "d2aa8d1a-864b-4d37-961c-f1fe5d39d849")
                .Replace(":", "974e8b70-9f8c-4211-ae21-7ac435d5fded")
                .Replace("\\", "de7444c2-942f-4686-8297-e3db1f653d7c")
                .Replace(".", "24a87896-2723-45ef-9fcb-b39104857861")

                ;
        }
        private byte[] SerializeEvent(AggregateCommit commit)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, commit);
                return stream.ToArray();
            }
        }

        private AggregateCommit DeserializeGitCommit(Commit gitCommit)
        {
            var commitData = Convert.FromBase64String(gitCommit.Message);
            using (var stream = new MemoryStream(commitData))
            {
                return (AggregateCommit)serializer.Deserialize(stream);
            }
        }
    }
}