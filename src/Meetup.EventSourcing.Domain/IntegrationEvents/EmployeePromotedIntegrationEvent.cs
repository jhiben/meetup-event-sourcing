using System;

namespace Meetup.EventSourcing.Domain.IntegrationEvents
{
    public class EmployeePromotedIntegrationEvent : EmployeeIntegrationEventBase
    {
        public override string GetEventType() => "Employee.Promoted";

        public string Role { get; }

        public double Salary { get; }

        public EmployeePromotedIntegrationEvent(Guid employeeId, Role role, double salary)
            : base(employeeId)
        {
            Role = role.ToString();
            Salary = salary;
        }
    }
}
