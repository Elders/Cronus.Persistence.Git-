using Elders.Cronus.DomainModeling;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "822ac781-619e-4cfe-b0c5-dc4c595320c3")]
    public class TestCreateEvent : IEvent
    {
        TestCreateEvent() { }

        public TestCreateEvent(TestAggregateId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public TestAggregateId Id { get; set; }
    }
}
