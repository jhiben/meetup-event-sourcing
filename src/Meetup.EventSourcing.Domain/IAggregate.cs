using System;
using System.Collections.Generic;

namespace Meetup.EventSourcing.Domain
{
    public interface IAggregate
    {
        public Guid Id { get; }

        IEnumerable<IntegrationEvent> GetIntegrationEvents();
    }
}
