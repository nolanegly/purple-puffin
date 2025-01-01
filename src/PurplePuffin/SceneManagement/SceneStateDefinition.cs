namespace PurplePuffin.SceneManagement;

/// <summary>
/// Poor man's state machine for defining scene states and what screens are active for that state.
/// A state machine library will support transitions better, but this is usable for now.
/// </summary>
public class SceneStateDefinition
{
    public SceneStateEnum SceneState { get; }
    public SceneTypeEnum[] ActiveScenes { get; }

    private SceneStateDefinition(SceneStateEnum sceneState, SceneTypeEnum[] activeScenes)
    {
        SceneState = sceneState;
        ActiveScenes = activeScenes;
    }
    
    public static SceneStateDefinition Title = new SceneStateDefinition(SceneStateEnum.Title, [SceneTypeEnum.Title]);
    public static SceneStateDefinition MainMenu = new SceneStateDefinition(SceneStateEnum.MainMenu, [SceneTypeEnum.MainMenu]);
    public static SceneStateDefinition OptionsMenu = new SceneStateDefinition(SceneStateEnum.OptionMenu, [SceneTypeEnum.OptionsMenu]);
    public static SceneStateDefinition GamePlay = new SceneStateDefinition(SceneStateEnum.Game, [SceneTypeEnum.Game]);
    public static SceneStateDefinition GamePaused = new SceneStateDefinition(SceneStateEnum.GamePaused, [SceneTypeEnum.GamePaused]);
}