using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PurplePuffin;


public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _arialFont;
    private Vector2 _titlePos;
    private Vector2 _gameplayPos;
    
    private SceneType _sceneType;
    

    public PurplePuffinGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _sceneType = SceneType.Title;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _arialFont = Content.Load<SpriteFont>("arial");
        
        var viewport = GraphicsDevice.Viewport;
        _titlePos = new Vector2(viewport.Width / 2, viewport.Height / 2);
        _gameplayPos = new Vector2(viewport.Width / 2, viewport.Height / 2);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            _sceneType = SceneType.Game;
        }
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        
        if (_sceneType == SceneType.Title)
        {
            var message = "Title scene";
            var messageOrigin = _arialFont.MeasureString(message) / 2;
            _spriteBatch.DrawString(_arialFont, message, _titlePos, Color.LightGreen,
                0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
        else if (_sceneType == SceneType.Game)
        {
            // Draw game message
            var message = "Game scene";
            var messageOrigin = _arialFont.MeasureString(message) / 2;
            _spriteBatch.DrawString(_arialFont, message, _titlePos, Color.LightGreen,
                0, messageOrigin, 1.0f, SpriteEffects.None, 0.5f);
        }
        else
        {
            throw new Exception($"Unhandled Scene type: ${_sceneType}");
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

public enum SceneType
{
    Title,
    Game
}