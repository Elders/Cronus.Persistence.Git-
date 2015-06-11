using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventStore;
using Elders.Cronus.Persistence.Cassandra;
using Elders.Cronus.Serializer.Protoreg;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Persistence.Git.Tests
{

    [Subject("")]
    public class When_create_git_repository
    {
        Establish context = () =>
            {
                aggregateRootId = new TestAggregateId(Guid.NewGuid());

                var repository = @"D:\etc\Elders.Cronus.Persistence.Git\EventStore\";
                var strategy = new RepositoryPerBoundedContext();
                var aa = new GitEventStoreStorageManager(repository, strategy, aggregateRootId.GetType().GetBoundedContext().BoundedContextName);
                aa.CreateStorage();

                var regs = new Protoreg.ProtoRegistration();
                regs.RegisterAssembly<CronusAssemby>();
                regs.RegisterAssembly<IMessage>();
                regs.RegisterAssembly<When_create_git_repository>();

                var serializer = new ProtoregSerializer(regs);
                serializer.Build();

                es = new GitEventStore(repository, new RepositoryPerBoundedContext(), serializer);

            };

        Because of = () =>
            {

            };

        It should_ = () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var theID = Guid.NewGuid();
                    originalCommit = new AggregateCommit(new TestAggregateId(theID), 1, new List<IEvent>() { new TestCreateEvent(aggregateRootId) });
                    es.Append(originalCommit);
                }
            };

        static EventStream eventStream;
        static TestAggregateId aggregateRootId;
        static AggregateCommit originalCommit;
        static IEventStore es;

    }
}
