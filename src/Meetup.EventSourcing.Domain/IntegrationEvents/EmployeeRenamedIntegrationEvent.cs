using System;

namespace Meetup.EventSourcing.Domain.IntegrationEvents
{
    public class EmployeeRenamedIntegrationEvent : EmployeeIntegrationEventBase
    {
        public override string GetEventType() => "Employee.Renamed";

        public string Name { get; }

        public EmployeeRenamedIntegrationEvent(Guid employeeId, string name)
            : base(employeeId)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
