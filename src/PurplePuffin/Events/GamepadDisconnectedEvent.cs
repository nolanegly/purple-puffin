namespace PurplePuffin.Events;

public class GamepadDisconnectedEvent(int gamepadIndex) : EventBase
{
    public override EventType EventType => EventType.GamepadDisconnected;

    public int GamepadIndex { get; init; } = gamepadIndex;
}