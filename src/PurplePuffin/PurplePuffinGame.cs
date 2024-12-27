﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Myra;

namespace PurplePuffin;

public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SharedContent _sharedContent;
    
    private Scene _activeScene;
    
    private TitleScene _titleScene;
    private MainMenuScene _mainMenuScene;
    private GameScene _gameScene;

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
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _titleScene = new TitleScene(GraphicsDevice, _spriteBatch);
        _mainMenuScene = new MainMenuScene(GraphicsDevice);
        _gameScene = new GameScene(GraphicsDevice, _spriteBatch);
        
        _activeScene = _titleScene;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sharedContent.ArialFont = Content.Load<SpriteFont>("arial");
        
        _titleScene.LoadContent(_sharedContent);
        _mainMenuScene.LoadContent();
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
        var currSceneType = _activeScene.SceneType;
        if (currSceneType == SceneType.Title)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                _activeScene = _mainMenuScene;
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
        
        
        var events = _activeScene.Update(gameTime);

        foreach (var e in events)
        {
            // TODO: the code detecting ESC input at the top of the method should probably generate this event
            // rather than handle directly
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
                _activeScene = _gameScene;
        }
        
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

