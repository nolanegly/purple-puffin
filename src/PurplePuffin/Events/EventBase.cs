namespace PurplePuffin.Events;

public abstract class EventBase
{
    public abstract EventType EventType { get; }
}