namespace PurplePuffin.Events;

public enum EventType
{
    Uninitialized,
    TransitionRequested,
    MainMenuRequested,
    StartNewGameRequested,
    PauseGameRequested,
    UnpauseGameRequested,
    OptionsMenuRequested,
    QuitGameRequested,
}