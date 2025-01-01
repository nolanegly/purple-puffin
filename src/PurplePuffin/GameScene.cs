using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class GameScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<Event> _eventsToReturn = new();
    
    private SharedContent _sharedContent;
    private Vector2 _centerScene;
    private float _messageOffset = 0.0f;
    private int _messageDirection = 1;

    public bool _pauseGameHandled = false;

    public GameScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.Game;
        
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public void LoadContent(SharedContent sharedContent)
    {
        _sharedContent = sharedContent;
        
        var viewport = _graphicsDevice.Viewport;
        _centerScene = new Vector2(viewport.Width / 2, viewport.Height / 2);        
    }
    
    public override Event[] Update(GameTime gameTime)
    {
        // handle player input
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                                                              Keyboard.GetState().IsKeyDown(Keys.Escape))
            _eventsToReturn.Add(new Event(EventType.QuitGameRequested));
        // Don't return any other simultaneous requests if a quit was requested
        else if (!_pauseGameHandled && (
                     Keyboard.GetState().IsKeyDown(Keys.Space))
                )
        {
            _eventsToReturn.Add(new Event(EventType.PauseGameRequested));
            _pauseGameHandled = true;
        }
        else if (_pauseGameHandled && (
                     Keyboard.GetState().IsKeyUp(Keys.Space))
                )
        {
            _pauseGameHandled = false;
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
        var message = "Game scene";
        var messageOrigin = _sharedContent.ArialFont.MeasureString(message) / 2;
        var messagePos = new Vector2(_centerScene.X + (_centerScene.X * _messageOffset), _centerScene.Y);
        
        _spriteBatch.DrawString(_sharedContent.ArialFont, message, messagePos, Color.LightGreen,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
    }
}