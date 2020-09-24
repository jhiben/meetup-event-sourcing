using System;

namespace Meetup.EventSourcing.Api.Models
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public double Salary { get; set; }
    }
}
