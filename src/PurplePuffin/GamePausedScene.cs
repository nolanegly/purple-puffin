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
    private readonly List<Event> _eventsToReturn = new();
    
    private SharedContent _sharedContent;
    private Vector2 _centerTopScene;
    private float _messageOffset = 0.0f;
    private int _messageDirection = 1;

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
    
    public override Event[] Update(GameTime gameTime)
    {
        // handle player input
        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || 
                                                              Keyboard.GetState().IsKeyDown(Keys.Space))
            _eventsToReturn.Add(new Event(EventType.UnpauseGameRequested));
        
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
        
        _spriteBatch.DrawString(_sharedContent.ArialFont, message, messagePos, Color.LightGreen,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
    }
}