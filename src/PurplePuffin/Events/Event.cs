using System;

namespace PurplePuffin.Events;

public class Event : EventBase
{
    public override EventType EventType { get; }

    public Event(EventType eventType)
    {
        if (eventType == EventType.Uninitialized)
            throw new Exception($"Cannot construct event with type {eventType}");
        
        EventType = eventType;
    }
}