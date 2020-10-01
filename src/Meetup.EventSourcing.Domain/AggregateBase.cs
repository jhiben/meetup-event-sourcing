using System;
using System.Collections.Generic;

namespace Meetup.EventSourcing.Domain
{
    public abstract class AggregateBase : IAggregate
    {
        private readonly List<IntegrationEvent> _integrationEvents = new List<IntegrationEvent>();

        public Guid Id { get; }

        protected AggregateBase(Guid id)
        {
            Id = id;
        }

        IEnumerable<IntegrationEvent> IAggregate.GetIntegrationEvents() => _integrationEvents;

        protected void AddIntegrationEvent(IntegrationEvent integrationEvent) => _integrationEvents.Add(integrationEvent);
    }
}
