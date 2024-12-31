namespace PurplePuffin.SceneManagement;

/// <summary>
/// State of the game - at a minimum what scene(s) are active and potentially some differentiating state.
/// </summary>
/// <remarks>
/// For simple games the states will match the scenes. More complex games may have states with multiple active scenes.
/// For example: imagine a multiplayer game with a main game loop on a Game scene. The player can bring up an
/// Inventory scene to change equipment, but this does NOT pause the game for other players. This could be represented
/// with SceneTypeEnum values of Game and Inventory, and SceneStateEnum values of Game and GameAndInventory. Normal play
/// would have a SceneState with
/// CurrState: GameScene, HasInputFocus: GameScene, and ActiveScenes: [GameScene].
/// Pulling up the inventory overlay scene would have a SceneState with
/// CurrState: GameAndInventory, HasInputFocus: InventoryScene, ActiveScenes: [GameScene, InventoryScene]
/// This would allow the scene manager to continue calling the game scene's Draw() to continue rendering the action
/// behind a semi-transparent inventory screen. 
/// </remarks>
public enum SceneStateEnum
{
    Title,
    MainMenu,
    OptionMenu,
    Game
}