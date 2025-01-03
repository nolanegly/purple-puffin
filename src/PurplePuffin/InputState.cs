using Microsoft.Xna.Framework.Input;

namespace PurplePuffin;

public class InputState
{
    private KeyboardState _prevKeyboardState;
    private KeyboardState _currKeyboardState;
    
    public void GetState()
    {
        _prevKeyboardState = _currKeyboardState;
        _currKeyboardState = Keyboard.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyTriggered(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key);
    }
}