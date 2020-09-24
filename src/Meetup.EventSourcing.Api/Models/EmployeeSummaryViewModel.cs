using System;

namespace Meetup.EventSourcing.Api.Models
{
    public class EmployeeSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }
    }
}
