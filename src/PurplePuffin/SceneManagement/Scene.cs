using Microsoft.Xna.Framework;
using PurplePuffin.Events;

namespace PurplePuffin.SceneManagement;

public abstract class Scene
{
    public SceneTypeEnum SceneType = SceneTypeEnum.Uninitialized;
    public abstract EventBase[] Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);

    /// <summary>
    /// Signals a scene a coordinated transition is starting
    /// </summary>
    public virtual void BeginTransition(SceneTransition sceneTransition, GameTime gameTime)
    {
    }

    /// <summary>
    /// Signals scenes the current transition increment 
    /// </summary>
    public virtual void StepTransition(float currDegree, GameTime gameTime)
    {
    }

    /// <summary>
    /// Signals scenes a coordinated transition is ending
    /// </summary>
    public virtual void EndTransition(GameTime gameTime)
    {
    }
}