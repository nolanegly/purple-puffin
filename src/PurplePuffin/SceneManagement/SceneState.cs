using System.Linq;
using PurplePuffin.Events;

namespace PurplePuffin.SceneManagement;

/// <summary>
/// Represents how a scene manager should route player input, and what scenes to potentially render 
/// </summary>
public class SceneState
{
    public SceneStateEnum CurrState;
    // TODO: this array could pretty much be a stack (or the scenes need a z-index).
    // When rendering multiple overlapping display areas, we have to know an ordering
    // to draw foreground/background correctly between the overlapping images.
    public SceneTypeEnum[] ActiveScenes;
    public SceneTransition Transition;
    public float TransitionDegree;

    public static SceneState FromDefinition(SceneStateDefinition definition)
    {
        return new SceneState
        {
            CurrState = definition.SceneState,
            ActiveScenes = definition.ActiveScenes.ToArray(),
            Transition = null,
            TransitionDegree = 0.0f
        };
    }
}