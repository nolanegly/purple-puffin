using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class TitleScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<Event> _eventsToReturn = new();

    private SharedContent _sharedContent;
    private Vector2 _titlePos;
    private TimeSpan? _timeSinceFirstUpdate;

    public TitleScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneType.Title;
        
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public void LoadContent(SharedContent sharedContent)
    {
        _sharedContent = sharedContent;
        
        var viewport = _graphicsDevice.Viewport;
        _titlePos = new Vector2(viewport.Width / 2, viewport.Height / 2);
    }
    
    public override Event[] Update(GameTime gameTime)
    {
        WaitTwoSecondsAndThenAdvanceToMainMenu(gameTime);
        
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        var message = "Title scene";
        var messageOrigin = _sharedContent.ArialFont.MeasureString(message) / 2;
        _spriteBatch.DrawString(_sharedContent.ArialFont, message, _titlePos, Color.LightGreen,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
    }
    
    private void WaitTwoSecondsAndThenAdvanceToMainMenu(GameTime gameTime)
    {
        if (_timeSinceFirstUpdate == null)
        {
            _timeSinceFirstUpdate = gameTime.TotalGameTime;
        }
        else if (gameTime.TotalGameTime > _timeSinceFirstUpdate.Value.Add(new TimeSpan(0, 0, 2)))
        {
            _eventsToReturn.Add(new Event(EventType.MainMenuRequested));
        }
    }
}