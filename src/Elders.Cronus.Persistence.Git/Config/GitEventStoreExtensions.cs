using System;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Serializer;
using Elders.Cronus.IocContainer;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Config;

namespace Elders.Cronus.Persistence.Cassandra.Config
{
    public static class GitEventStoreExtensions
    {
        public static T UseGitEventStore<T>(this T self, Action<GitEventStoreSettings> configure) where T : IConsumerSettings<ICommand>
        {
            GitEventStoreSettings settings = new GitEventStoreSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T SetLocation<T>(this T self, string location) where T : IGitEventStoreSettings
        {
            self.Location = location;

            return self;
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Type aggregateStatesAssembly) where T : IGitEventStoreSettings
        {
            return self.SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Assembly aggregateStatesAssembly) where T : IGitEventStoreSettings
        {
            self.BoundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            self.EventStoreTableNameStrategy = new RepositoryPerBoundedContext();
            return self;
        }

        public static T WithNewStorageIfNotExists<T>(this T self) where T : IGitEventStoreSettings
        {
            var storageManager = new GitEventStoreStorageManager(self.Location, self.EventStoreTableNameStrategy, self.BoundedContext);
            storageManager.CreateStorage();
            return self;
        }
    }

    public interface IGitEventStoreSettings : IEventStoreSettings
    {
        string Location { get; set; }
        string KeySpace { get; set; }
        IGitEventStoreRepositoryNameStrategy EventStoreTableNameStrategy { get; set; }
    }

    public class GitEventStoreSettings : SettingsBuilder, IGitEventStoreSettings
    {
        public GitEventStoreSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder) { }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            IGitEventStoreSettings settings = this as IGitEventStoreSettings;

            builder.Container.RegisterSingleton<IAggregateRevisionService>(() => new InMemoryAggregateRevisionService(), builder.Name);
            var eventStore = new GitEventStore(settings.Location, settings.EventStoreTableNameStrategy, builder.Container.Resolve<ISerializer>());
            var aggregateRepository = new AggregateRepository(eventStore, builder.Container.Resolve<IPublisher<IEvent>>(builder.Name), builder.Container.Resolve<IAggregateRevisionService>(builder.Name));
            //var player = new CassandraEventStorePlayer(settings.Session, settings.EventStoreTableNameStrategy, builder.Container.Resolve<ISerializer>());

            builder.Container.RegisterSingleton<IAggregateRepository>(() => aggregateRepository, builder.Name);
            builder.Container.RegisterSingleton<IEventStore>(() => eventStore, builder.Name);
        }

        string IEventStoreSettings.BoundedContext { get; set; }

        string IGitEventStoreSettings.Location { get; set; }

        IGitEventStoreRepositoryNameStrategy IGitEventStoreSettings.EventStoreTableNameStrategy { get; set; }

        string IGitEventStoreSettings.KeySpace { get; set; }
    }
}