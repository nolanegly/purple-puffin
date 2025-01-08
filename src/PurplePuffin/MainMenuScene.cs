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
    private readonly InputState _inputState;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<EventBase> _eventsToReturn = new();

    private readonly Desktop _desktop = new();
    
    public MainMenuScene(InputState inputState, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.MainMenu;

        _inputState = inputState;
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public override void LoadContent(SharedContent sharedContent)
    {
        base.LoadContent(sharedContent);
        
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
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        _desktop.Render();

        DrawDefaultTransitionIfNeeded(_graphicsDevice, _spriteBatch);
    }
    
    private void MenuStartNewGameOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.StartNewGameRequested));
    }
    
    private void MenuOptionsOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new TransitionEvent(new SceneTransition
        {
            OldState = SceneStateEnum.MainMenu,
            NewState = SceneStateEnum.OptionMenu,
            DegreeStepAmount = SceneTransition.MediumStep
        }));
        
    }
    
    private void MenuQuitOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.QuitGameRequested));
    }
}