using PurplePuffin.SceneManagement;

namespace PurplePuffin.Events;

public class TransitionEvent(SceneTransition sceneTransition) : EventBase
{
    public override EventType EventType => EventType.TransitionRequested;
    public SceneTransition SceneTransition { get; init; } = sceneTransition;
}