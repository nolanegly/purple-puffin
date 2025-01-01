using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Myra;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SharedContent _sharedContent;
    
    private SceneState _sceneState;
    
    private TitleScene _titleScene;
    private MainMenuScene _mainMenuScene;
    private OptionsMenuScene _optionsMenuScene;
    private GameScene _gameScene;
    private GamePausedScene _gamePausedScene;
    
    private readonly List<Scene> _scenes = new(4);

    private Song _backgroundSong;

    public PurplePuffinGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _sharedContent = new SharedContent();
    }

    protected override void Initialize()
    {
        // Initialize Myra (the UI library) 
        MyraEnvironment.Game = this;

        // Set initial starting volume to music player before loading Options screen,
        // so the UI controls will be set to a matching state
        MediaPlayer.Volume = Defaults.MusicPlayerVolume;
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _titleScene = new TitleScene(GraphicsDevice, _spriteBatch);
        _scenes.Add(_titleScene);
        _mainMenuScene = new MainMenuScene(GraphicsDevice, _spriteBatch);
        _scenes.Add(_mainMenuScene);
        _optionsMenuScene = new OptionsMenuScene(GraphicsDevice);
        _scenes.Add(_optionsMenuScene);
        _gameScene = new GameScene(GraphicsDevice, _spriteBatch);
        _scenes.Add(_gameScene);
        _gamePausedScene = new GamePausedScene(GraphicsDevice, _spriteBatch);
        _scenes.Add(_gamePausedScene);

        _sceneState = SceneState.FromDefinition(SceneStateDefinition.Title);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sharedContent.ArialFont = Content.Load<SpriteFont>("arial");

        _titleScene.LoadContent(_sharedContent);
        _mainMenuScene.LoadContent();
        _optionsMenuScene.LoadContent();
        _gameScene.LoadContent(_sharedContent);
        _gamePausedScene.LoadContent(_sharedContent);
        
        
        // Naive "how do I make this work" code for music
        // TODO: this throws an exception on my machine if my Bluetooth headphones aren't turned on!
        // Need to add exception handling in case no audio hardware is found
        _backgroundSong = Content.Load<Song>("music\\Juhani Junkala [Chiptune Adventures] 4. Stage Select");
        
        // stop if something else playing/paused
        if (MediaPlayer.State != MediaState.Stopped)
            MediaPlayer.Stop();

        MediaPlayer.Play(_backgroundSong);
        MediaPlayer.IsRepeating = true;
    }

    protected override void Update(GameTime gameTime)
    {
        var events = new Dictionary<SceneTypeEnum, EventBase[]>();

        foreach (var activeSceneType in _sceneState.ActiveScenes)
        {
            var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
            events.Add(activeScene.SceneType, activeScene.Update(gameTime));
        }
        
        var sceneAndEvents = events.SelectMany(pair => 
            pair.Value.Select(@event => new { pair.Key, @event }))
            .ToList();

        // Handle transition event first (choose one if there are multiples for some reason)
        var sceneTransitionEvent = sceneAndEvents.FirstOrDefault(e => e.@event is TransitionEvent);
        var sceneTransition = (sceneTransitionEvent?.@event as TransitionEvent)?.SceneTransition;
        
        // Three possible transition states
        // 1. We are not currently in a transition and need to Begin() one
        // 2. OR We are currently in a transition and need to Step() or End() it
        //    2a. If a second transition was requested before first one is finished, log warning that it's being ignored 
        // 3. OR We are not currently in a transition
        
        // Handle 1. We are not currently in a transition and need to Begin() one
        if (sceneTransition != null && _sceneState.CurrState != SceneStateEnum.Transitioning)
        {
            // Set state to Transitioning
            _sceneState.CurrState = SceneStateEnum.Transitioning;
            _sceneState.Transition = sceneTransition;
            _sceneState.TransitionDegree = 0.0f;
            
            // Add any scenes in new state that are not already active 
            var scenesToAdd = SceneStateDefinition.For(_sceneState.Transition.NewState).ActiveScenes
                .Where(s => !_sceneState.ActiveScenes.Contains(s))
                .ToArray();
            _sceneState.ActiveScenes = _sceneState.ActiveScenes.Union(scenesToAdd).ToArray();
            
            // All active scenes should set any internal state necessary for imminent StepTransition() or EndTransition() calls
            foreach (var activeSceneType in _sceneState.ActiveScenes)
            {
                var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
                activeScene.BeginTransition(_sceneState.Transition, gameTime);
            }
            
            // Call Update() on any newly added scenes
            var newlyAddedScenes = _scenes.Where(s => scenesToAdd.Contains(s.SceneType)).ToArray();
            foreach (var newlyAddedScene in newlyAddedScenes)
            {
                events.Add(newlyAddedScene.SceneType, newlyAddedScene.Update(gameTime));
            }
            
            // Don't handle the transition event again in the non-transition event processing 
            sceneAndEvents.Remove(sceneTransitionEvent);
        }
        // Handle 2. We are currently in a transition and need to Step() or End() it
        else if (_sceneState.CurrState == SceneStateEnum.Transitioning)
        {
            // Handle 2a. If a second transition was requested before first one is finished, log warning that it's being ignored
            //
            // In the future, support for canceling the transition in progress could be added for certain transitions.
            // E.g. Legend of Zelda LTTP where inventory menu slides down from top, but can be cancelled and sent
            // back upwards before it comes down fully. Alternatively, the transition could be done instantaneous
            // and the inventory screen could do its slide-in animation before accepting player input except
            // a command to close the inventory screen (starting a slide-out animation before triggering an
            // instant transition to game screen).
            if (sceneTransition != null)
            {
                System.Diagnostics.Debug.WriteLine($"WARN: ignoring transition request while already transitioning. {sceneTransitionEvent.Key} requested transition: {sceneTransition}");
                
                // Don't handle the transition event again in the non-transition event processing 
                sceneAndEvents.Remove(sceneTransitionEvent);
            }

            _sceneState.TransitionDegree += _sceneState.Transition.DegreeStepAmount;
            if (_sceneState.TransitionDegree < 1.0f)
            {
                foreach (var activeSceneType in _sceneState.ActiveScenes)
                {
                    var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
                    activeScene.StepTransition(_sceneState.TransitionDegree, gameTime);
                }
            }
            else
            {
                foreach (var activeSceneType in _sceneState.ActiveScenes)
                {
                    var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
                    activeScene.EndTransition(gameTime);
                }
                
                // end the transition
                _sceneState.CurrState = _sceneState.Transition.NewState;
                _sceneState.Transition = null;
                _sceneState.TransitionDegree = 0.0f;
                
                // remove any scenes that are not present in the current state
                _sceneState.ActiveScenes = SceneStateDefinition.For(_sceneState.CurrState).ActiveScenes;
            }
        }
        
        // After processing transition event, handle any other non-transition events
        foreach (var sceneEvent in sceneAndEvents)
        {
            if (sceneEvent.@event.EventType == EventType.TransitionRequested)
            {
                System.Diagnostics.Debug.WriteLine($"WARN: ignoring extra transition requests in same Update() frame. {sceneTransitionEvent.Key} requested transition: {sceneTransition}, but {sceneTransitionEvent.Key} already started: {sceneTransition}");
            }
            
            if (sceneEvent.@event.EventType == EventType.QuitGameRequested)
                Exit();

            // TODO: convert these state changes to use TransitionEvent instead
            if (sceneEvent.@event.EventType == EventType.StartNewGameRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.GamePlay);
            
            if (sceneEvent.@event.EventType == EventType.MainMenuRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.MainMenu);
            
            if (sceneEvent.@event.EventType == EventType.OptionsMenuRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.OptionsMenu);
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        foreach (var activeSceneType in _sceneState.ActiveScenes)
        {
            var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
            activeScene.Draw(gameTime);
        }
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
