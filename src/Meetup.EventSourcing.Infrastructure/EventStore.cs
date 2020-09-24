using Meetup.EventSourcing.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meetup.EventSourcing.Infrastructure
{
    public class EventStore : IEventStore
    {
        private readonly Container _container;

        public EventStore(CosmosClient client)
        {
            _ = client ?? throw new ArgumentNullException(nameof(client));

            _container = client.GetContainer("event-store", "events");
        }

        public async IAsyncEnumerable<StoreEvent> GetEvents(string stream)
        {
            var iterator = _container.GetItemLinqQueryable<StoreEvent>()
                .Where(e => e.Stream == stream)
                .ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    yield return item;
                }
            }
        }

        public async Task PublishEvents(string stream, IEnumerable<IntegrationEvent> integrationEvents)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                var e = new StoreEvent(stream, integrationEvent.GetEventType(), integrationEvent);
                await _container.CreateItemAsync(e);
            }
        }
    }
}
