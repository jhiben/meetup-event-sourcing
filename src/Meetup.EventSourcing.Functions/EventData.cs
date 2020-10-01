using Newtonsoft.Json.Linq;

namespace Meetup.EventSourcing.Functions
{
    public class EventData
    {
        public string EventType { get; set; }

        public JToken Data { get; set; }
    }
}
