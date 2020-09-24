using Newtonsoft.Json.Linq;
using System;

namespace Meetup.EventSourcing.Functions
{
    public class EventData
    {
        public Guid Id { get; set; }

        public string Stream { get; set; }

        public string EventType { get; set; }

        public JToken Data { get; set; }
    }
}
