namespace PurplePuffin.SceneManagement;

public class SceneTransition
{
    public static float SlowStep = 0.01f;
    public static float MediumStep = 0.03f;
    
    public SceneStateEnum OldState { get; init; }
    public SceneStateEnum NewState { get; init; }
    public float DegreeStepAmount { get; init; }

    public override string ToString()
    {
        return $"{nameof(OldState)}: {OldState}, {nameof(NewState)}: {NewState}, {nameof(DegreeStepAmount)}: {DegreeStepAmount}";
    }
}