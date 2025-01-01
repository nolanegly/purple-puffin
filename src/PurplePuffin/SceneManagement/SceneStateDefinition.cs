using System.Collections.Generic;
using System.Linq;

namespace PurplePuffin.SceneManagement;

/// <summary>
/// Poor man's state machine for defining scene states and what screens are active for that state.
/// A state machine library will support transitions better, but this is usable for now.
/// </summary>
public class SceneStateDefinition
{
    public SceneStateEnum SceneState { get; }
    public SceneTypeEnum[] ActiveScenes { get; }

    private static List<SceneStateDefinition> _definitions = [];

    private SceneStateDefinition(SceneStateEnum sceneState, SceneTypeEnum[] activeScenes)
    {
        SceneState = sceneState;
        ActiveScenes = activeScenes;
        
        _definitions.Add(this);
    }

    public static SceneStateDefinition For(SceneStateEnum sceneState)
    {
        return _definitions.Single(d => d.SceneState == sceneState);
    }
    
    public static SceneStateDefinition Title = new SceneStateDefinition(SceneStateEnum.Title, [SceneTypeEnum.Title]);
    public static SceneStateDefinition MainMenu = new SceneStateDefinition(SceneStateEnum.MainMenu, [SceneTypeEnum.MainMenu]);
    public static SceneStateDefinition OptionsMenu = new SceneStateDefinition(SceneStateEnum.OptionMenu, [SceneTypeEnum.OptionsMenu]);
    public static SceneStateDefinition GamePlay = new SceneStateDefinition(SceneStateEnum.Game, [SceneTypeEnum.Game]);
    public static SceneStateDefinition GamePaused = new SceneStateDefinition(SceneStateEnum.GamePaused, [SceneTypeEnum.GamePaused]);
}