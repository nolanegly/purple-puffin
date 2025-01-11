namespace PurplePuffin.Events;

public class GamepadConnectedEvent(int gamepadIndex) : EventBase
{
    public override EventType EventType => EventType.GamepadConnected;

    public int GamepadIndex { get; init; } = gamepadIndex;
}