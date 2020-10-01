using System;

namespace Meetup.EventSourcing.Domain.IntegrationEvents
{
    public abstract class EmployeeIntegrationEventBase : IntegrationEvent
    {
        public Guid EmployeeId { get; }

        protected EmployeeIntegrationEventBase(Guid employeeId)
        {
            EmployeeId = employeeId;
        }
    }
}
