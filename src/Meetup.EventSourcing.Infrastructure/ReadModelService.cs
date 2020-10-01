using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;

namespace Meetup.EventSourcing.Infrastructure
{
    public class ReadModelService : IReadModelService
    {
        private readonly Container _container;

        public ReadModelService(CosmosClient client)
        {
            _ = client ?? throw new ArgumentNullException(nameof(client));

            _container = client.GetContainer("event-store", "summaries-meetup");
        }

        public async IAsyncEnumerable<EmployeeSummary> GetAllSummaries()
        {
            var iterator = _container.GetItemQueryIterator<EmployeeSummary>();

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    yield return item;
                }
            }
        }
    }
}
