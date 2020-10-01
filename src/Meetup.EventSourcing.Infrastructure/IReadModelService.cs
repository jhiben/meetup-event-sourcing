using System.Collections.Generic;

namespace Meetup.EventSourcing.Infrastructure
{
    public interface IReadModelService
    {
        IAsyncEnumerable<EmployeeSummary> GetAllSummaries();
    }
}