using Meetup.EventSourcing.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meetup.EventSourcing.Infrastructure
{
    public interface IEventStore
    {
        Task Publish(string stream, IEnumerable<IntegrationEvent> integrationEvents);

        IAsyncEnumerable<StoreEvent> GetEvents(string stream);
    }
}