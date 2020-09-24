using Newtonsoft.Json;
using System;

namespace Meetup.EventSourcing.Infrastructure
{
    public class EmployeeSummary
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
