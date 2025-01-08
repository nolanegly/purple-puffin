using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PurplePuffin.Events;

namespace PurplePuffin.SceneManagement;

public abstract class Scene
{
    protected SharedContent SharedContent;
    
    private float _transitionAlpha = 0.0f;
    private TransitionStateEnum _transitionState = TransitionStateEnum.None;
    
    // Every scene should set this in their constructor.
    // I debated making it an abstract property to ensure it got implemented,
    // but am avoiding simple, "pass-through" getter/setters 
    public SceneTypeEnum SceneType = SceneTypeEnum.Uninitialized;

    /// <summary>
    /// Instructs scene manager whether this scene wants to be drawn or not.
    /// Primarily intended for coordinating transitions between scenes.
    /// </summary>
    public bool ShouldBeDrawn { get; private set; } = true;

    public virtual void LoadContent(SharedContent sharedContent)
    {
        SharedContent = sharedContent;
    }
    
    public abstract EventBase[] Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);

    /// <summary>
    /// Signals a scene a coordinated transition is starting
    /// </summary>
    public virtual void BeginTransition(SceneTransition sceneTransition, GameTime gameTime)
    {
        var oldScenes = SceneStateDefinition.For(sceneTransition.OldState).ActiveScenes;
        var newScenes = SceneStateDefinition.For(sceneTransition.NewState).ActiveScenes;
        var isOneToOne = oldScenes.Count() == 1 && newScenes.Count() == 1;

        if (oldScenes.Contains(SceneType) && !newScenes.Contains(SceneType))
        {
            if (!isOneToOne)
            {
                System.Diagnostics.Debug.WriteLine($"WARN: Scene {SceneType} is using base BeginTransition but the base method only supports one to one transitions. Given transition: {sceneTransition}");
                return;
            }
            
            _transitionState = TransitionStateEnum.Out;
            _transitionAlpha = 0.0f;
        }
        else if (!oldScenes.Contains(SceneType) && newScenes.Contains(SceneType))
        {
            if (!isOneToOne)
            {
                System.Diagnostics.Debug.WriteLine($"WARN: Scene {SceneType} is using base BeginTransition but the base method only supports one to one transitions. Given transition: {sceneTransition}");
                return;
            }

            _transitionState = TransitionStateEnum.In;
            _transitionAlpha = 1.0f;
            ShouldBeDrawn = false; // do not start drawing inbound scene until outbound scene has been faded out
        }
    }

    /// <summary>
    /// Signals scenes the current transition increment 
    /// </summary>
    public virtual void StepTransition(float currDegree, GameTime gameTime)
    {
        if (_transitionState == TransitionStateEnum.None) return;

        // The first half of the transition is fading out the old scene, and then the second half
        // is fading in the new scene. Note splitting the transition between the two scenes requires
        // doubling the TransitionDegree value so both fades finish within its increment from 0 to 1.
        if (_transitionState == TransitionStateEnum.Out && ShouldBeDrawn)
        {
            if (currDegree < 0.5f)
                _transitionAlpha = currDegree * 2;
            else
                ShouldBeDrawn = false;
        }
        else if (_transitionState == TransitionStateEnum.In && currDegree > 0.5f)
        {
            ShouldBeDrawn = true; // don't bother with a conditional, just keep setting it multiple times
            
            // How the math works for setting fade in on the last half of the transition:
            // currDegree: 0.5, 0.6, 0.75, 0.9, 1.0
            // alpha:      1.0, 0.9, 0.5, 0.1, 0.0
            _transitionAlpha = 1 - ((currDegree - 0.5f) * 2);
        }
    }

    /// <summary>
    /// Signals scenes a coordinated transition is ending
    /// </summary>
    public virtual void EndTransition(GameTime gameTime)
    {
        _transitionState = TransitionStateEnum.None;
    }

    /// <summary>
    /// Invoke in your Draw() method when you aren't handling scene transitions yourself 
    /// </summary>
    protected void DrawDefaultTransitionIfNeeded(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        if (_transitionState == TransitionStateEnum.None || ShouldBeDrawn == false) return;

        // don't draw the inbound scene on the fade out, or the outbound scene on the fade in
        if (ShouldBeDrawn == false) return;
        
        spriteBatch.Draw(SharedContent.PlaceholderPixel, new Vector2(0, 0), null, 
            new Color(0, 0, 0, _transitionAlpha), 0f, Vector2.Zero, 
            new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
            SpriteEffects.None, 0);
    }
}