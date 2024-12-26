using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PurplePuffin;

public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SharedContent _sharedContent;
    
    private SceneType _sceneType;
    private Scene _activeScene;
    
    private TitleScene _titleScene;
    private GameScene _gameScene;

    private Song _backgroundSong;

    public PurplePuffinGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _sceneType = SceneType.Title;
        _sharedContent = new SharedContent();
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _titleScene = new TitleScene(GraphicsDevice, _spriteBatch);
        _gameScene = new GameScene(GraphicsDevice, _spriteBatch);
        
        _activeScene = _titleScene;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sharedContent.ArialFont = Content.Load<SpriteFont>("arial");
        
        _titleScene.LoadContent(_sharedContent);
        _gameScene.LoadContent(_sharedContent);
        
        
        // Naive "how do I make this work" code for music
        _backgroundSong = Content.Load<Song>("music\\Juhani Junkala [Chiptune Adventures] 4. Stage Select");
        
        // stop if something else playing/paused
        if (MediaPlayer.State != MediaState.Stopped)
            MediaPlayer.Stop();
        
        MediaPlayer.Play(_backgroundSong);
        MediaPlayer.IsRepeating = true;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Having this kind of conditional logic for input based on active scene in the update loop
        // is going to get awkward. We'll extract this out in the future so each scene handles input itself.
        if (_activeScene.SceneType == SceneType.Title)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                _activeScene = _gameScene;
            }
        }
        
        // Naive implementation to control music
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
            MediaPlayer.Volume += 0.05f;
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
            MediaPlayer.Volume -= 0.05f;
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            System.Diagnostics.Debug.WriteLine($"*** GameHasControl: {MediaPlayer.GameHasControl}");
            if (MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Resume();
            else
                MediaPlayer.Pause();
        }
        
        
        _activeScene.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _activeScene.Draw(gameTime);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

