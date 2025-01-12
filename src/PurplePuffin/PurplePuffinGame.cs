using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Myra;
using PurplePuffin.Events;
using PurplePuffin.SceneManagement;

namespace PurplePuffin;

public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private int _windowWidth;
    private int _windowHeight;
    
    private InputState _inputState;
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
        _inputState = new InputState();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _sharedContent = new SharedContent();
    }

    protected override void Initialize()
    {
        this.Activated += OnGameActivated;
        this.Deactivated += OnGameDeactivated;
        
        InitializeGraphicsDisplay();
        if (!_graphics.IsFullScreen)
            ToggleFullscreen(true);
        
        // Initialize Myra (the UI library) 
        MyraEnvironment.Game = this;
        
        // Fetch the input state once before the main Update() loop starts to ensure
        // both previous and current state are populated
        _inputState.GetState();

        // Set initial starting volume to music player before loading Options screen,
        // so the UI controls will be set to a matching state
        MediaPlayer.Volume = Defaults.MusicPlayerVolume;
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _titleScene = new TitleScene(_inputState, GraphicsDevice, _spriteBatch);
        _scenes.Add(_titleScene);
        _mainMenuScene = new MainMenuScene(_inputState, GraphicsDevice, _spriteBatch);
        _scenes.Add(_mainMenuScene);
        _optionsMenuScene = new OptionsMenuScene(_inputState, GraphicsDevice, _spriteBatch);
        _scenes.Add(_optionsMenuScene);
        _gameScene = new GameScene(_inputState, GraphicsDevice, _spriteBatch);
        _scenes.Add(_gameScene);
        _gamePausedScene = new GamePausedScene(_inputState, GraphicsDevice, _spriteBatch);
        _scenes.Add(_gamePausedScene);

        _sceneState = SceneState.FromDefinition(SceneStateDefinition.Title);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sharedContent.PlaceholderPixel = new Texture2D(GraphicsDevice, 1, 1);
        _sharedContent.PlaceholderPixel.SetData<Color>(new Color[] { Color.Black });
        
        _sharedContent.ArialFont = Content.Load<SpriteFont>("arial");

        _titleScene.LoadContent(_sharedContent);
        _mainMenuScene.LoadContent(_sharedContent);
        _optionsMenuScene.LoadContent(_sharedContent);
        _gameScene.LoadContent(_sharedContent);
        _gamePausedScene.LoadContent(_sharedContent);
        
        
        // Naive "how do I make this work" code for music
        try
        {
            _backgroundSong = Content.Load<Song>("music\\Juhani Junkala [Chiptune Adventures] 4. Stage Select");
        
            // stop if something else playing/paused
            if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();

            MediaPlayer.Play(_backgroundSong);
            MediaPlayer.IsRepeating = true;
        }
        catch (NoAudioHardwareException e)
        {
            System.Diagnostics.Debug.WriteLine($"WARN: caught NoAudioHardwareException {e}");
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Not quite sure what to do with gamepad connect/disconnect events yet.
        // Might handle here, or might save the return value with SceneType.Input
        // or something, and possibly pass to active scenes?
        var inputEvents = _inputState.GetState();

        // detect keyboard command to toggle fullscreen
        if (_inputState.IsKeyTriggered(Keys.Enter) &&
            (_inputState.IsKeyDown(Keys.LeftAlt) || _inputState.IsKeyDown(Keys.RightAlt)))
        {
            ToggleFullscreen(!_graphics.IsFullScreen);
        }

        // call Update on each active scene, collecting any events
        var events = new Dictionary<SceneTypeEnum, EventBase[]>();

        foreach (var activeSceneType in _sceneState.ActiveScenes)
        {
            var activeScene = _scenes.Single(s => s.SceneType == activeSceneType);
            events.Add(activeScene.SceneType, activeScene.Update(gameTime));
        }
        
        // select tuple of requesting scene and event
        var sceneAndEvents = events.SelectMany(pair => 
            pair.Value.Select(@event => new { pair.Key, @event }))
            .ToList();

        // Handle transition event first (choose one if there are multiple transitions for some reason, that shouldn't happen)
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
            if (activeScene.ShouldBeDrawn == false) continue;
            activeScene.Draw(gameTime);
        }
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    private void InitializeGraphicsDisplay()
    {
        // Log current and supported display modes for troubleshooting purposes 
        var c = GraphicsDevice.Adapter.CurrentDisplayMode;
        System.Diagnostics.Debug.WriteLine($"Current display mode: {c.Width}x{c.Height}, Format: {c.Format} TitleSafeArea: {c.TitleSafeArea}");
        System.Diagnostics.Debug.WriteLine("Supported display modes are:");
        var supported = GraphicsDevice.Adapter.SupportedDisplayModes.ToArray();
        for (var i = 0; i < supported.Length; i++)
        {
            var s = supported[i];
            System.Diagnostics.Debug.WriteLine($"{i:D3}: {s.Width}x{s.Height}, Format: {s.Format} TitleSafeArea: {s.TitleSafeArea}");
        }
            
        
        
        Window.ClientSizeChanged += WindowOnClientSizeChanged;
        Window.OrientationChanged += WindowOnOrientationChanged; // This will probably never fire unless on mobile platform
        
        Window.AllowAltF4 = true;
        Window.IsBorderless = false;
        Window.Title = "Prototype - Purple Puffin";
        Window.AllowUserResizing = true;
        // TODO: figure out what default size and aspect ratio make sense for the actual game when windowed.
        
        _graphics.HardwareModeSwitch = true;
    }

    private void ToggleFullscreen(bool isFullScreen)
    {
        if (isFullScreen)
        {
            // Save window size before going full screen in case we toggle back
            _windowWidth = Window.ClientBounds.Width;
            _windowHeight = Window.ClientBounds.Height;

            // Set the resolution equal to current display resolution.
            // If you don't, you might get the display's lowest resolution by default.
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
        }
        else
        {
            _graphics.PreferredBackBufferWidth = _windowWidth;
            _graphics.PreferredBackBufferHeight = _windowHeight;
            _graphics.IsFullScreen = false;
        }

        _graphics.ApplyChanges();
    }

    private void OnGameActivated(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Game activated");
    }

    private void OnGameDeactivated(object sender, EventArgs e)
    {
        // TODO: this will fire whenever the game screen (whether windowed or full screen) loses focus.
        // Will need to handle this in the game loop, so play pauses automatically
        System.Diagnostics.Debug.WriteLine("Game deactivated");
    }

    private void WindowOnOrientationChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"WindowOnOrientationChanged: {Window.CurrentOrientation}");
    }

    private void WindowOnClientSizeChanged(object sender, EventArgs e)
    {
        // TODO: need to deal with window size changes to resize the drawn elements properly,
        // and potentially adding letterboxes or pillarboxes
        System.Diagnostics.Debug.WriteLine($"WindowOnClientSizeChanged: {Window.ClientBounds}");
    }
}
