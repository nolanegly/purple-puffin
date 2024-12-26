using Microsoft.Xna.Framework;

namespace PurplePuffin;

public enum SceneType
{
    Undefined,
    Title,
    Game
}

public abstract class Scene
{
    public SceneType SceneType = SceneType.Undefined;
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}