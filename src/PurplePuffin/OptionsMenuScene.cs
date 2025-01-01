using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Myra.Events;
using Myra.Graphics2D.UI;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class OptionsMenuScene : Scene
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Desktop _desktop = new();
    private readonly List<Event> _eventsToReturn = new();

    public OptionsMenuScene(GraphicsDevice graphicsDevice)
    {
        SceneType = SceneTypeEnum.OptionsMenu;
        _graphicsDevice = graphicsDevice;
    }

    public void LoadContent()
    {
        var ui = new OptionsMenuMyra();
        
        ui._menuBackToMainMenu.Selected += MenuBackToMainMenuOnSelected;

        ui._hsMusicVolume.Value = MediaPlayer.Volume  * 100f;
        ui._hsMusicVolume.ValueChanged += HsMusicVolumeOnValueChanged;

        _desktop.Root = ui;
    }

    private void HsMusicVolumeOnValueChanged(object sender, ValueChangedEventArgs<float> e)
    {
        // Because MediaPlayer is a static, there's little danger in directly
        // updating it from an event triggered by framework's Draw() callstack
        MediaPlayer.Volume = e.NewValue / 100f;
        System.Diagnostics.Debug.WriteLine($"HsMusicVolumeOnValueChanged from {e.OldValue} to {e.NewValue}");
    }

    private void MenuBackToMainMenuOnSelected(object sender, EventArgs e)
    {
        _eventsToReturn.Add(new Event(EventType.MainMenuRequested));
    }

    public override EventBase[] Update(GameTime gameTime)
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
}