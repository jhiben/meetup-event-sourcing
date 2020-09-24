using System;
using System.Collections.Generic;

namespace Meetup.EventSourcing.Domain
{
    public interface IAggregate
    {
        Guid Id { get; }

        string GetStreamName();

        IEnumerable<IntegrationEvent> GetIntegrationEvents();
    }
}
