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
        
        _sceneState = new SceneState
        {
            CurrState = SceneStateEnum.Title,
            HasInputFocus = SceneTypeEnum.Title,
            ActiveScenes = [SceneTypeEnum.Title]
        };

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
        var focusedScene = _scenes.Single(s => s.SceneType == _sceneState.HasInputFocus);
        var events = focusedScene.Update(gameTime);

        foreach (var e in events)
        {
            if (e.EventType == EventType.QuitGameRequested)
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
            if (e.EventType == EventType.StartNewGameRequested)
            {
                _sceneState.CurrState = SceneStateEnum.Game;
                _sceneState.HasInputFocus = SceneTypeEnum.Game;
                _sceneState.ActiveScenes = [SceneTypeEnum.Game];
            }
            
            if (e.EventType == EventType.MainMenuRequested)
            {
                _sceneState.CurrState = SceneStateEnum.MainMenu;
                _sceneState.HasInputFocus = SceneTypeEnum.MainMenu;
                _sceneState.ActiveScenes = [SceneTypeEnum.MainMenu];
            }
            
            if (e.EventType == EventType.OptionsMenuRequested)
            {
                _sceneState.CurrState = SceneStateEnum.OptionMenu;
                _sceneState.HasInputFocus = SceneTypeEnum.OptionsMenu;
                _sceneState.ActiveScenes = [SceneTypeEnum.OptionsMenu];
            }
            
            if (e.EventType == EventType.PauseGameRequested)
            {
                _sceneState.CurrState = SceneStateEnum.GamePaused;
                _sceneState.HasInputFocus = SceneTypeEnum.GamePaused;
                _sceneState.ActiveScenes = [SceneTypeEnum.GamePaused];
            }
            
            if (e.EventType == EventType.UnpauseGameRequested)
            {
                _sceneState.CurrState = SceneStateEnum.Game;
                _sceneState.HasInputFocus = SceneTypeEnum.Game;
                _sceneState.ActiveScenes = [SceneTypeEnum.Game];
            }
        }
        
        // var processedFocusScene = _sceneState.HasInputFocus // save to local variable to avoid calling it twice when beginning transitions
        
        // TODO: consider adding a GetSceneTransitionRequests() and calling that on the _sceneState.HasInputFocus scene.
        // Only allow transition changes via that method. Would allow always calling Update on all active scenes without
        // conditional logic. 
        // On second thought: what if you have an Inventory screen over a Game screen, and the player dies while looking
        // at their inventory? the Game scene needs to raise a transition event because the player died, even though it
        // the "input focused" scene. Like case 1/case 2 for multiple transitions, the responsibility falls on the
        // scenes to work together correctly. Probably remove the whole HasInputFocus field. But the dev needs to design
        // their game states and code their scenes so only one scene per SceneState is handling user input (probably, lol).
        // So instead of calling a single scene, need to iterate over all _sceneState.ActiveScenes (in same order as
        // Draw() probably) and conglomerate all returned events together. 
        //      ----------------------------------------------------------------------------------------------------------------------
        //      TLDR: highly consider removing the HasInputFocus property from previous commit.
        //      ----------------------------------------------------------------------------------------------------------------------
        
        
        // call Update() on processedFocusScene so it can get get user input
        // if transition requested and _sceneState.CurrState is NOT Transitioning
        //      - set _sceneState.CurrState to Transitioning
        //      - set _sceneState.Transition from null to Transition instance
        //      - set _sceneState.HasInputFocus to the NewState scene
        //      - add NewState scenes to _sceneState.ActiveScenes
        //      - call BeginTransition() on all _sceneState.ActiveScenes
        //      - invoked scenes should set any internal state necessary for imminent StepTransition() or EndTransition() calls
        //      - call Update() on any other _sceneState.ActiveScenes that are not processedFocusScene
        //      - call base.Update()
        //      - return early
        //      - end branch
        // else if transition requested and _sceneState.CurrState IS Transitioning
        //      // TODO: think through logic of overriding an existing transition.
        //      ----------------------------------------------------------------------------------------------------------------------
        //      TLDR: probably don't support it? High effort, low yield.
        //      Have scene manager log a warning if it happens and ignore the 2nd overlapping request
        //      ----------------------------------------------------------------------------------------------------------------------
        //          // The two cases I can think of are
        //          1) reversing the existing transition, A->AB->BA where BA request interrupts AB transition
        //          2) starting another transition before the first finishes, A->AB->BC where BC request interrupts AB transition
        //          Case 1 might be fairly easy to do, a B to A transition can be started with a matching degree and easily
        //          "unwind" or "counteract" the transition. BUT SHOULD BE TESTED :) And is still the responsibility of the
        //          scene to prevent starting a counteracting transition if it doesn't work well (see Case 2).
        //          Case 2 is trickier because the A->B transition has not finished, and the scene manager does not know
        //          if the scene transition implementations can meaningfully handle two simultaneous transitions.
        //          Transitions that slide a screen to the left would work, because you could have two sliding off at
        //          the same time. A fade transition would not really work however.
        //          Therefore, it is the responsibility of the scenes to know if they can handle multiple transitions,
        //          and prevent starting another transition until their current activating one is complete!
        //      - end branch
        // else if no transition requested but _sceneState.CurrState is Transitioning
        //      - step the transition degree
        //      - if degree still < 1.0f
        //          - call StepTransition() on _sceneState.ActiveScenes
        //          - call Update() on all _sceneState.ActiveScenes
        //          - end branch
        //      - else if degree >= 1.0f
        //          - call EndTransition() on all _sceneState.ActiveScenes
        //          - invoked scenes should clean up any internal state from the transition
        //          - OldState scenes should expect they will not get any further Update() or Draw() calls
        //          - set _sceneState.CurrState to Transition.NewState
        //          - remove Transition.OldState scenes from _sceneState.ActiveScenes
        //          - set _sceneState.Transition to null
        // end else
        // // for condition: no transition requested and _sceneState.CurrState is not Transitioning - nothing extra to do
        // - call Update() on all _sceneState.ActiveScenes
        
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
