using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class GameScene : Scene
{
    private readonly InputState _inputState;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<EventBase> _eventsToReturn = new();
    
    private Vector2 _centerScene;
    private float _messageOffset = 0.0f;
    private int _messageDirection = 1;
    private float _messageAlpha = 1.0f;

    private TransitionStateEnum _transitionState = TransitionStateEnum.None;

    public GameScene(InputState inputState, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.Game;

        _inputState = inputState;
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public override void LoadContent(SharedContent sharedContent)
    {
        base.LoadContent(sharedContent);
        
        var viewport = _graphicsDevice.Viewport;
        _centerScene = new Vector2(viewport.Width / 2, viewport.Height / 2);        
    }
    
    public override EventBase[] Update(GameTime gameTime)
    {
        // handle player input
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || _inputState.IsKeyTriggered(Keys.Escape))
            _eventsToReturn.Add(new Event(EventType.QuitGameRequested));
        // Don't return any other simultaneous requests if a quit was requested
        else if (_inputState.IsKeyTriggered(Keys.Space))
        {
            _eventsToReturn.Add(new TransitionEvent(new SceneTransition
            {
                OldState = SceneStateEnum.Game,
                NewState = SceneStateEnum.GamePaused,
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
        // var message = "Game scene";
        // var messageOrigin = SharedContent.ArialFont.MeasureString(message) / 2;
        // var messagePos = new Vector2(_centerScene.X + (_centerScene.X * _messageOffset), _centerScene.Y);
        // var color = Color.LightGreen * _messageAlpha;
        // _spriteBatch.DrawString(SharedContent.ArialFont, message, messagePos, color,
        //     0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);

        
        _spriteBatch.Draw(SharedContent.ScaleExperiment, new Vector2(0, 0), Color.White);
    }
    
    public override void BeginTransition(SceneTransition sceneTransition, GameTime gameTime)
    {
        if (sceneTransition.OldState == SceneStateEnum.Game &&
            sceneTransition.NewState == SceneStateEnum.GamePaused)
        {
            _transitionState = TransitionStateEnum.Out;
        }
        else if (sceneTransition.OldState == SceneStateEnum.GamePaused &&
                 sceneTransition.NewState == SceneStateEnum.Game)
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