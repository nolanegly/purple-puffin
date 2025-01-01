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
    
    // TODO: this list could pretty much be a stack (or the scenes need a z-index).
    // When rendering multiple overlapping display areas, we have to know an ordering
    // to draw foreground/background correctly between the overlapping images.
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
        var focusedScenes = _scenes
            .Where(s => _sceneState.ActiveScenes.Contains(s.SceneType))
            .ToArray();

        var events = new Dictionary<SceneTypeEnum, Event[]>();

        foreach (var activeSceneType in _sceneState.ActiveScenes)
        {
            var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
            events.Add(activeScene.SceneType, activeScene.Update(gameTime));
        }
        
        var allEvents = events.SelectMany(pair => 
            pair.Value.Select(@event => new { pair.Key, @event }))
            .ToArray();
        
        foreach (var sceneEvent in allEvents)
        {
            if (sceneEvent.@event.EventType == EventType.QuitGameRequested)
                Exit();

            // TODO: this works but has a hidden issue (the transition from the title scene has the same problem)
            // This straightforward change of active scene causes a processing mismatch between the framework
            // calling this Update() and Draw() method. Scene A is active on Update(), the logic changes the
            // active scene to Scene B, and then Scene B is active on Draw(). But Scene B did not handle the 
            // Update() for this update/draw cycle.
            // Instead of switching the active scene directly, either:
            // 1) a scene transition needs to be queued and handled at the top of the next Update(), so the subsequent
            //    Draw() is called on the same scene that Update() was
            // 2) when changing scenes, need to call Update() on the newly active scene (which means two scenes will
            //    have been updated, potentially throwing the average elapsed time...dunno if that matters?)
            if (sceneEvent.@event.EventType == EventType.StartNewGameRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.GamePlay);
            
            if (sceneEvent.@event.EventType == EventType.MainMenuRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.MainMenu);
            
            if (sceneEvent.@event.EventType == EventType.OptionsMenuRequested)
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.OptionsMenu);
            
            if (sceneEvent.@event.EventType == EventType.PauseGameRequested)
            {
                // Tell the game paused scene it has already handled the spacebar being pressed, because coming into
                // the pause screen the player is still holding down the space bar as the transition is executed and
                // not setting this to true causes the pause scene to immediately fire an unpause request.
                // This isn't a great technique, but it works for now and we'll add a better way to handle the beginning
                // of transitions shortly.
                _gamePausedScene._unpauseGameHandled = true;
                
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.GamePaused);
            }
            
            if (sceneEvent.@event.EventType == EventType.UnpauseGameRequested)
            {
                _gameScene._pauseGameHandled = true;
                
                _sceneState = SceneState.FromDefinition(SceneStateDefinition.GamePlay);
            }
        }
        
        // var updatedScenes = _sceneState.ActiveScenes.ToArray();
        // call Update() on updatedScenes so it can get  user input
        // if transition requested and _sceneState.CurrState is NOT Transitioning
        //      - set _sceneState.CurrState to Transitioning
        //      - set _sceneState.Transition from null to Transition instance
        //      - add NewState scenes to _sceneState.ActiveScenes (reference SceneStateDefinitions resource)
        //      - call BeginTransition() on all _sceneState.ActiveScenes
        //      - invoked scenes should set any internal state necessary for imminent StepTransition() or EndTransition() calls
        //      - call Update() on any other _sceneState.ActiveScenes that are not in updatedScenes
        //      - end branch
        // else if transition requested and _sceneState.CurrState IS Transitioning
        //      - log warning that the new transition request is being ignored
        //      // In the future, support for canceling the transition in progress could be added for certain transitions.
        //      // E.g. Legend of Zelda LTTP where inventory menu slides down from top, but can be cancelled and sent
        //      // back upwards before it comes down fully. Alternatively, the transition could be done instantaneous
        //      // and the inventory screen could do its slide-in animation before accepting player input except
        //      // a command to close the inventory screen (starting a slide-out animation before triggering an
        //      // instant transition to game screen).
        //      - end branch
        // else if no transition requested but _sceneState.CurrState is Transitioning
        //      - step the transition degree
        //      - if degree still < 1.0f
        //          - call StepTransition() on _sceneState.ActiveScenes
        //          - end branch
        //      - else if degree >= 1.0f
        //          - call EndTransition() on all _sceneState.ActiveScenes
        //          - invoked scenes should clean up any internal state from the transition
        //          - OldState scenes not also in NewState should expect they will not get any
        //            further Update() or Draw() calls
        //          - set _sceneState.CurrState to Transition.NewState
        //          - remove Transition.OldState scenes not also in NewState from _sceneState.ActiveScenes
        //            (reference SceneStateDefinitions resource)
        //          - set _sceneState.Transition to null
        // end else
        
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
