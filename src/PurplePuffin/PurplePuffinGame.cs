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
    // Our target/base resolution we will render at, and then scale up or down based on actual physical resolution.
    // Based on Steam statistics I'm not worried about the approximately 7% of users with a resolution lower than HD.
    public const int VirtualWidth = 1920;
    public const int VirtualHeight = 1080;
    
    // TODO: these should be compiler directives, to avoid doing the same logic checks over and over again at runtime.
    public const bool AllowSuboptimalResolution = false; // Don't let the game run smaller than virtual resolution
    public const bool PreserveAspectRatio = true; // Apply letter/pillar boxing to prevent aspect ratio distortion
    
    private GraphicsDeviceManager _graphics;
    // Dimensions of the screen before toggling into fullsize, so they can be restored when toggling out.
    private int _windowWidth;
    private int _windowHeight;
    
    private InputState _inputState;
    private SpriteBatch _spriteBatch;
    private Matrix _spriteScale;

    private SharedContent _sharedContent;
    
    private SceneState _sceneState;
    
    private TitleScene _titleScene;
    private MainMenuScene _mainMenuScene;
    private OptionsMenuScene _optionsMenuScene;
    private GameScene _gameScene;
    private GamePausedScene _gamePausedScene;
    
    private readonly List<Scene> _scenes = new(5);

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
        // Make config changes that make constant start/stop for devs easier.
        ApplyDebugConvenienceOverrides();
        
        this.Activated += OnGameActivated;
        this.Deactivated += OnGameDeactivated;
        
        InitializeGraphicsDisplay();
        if (Defaults.StartFullScreen == true)
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

        _sceneState = SceneState.FromDefinition(Defaults.InitialScene);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sharedContent.PlaceholderPixel = new Texture2D(GraphicsDevice, 1, 1);
        _sharedContent.PlaceholderPixel.SetData<Color>(new Color[] { Color.Black });
        
        _sharedContent.ArialFont = Content.Load<SpriteFont>("arial");
        _sharedContent.ScaleExperiment = Content.Load<Texture2D>("scale-experiment");

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

        CheckForScaleExperimentChanges();
        
        base.Update(gameTime);
    }

    private void CheckForScaleExperimentChanges()
    {
        // Convenience method to quickly toggle between some specific resolutions for testing.
        
        if (_inputState.IsKeyTriggered(Keys.Y))
        {
            // 0.833% of our virtual/target resolution (but still same aspect ratio)
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.ApplyChanges();
            UpdateTargetRenderAreaAndScaling();
        }
        else if (_inputState.IsKeyTriggered(Keys.U))
        {
            // Our virtual/target resolution
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.ApplyChanges();
            UpdateTargetRenderAreaAndScaling();
        }
        else if (_inputState.IsKeyTriggered(Keys.I))
        {
            // larger than our virtual/target resolution
            _graphics.PreferredBackBufferHeight = 1440;
            _graphics.PreferredBackBufferWidth = 2560;
            _graphics.ApplyChanges();
            UpdateTargetRenderAreaAndScaling();
        }
        else if (_inputState.IsKeyTriggered(Keys.O))
        {
            // 2x our virtual/target resolution (native display resolution)
            // This will only display actual size when in full screen mode.
            _graphics.PreferredBackBufferHeight = 2160;
            _graphics.PreferredBackBufferWidth = 3840;
            _graphics.ApplyChanges();
            UpdateTargetRenderAreaAndScaling();
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        // Whatever color is used here will be the color letterboxes/pillarboxes when aspect ratio is being maintained
        // on a display area larger than we are rendering when preventing distortion.
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin(transformMatrix: _spriteScale);

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
        var curr = GraphicsDevice.Adapter.CurrentDisplayMode;
        System.Diagnostics.Debug.WriteLine($"Current display mode: {curr.Width}x{curr.Height}, Format: {curr.Format} TitleSafeArea: {curr.TitleSafeArea}");
        System.Diagnostics.Debug.WriteLine("Supported display modes are:");
        var supported = GraphicsDevice.Adapter.SupportedDisplayModes.ToArray();
        for (var i = 0; i < supported.Length; i++)
        {
            var s = supported[i];
            System.Diagnostics.Debug.WriteLine($"{i:D3}: {s.Width}x{s.Height}, Format: {s.Format} TitleSafeArea: {s.TitleSafeArea}");
        }

        if (AllowSuboptimalResolution == false)
        {
            // Exit immediately if the player's screen does not support at least our virtual resolution.
            if (curr.Height < VirtualHeight || curr.Width < VirtualWidth)
            {
                var msg = $"This lowest screen resolution this game currently supports is {VirtualWidth}x{VirtualHeight}, the current resolution is {curr.Width}x{curr.Height}. The game will now exit.";
                System.Diagnostics.Debug.WriteLine(msg);
                
                // This unfortunately doesn't always work. Supported when project targets WindowsDX, but not
                // OpenGL (which is what you presumably get when target is set to Any).
                // See https://community.monogame.net/t/microsoft-xna-framework-input-messagebox/20056
                // MessageBox.Show("Cannot Run", $"This lowest screen resolution this game currently supports is {VirtualWidth}x{VirtualHeight}, the current resolution is {curr.Width}x{curr.Height}. The game will now exit.", ["OK"]);
                
                // TODO: low priority, figure out how to tell player their resolution is not supported before just silently exiting.
                Exit();
            }
        }
        
        Window.ClientSizeChanged += WindowOnClientSizeChanged;
        Window.OrientationChanged += WindowOnOrientationChanged; // This will probably never fire unless on mobile platform
        
        Window.AllowAltF4 = true;
        Window.IsBorderless = false;
        Window.Title = "Prototype - Purple Puffin";
        Window.AllowUserResizing = true;
        
        _graphics.HardwareModeSwitch = true;

        // Start game at our virtual resolution
        _graphics.PreferredBackBufferHeight = VirtualHeight;
        _graphics.PreferredBackBufferWidth = VirtualWidth;
        
        // Now that we've set our buffer size, initialize our rendering area and scaling
        UpdateTargetRenderAreaAndScaling();
    }
    
    private void UpdateTargetRenderAreaAndScaling()
    {
        var actualWidth = _graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
        var actualHeight = _graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;
        Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: back buffer size is now {actualWidth}x{actualHeight}");
        
        // Handle at least one dimension is below virtual resolution
        if (_graphics.GraphicsDevice.PresentationParameters.BackBufferWidth < VirtualWidth ||
            _graphics.GraphicsDevice.PresentationParameters.BackBufferHeight < VirtualHeight)
        {
            if (AllowSuboptimalResolution == false)
            {
                // If below virtual resolution, force suboptimal size back to at least our virtual size
                Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: resizing back buffer size back to {VirtualWidth}x{VirtualHeight}");
                _graphics.PreferredBackBufferHeight = VirtualHeight;
                _graphics.PreferredBackBufferWidth = VirtualWidth;
                _graphics.ApplyChanges();
            }
            else
            {
                // Set drawing area to match available area. It will be distorted because it
                // will be scaled down below our virtual resolution, and the aspect ratio is probably different.
                _graphics.GraphicsDevice.Viewport = new Viewport(0, 0, actualWidth, actualHeight, 0, 1);
            }
        }
        // The display area is at least the size of our virtual resolution
        else
        {
            if (PreserveAspectRatio == true)
            {
                Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: applying any needed boxing for {actualWidth}x{actualHeight} to be drawn at aspect ratio");

                var scaleIncrementX = actualWidth / VirtualWidth;
                var scaleIncrementY = actualHeight / VirtualHeight;
                if (scaleIncrementX != scaleIncrementY)
                {
                    // Don't scale unevenly, e.g. 2 horizontally but only 1 vertically. This will distort the output.
                    var scaleMax = Math.Min(scaleIncrementX, scaleIncrementY);
                    Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: restricting scale to {scaleMax} (X scales {scaleIncrementX}, Y scales {scaleIncrementY})");
                    scaleIncrementX = scaleMax;
                    scaleIncrementY = scaleMax;
                }
            
                // Calculate the padding needed for the number of increments on each dimension.
                var totalHorizontalWidth = VirtualWidth * scaleIncrementX;
                var horizontalPadNeeded = (actualWidth - totalHorizontalWidth) / 2;
                Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: X renders {totalHorizontalWidth} wide with {horizontalPadNeeded} horizontal pad");            
            
                var totalVerticalHeight = VirtualHeight * scaleIncrementY;
                var verticalPadNeeded = (actualHeight - totalVerticalHeight) / 2;
                Debug.WriteLine($"UpdateTargetRenderAreaAndScaling: Y renders {totalVerticalHeight} wide with {verticalPadNeeded} vertical pad");
            
                // Center the drawing into an area that matches the virtual resolution target ratio.
                _graphics.GraphicsDevice.Viewport =
                    new Viewport(horizontalPadNeeded, verticalPadNeeded,
                        totalHorizontalWidth, totalVerticalHeight, 0, 1);
            }
        }
        
        // Now that we've established what area of screen to draw into (if needed), determine the needed scaling to fill it.
        var scaleX = _graphics.GraphicsDevice.Viewport.Width / (float) VirtualWidth;
        var scaleY = _graphics.GraphicsDevice.Viewport.Height / (float) VirtualHeight;
        Debug.WriteLine($"UpdateSpriteScale: setting sprite scale to {scaleX} X and {scaleY} Y");
        // Create the scale transform for Draw (do NOT scale the sprite depth, keep Z=1).
        _spriteScale = Matrix.CreateScale(scaleX, scaleY, 1);
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

        // Update our rendering when the drawing area size changes.
        UpdateTargetRenderAreaAndScaling();
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
        System.Diagnostics.Debug.WriteLine($"WindowOnClientSizeChanged: {Window.ClientBounds}");
        
        // Update our rendering when drawing area size changes.
        UpdateTargetRenderAreaAndScaling();
    }
    
    private static void ApplyDebugConvenienceOverrides()
    {
        Defaults.MusicPlayerVolume = 0.0f; // annoying while coding
        Defaults.StartFullScreen = false; // takes less time to launch
        Defaults.InitialScene = SceneStateDefinition.GamePlay; // jump straight to the scene being worked on
    }
}
