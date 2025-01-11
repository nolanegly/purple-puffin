namespace PurplePuffin.Events;

public enum EventType
{
    Uninitialized,
    GamepadConnected,
    GamepadDisconnected,
    TransitionRequested,
    StartNewGameRequested,
    PauseGameRequested,
    UnpauseGameRequested,
    QuitGameRequested,
}