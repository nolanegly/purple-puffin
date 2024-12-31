using Microsoft.Xna.Framework;
using PurplePuffin.Events;

namespace PurplePuffin.SceneManagement;

public abstract class Scene
{
    public SceneType SceneType = SceneType.Uninitialized;
    public abstract Event[] Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}