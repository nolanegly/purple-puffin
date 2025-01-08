namespace PurplePuffin.Events;

public enum EventType
{
    Uninitialized,
    TransitionRequested,
    StartNewGameRequested,
    PauseGameRequested,
    UnpauseGameRequested,
    QuitGameRequested,
}