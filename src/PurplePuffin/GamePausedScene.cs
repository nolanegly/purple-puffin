using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class GamePausedScene : Scene
{
    private readonly List<EventBase> _eventsToReturn = new();
    
    private SharedContent _sharedContent;
    private float _messageOffset = 0.0f;
    private int _messageDirection = 1;
    private float _messageAlpha = 1.0f;

    private TransitionStateEnum _transitionState = TransitionStateEnum.None;
    

    public GamePausedScene(SceneResources sceneResources) : base(sceneResources)
    {
    }

    public override SceneTypeEnum SceneType { get; } = SceneTypeEnum.GamePaused;

    public void LoadContent(SharedContent sharedContent)
    {
        _sharedContent = sharedContent;
    }
    
    public override EventBase[] Update(GameTime gameTime)
    {
        // handle player input
        if (_inputState.IsKeyTriggered(Keys.Space))
        {
            _eventsToReturn.Add(new TransitionEvent(new SceneTransition
            {
                OldState = SceneStateEnum.GamePaused,
                NewState = SceneStateEnum.Game,
                DegreeStepAmount = 0.1f
            }));
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
        var centerTopScene = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 4);
        var messagePos = new Vector2(centerTopScene.X + (centerTopScene.X * _messageOffset), centerTopScene.Y);
        var color = Color.LightGreen * _messageAlpha;
        _textBatch.DrawString(_sharedContent.ArialFont, message, messagePos, color,
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