namespace Meetup.EventSourcing.Domain
{
    public abstract class IntegrationEvent
    {
        public abstract string GetEventType();
    }
}
