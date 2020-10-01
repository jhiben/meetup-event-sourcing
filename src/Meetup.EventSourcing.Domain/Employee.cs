using Meetup.EventSourcing.Domain.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.EventSourcing.Domain
{
    public class Employee : AggregateBase
    {
        public string Name { get; private set; }

        public Role Role { get; private set; }

        public double Salary { get; private set; }

        private Employee(Guid id)
            : base(id)
        {
        }

        public static Employee Create(string name, Role role)
        {
            var employee = new Employee(Guid.NewGuid());

            employee.SetName(name);
            employee.SetRole(role);

            return employee;
        }

        public static Employee From(Guid id, IEnumerable<IntegrationEvent> integrationEvents) =>
            integrationEvents.Aggregate(new Employee(id), (acc, next) => acc.Apply(next));

        public Employee Apply(IntegrationEvent integrationEvent)
        {
            switch (integrationEvent)
            {
                case EmployeeRenamedIntegrationEvent e:
                    Name = e.Name;
                    break;
                case EmployeePromotedIntegrationEvent e:
                    Role = (Role)Enum.Parse(typeof(Role), e.Role);
                    Salary = e.Salary;
                    break;
            }

            return this;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Cannot be empty", nameof(name));
            }

            if (Name != name)
            {
                Name = name;
                AddIntegrationEvent(new EmployeeRenamedIntegrationEvent(Id, Name));
            }
        }

        public void Promote()
        {
            if (Role == Role.ElonMusk)
            {
                return;
            }

            SetRole(Role + 1);
        }

        private void SetRole(Role role)
        {
            if (Role != role)
            {
                Role = role;
                Salary = MapSalary(role);
                AddIntegrationEvent(new EmployeePromotedIntegrationEvent(Id, Role, Salary));
            }
        }

        private double MapSalary(Role role) =>
            role switch
            {
                Role.Developer => 1000,
                Role.CleaningLady => 2000,
                Role.Manager => 5000,
                Role.CTO => 25_000,
                Role.GregYoung => 50_000,
                Role.ElonMusk => 10_000_000,
                _ => throw new InvalidOperationException("Unkown role"),
            };
    }
}
