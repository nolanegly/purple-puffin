using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class MainMenuScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<Event> _eventsToReturn = new();

    private readonly Desktop _desktop = new();
    
    private float _transitionDegree = 0.0f;
    private readonly Texture2D _placeholderPixel;
    
    public MainMenuScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.MainMenu;

        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        
        _placeholderPixel = new Texture2D(_graphicsDevice, 1, 1);
        _placeholderPixel.SetData<Color>(new Color[] { Color.Black });    
    }

    public void LoadContent()
    {
        var ui = new MainMenuMyra();
        
        // UI control event handlers will be invoked by Myra during the Draw() invocation. Any game logic
        // (as opposed to changing a UI state on the scene) should be queued for the Update() call to handle.
        ui._menuStartNewGame.Selected += MenuStartNewGameOnSelected;
        ui._menuOptions.Selected += MenuOptionsOnSelected;
        ui._menuQuit.Selected += MenuQuitOnSelected;

        _desktop.Root = ui;
    }
    
    public override EventBase[] Update(GameTime gameTime)
    {
        if (_transitionDegree > 0.0f)
            _transitionDegree += 0.07f;

        if (_transitionDegree >= 1.0f)
        {
            _transitionDegree = 0.0f;
            _eventsToReturn.Add(new Event(EventType.OptionsMenuRequested));
        }
        
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.Black);
        _desktop.Render();

        // If we're transitioning, draw the fade out
        if (_transitionDegree > 0.0f)
        {
            _spriteBatch.Draw(_placeholderPixel, new Vector2(0, 0), null, 
                new Color(0, 0, 0, _transitionDegree), 0f, Vector2.Zero, 
                new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height),
                SpriteEffects.None, 0);
        }
    }
    
    private void MenuStartNewGameOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.StartNewGameRequested));
    }
    
    private void MenuOptionsOnSelected(object sender, EventArgs e)
    {
        _transitionDegree = 0.01f;
    }
    
    private void MenuQuitOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.QuitGameRequested));
    }
}