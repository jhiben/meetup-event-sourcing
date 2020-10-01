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

            _container = client.GetContainer("event-store", "events-meetup");
        }

        public async IAsyncEnumerable<StoreEvent> GetEvents(string stream)
        {
            var items = _container.GetItemLinqQueryable<StoreEvent>()
                .Where(e => e.Stream == stream)
                .ToFeedIterator();

            while (items.HasMoreResults)
            {
                foreach (var item in await items.ReadNextAsync())
                {
                    yield return item;
                }
            }
        }

        public async Task Publish(string stream, IEnumerable<IntegrationEvent> integrationEvents)
        {
            foreach (var item in integrationEvents)
            {
                var e = new StoreEvent(stream, item.GetEventType(), item);
                await _container.CreateItemAsync(e);
            }
        }
    }
}
