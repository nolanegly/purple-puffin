using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PurplePuffin;

public class PurplePuffinGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _arialFont;
    private Vector2 _helloWorldPos;

    public PurplePuffinGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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
        _helloWorldPos = new Vector2(viewport.Width / 2, viewport.Height / 2);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        
        // Draw Hello World message
        var helloWorldMessage = "Hello World!";
        var helloWorldOrigin = _arialFont.MeasureString(helloWorldMessage) / 2;
        _spriteBatch.DrawString(_arialFont, helloWorldMessage, _helloWorldPos, Color.LightGreen,
            0, helloWorldOrigin, 1.0f, SpriteEffects.None, 0.5f);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}