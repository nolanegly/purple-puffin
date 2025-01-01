namespace PurplePuffin.SceneManagement;

public class SceneTransition
{
    public SceneStateEnum OldState { get; init; }
    public SceneStateEnum NewState { get; init; }
    public float DegreeStepAmount { get; init; }

    public override string ToString()
    {
        return $"{nameof(OldState)}: {OldState}, {nameof(NewState)}: {NewState}, {nameof(DegreeStepAmount)}: {DegreeStepAmount}";
    }
}