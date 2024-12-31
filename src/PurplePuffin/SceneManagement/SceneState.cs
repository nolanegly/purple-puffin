namespace PurplePuffin.SceneManagement;

/// <summary>
/// Represents how a scene manager should route player input, and what scenes to potentially render 
/// </summary>
public class SceneState
{
    public SceneStateEnum CurrState;
    public SceneTypeEnum HasInputFocus;
    public SceneTypeEnum[] ActiveScenes;
}