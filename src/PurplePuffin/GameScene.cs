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
    private Vector2 _gameplayPos;

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
        _gameplayPos = new Vector2(viewport.Width / 2, viewport.Height / 2);        
    }
    
    public override Event[] Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                                                              Keyboard.GetState().IsKeyDown(Keys.Escape))
            _eventsToReturn.Add(new Event(EventType.QuitGameRequested));

        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        var message = "Game scene";
        var messageOrigin = _sharedContent.ArialFont.MeasureString(message) / 2;
        _spriteBatch.DrawString(_sharedContent.ArialFont, message, _gameplayPos, Color.LightGreen,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
    }
}