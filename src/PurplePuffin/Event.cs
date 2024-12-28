using System;

namespace PurplePuffin;

public enum EventType
{
    Uninitialized,
    MainMenuRequested,
    StartNewGameRequested,
    OptionsMenuRequested,
    QuitGameRequested,
}

public class Event
{
    public EventType EventType { get; init; }

    public Event(EventType eventType)
    {
        if (eventType == EventType.Uninitialized)
            throw new Exception($"Cannot construct event with type {eventType}");
        
        EventType = eventType;
    }
}