using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;

namespace PurplePuffin;

public class MainMenuScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Desktop _desktop = new();

    private readonly List<Event> _eventsToReturn = new(); 
    
    public MainMenuScene(GraphicsDevice graphicsDevice)
    {
        SceneType = SceneType.MainMenu;

        _graphicsDevice = graphicsDevice;
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
    
    public override Event[] Update(GameTime gameTime)
    {
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.Black);
        _desktop.Render();
    }
    
    private void MenuStartNewGameOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.StartNewGameRequested));
    }
    
    private void MenuOptionsOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.OptionsMenuRequested));
    }
    
    private void MenuQuitOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.QuitGameRequested));
    }
}