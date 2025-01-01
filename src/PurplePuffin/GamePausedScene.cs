using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class GamePausedScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<EventBase> _eventsToReturn = new();
    
    private SharedContent _sharedContent;
    private Vector2 _centerTopScene;
    private float _messageOffset = 0.0f;
    private int _messageDirection = 1;
    private float _messageAlpha = 1.0f;

    private bool _unpauseGameHandled = false;
    private TransitionStateEnum _transitionState = TransitionStateEnum.None;
    

    public GamePausedScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.GamePaused;
        
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public void LoadContent(SharedContent sharedContent)
    {
        _sharedContent = sharedContent;
        
        var viewport = _graphicsDevice.Viewport;
        _centerTopScene = new Vector2(viewport.Width / 2, viewport.Height / 4);        
    }
    
    public override EventBase[] Update(GameTime gameTime)
    {
        // handle player input
        if (!_unpauseGameHandled && (
                     Keyboard.GetState().IsKeyDown(Keys.Space))
                )
        {
            _eventsToReturn.Add(new TransitionEvent(new SceneTransition
            {
                OldState = SceneStateEnum.GamePaused,
                NewState = SceneStateEnum.Game,
                DegreeStepAmount = 0.1f
            }));
            
            _unpauseGameHandled = true;
        }
        else if (_unpauseGameHandled && (
                     Keyboard.GetState().IsKeyUp(Keys.Space))
                )
        {
            _unpauseGameHandled = false;
        }
        
        // animate the message
        if (_messageOffset >= 1.0f)
            _messageDirection = -1;
        else if (_messageOffset <= -1.0f)
            _messageDirection = 1;
        
        _messageOffset += (0.01f * _messageDirection);
        
        // return any events
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        var message = "-=[ PAUSE ]=-";
        var messageOrigin = _sharedContent.ArialFont.MeasureString(message) / 2;
        var messagePos = new Vector2(_centerTopScene.X + (_centerTopScene.X * _messageOffset), _centerTopScene.Y);
        var color = Color.LightGreen * _messageAlpha;
        _spriteBatch.DrawString(_sharedContent.ArialFont, message, messagePos, color,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
    }
    
    public override void BeginTransition(SceneTransition sceneTransition, GameTime gameTime)
    {
        if (sceneTransition.OldState == SceneStateEnum.GamePaused &&
                 sceneTransition.NewState == SceneStateEnum.Game)
        {
            _transitionState = TransitionStateEnum.Out;
        }
        else if (sceneTransition.OldState == SceneStateEnum.Game &&
            sceneTransition.NewState == SceneStateEnum.GamePaused)
        {
            // Tell the game paused scene it has already handled the spacebar being pressed, because coming into
            // the pause screen the player is still holding down the space bar as the transition is executed and
            // not setting this to true causes the pause scene to immediately fire an unpause request.
            // This isn't a great technique, but it works for now and we'll add a better way to handle the beginning
            // of transitions shortly.
            _unpauseGameHandled = true;
            
            _transitionState = TransitionStateEnum.In;
        }
    }

    public override void StepTransition(float currDegree, GameTime gameTime)
    {
        switch (_transitionState)
        {
            case TransitionStateEnum.None:
                return;
            case TransitionStateEnum.In:
                _messageAlpha = currDegree;
                break;
            case TransitionStateEnum.Out:
                _messageAlpha = 1.0f - currDegree;
                break;
        }
    }

    public override void EndTransition(GameTime gameTime)
    {
        if (_transitionState == TransitionStateEnum.None) return;

        _messageAlpha = _transitionState == TransitionStateEnum.In
            ? 1.0f
            : 0.0f;
        _transitionState = TransitionStateEnum.None;
    }    
}