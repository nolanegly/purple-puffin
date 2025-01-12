using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class TitleScene : Scene
{
    private readonly InputState _inputState;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly List<EventBase> _eventsToReturn = new();

    private TitleSceneStateEnum _state = TitleSceneStateEnum.NotYetDisplayed;
    private TimeSpan? _timeSinceFirstUpdate;

    public TitleScene(InputState inputState, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        SceneType = SceneTypeEnum.Title;

        _inputState = inputState;
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public override EventBase[] Update(GameTime gameTime)
    {
        if (_state == TitleSceneStateEnum.NotYetDisplayed)
        {
            _timeSinceFirstUpdate = gameTime.TotalGameTime;
            _state = TitleSceneStateEnum.Displaying;
        }
        else if (_state == TitleSceneStateEnum.Displaying)
        {
            WaitTwoSecondsAndThenAdvanceToMainMenu(gameTime);
        }
        
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }

    public override void Draw(GameTime gameTime)
    {
        var message = "Title scene";
        var messageOrigin = SharedContent.ArialFont.MeasureString(message) / 2;
        
        // TODO: title screen text is not centered now that we're launching in fullscreen for some reason
        var titlePos = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);

        _spriteBatch.DrawString(SharedContent.ArialFont, message, titlePos, Color.LightGreen,
            0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
        
        DrawDefaultTransitionIfNeeded(_graphicsDevice, _spriteBatch);
    }
    
    private void WaitTwoSecondsAndThenAdvanceToMainMenu(GameTime gameTime)
    {
        if (gameTime.TotalGameTime > _timeSinceFirstUpdate.Value.Add(new TimeSpan(0, 0, 2)))
        {
            _eventsToReturn.Add(new TransitionEvent(new SceneTransition
            {
                OldState = SceneStateEnum.Title,
                NewState = SceneStateEnum.MainMenu,
                DegreeStepAmount = SceneTransition.SlowStep
            }));
            
            _state = TitleSceneStateEnum.DoneDisplaying;
        }
    }

    private enum TitleSceneStateEnum
    {
        Uninitialized,
        NotYetDisplayed,
        Displaying,
        DoneDisplaying
    }
}