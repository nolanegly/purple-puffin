using System.Linq;

namespace PurplePuffin.SceneManagement;

/// <summary>
/// Represents how a scene manager should route player input, and what scenes to potentially render 
/// </summary>
public class SceneState
{
    public SceneStateEnum CurrState;
    public SceneTypeEnum[] ActiveScenes;

    public static SceneState FromDefinition(SceneStateDefinition definition)
    {
        return new SceneState
        {
            CurrState = definition.SceneState,
            ActiveScenes = definition.ActiveScenes.ToArray()
        };
    }
}