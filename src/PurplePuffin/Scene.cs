using Microsoft.Xna.Framework;

namespace PurplePuffin;

public enum SceneType
{
    Uninitialized,
    Title,
    MainMenu,
    Game
}

public abstract class Scene
{
    public SceneType SceneType = SceneType.Uninitialized;
    public abstract Event[] Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}