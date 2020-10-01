using System;

namespace Meetup.EventSourcing.Infrastructure
{
    public class StoreEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Stream { get; }

        public string EventType { get; }

        public object Data { get; }

        public StoreEvent(string stream, string eventType, object data)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
