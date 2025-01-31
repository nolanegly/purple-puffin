using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class GameScene : Scene
{
    private readonly List<EventBase> _eventsToReturn = new();
    
    private TransitionStateEnum _transitionState = TransitionStateEnum.None;

    public GameScene(SceneResources resources) : base(resources)
    {
    }
    
    public override SceneTypeEnum SceneType { get; } = SceneTypeEnum.Game;

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
        
        // return any events
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
    }
    
    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Draw(SharedContent.ScaleExperiment, new Vector2(0, 0), Color.White);
        DrawDefaultTransitionIfNeeded(_spriteBatch);
    }
}