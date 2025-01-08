using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace PurplePuffin;

public class InputState
{
    private KeyboardState _prevKeyboardState;
    private KeyboardState _currKeyboardState;
    
    private GamePadState[] _prevGamePadStates = new GamePadState[GamePad.MaximumGamePadCount];
    private GamePadState[] _currGamePadStates = new GamePadState[GamePad.MaximumGamePadCount];

    public void GetState()
    {
        _prevKeyboardState = _currKeyboardState;
        _currKeyboardState = Keyboard.GetState();

        for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            _prevGamePadStates[i] = _currGamePadStates[i];
            _currGamePadStates[i] = GamePad.GetState(i);

            // TODO: need to send events when gamepads are disconnecting/connecting 
            if (_prevGamePadStates[i].IsConnected && !_currGamePadStates[i].IsConnected)
            {
                System.Diagnostics.Debug.WriteLine($"GamePad {i} became disconnected!");
            }
            else if (!_prevGamePadStates[i].IsConnected && _currGamePadStates[i].IsConnected)
            {
                System.Diagnostics.Debug.WriteLine($"GamePad {i} became connected!");
            }
        }
    }
    
    public bool IsKeyDown(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyTriggered(Keys key)
    {
        return _currKeyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyUp(key);
    }

    public bool IsKeyReleased(Keys key)
    {
        return _currKeyboardState.IsKeyUp(key) && _prevKeyboardState.IsKeyDown(key);
    }

    public int[] CurrentlyConnectedGamePads()
    {
        var connectedPads = new List<int>(GamePad.MaximumGamePadCount);
        for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            if (_currGamePadStates[i].IsConnected)
                connectedPads.Add(i);
        }
        return connectedPads.ToArray();
    }

    // TODO: remove once analog input is available via methods on this class
    public GamePadState CurrGamePadState(int controllerIndex)
    {
        return _currGamePadStates[controllerIndex];
    }
    
    // TODO: remove once analog input is available via methods on this class
    public GamePadState PrevGamePadState(int controllerIndex)
    {
        return _prevGamePadStates[controllerIndex];
    }
    
    public bool IsGamepadButtonDown(int controllerIndex, Buttons button)
    {
        return _currGamePadStates[controllerIndex].IsConnected 
               && _currGamePadStates[controllerIndex].IsButtonDown(button);
    }
    
    public bool IsGamepadButtonTriggered(int controllerIndex, Buttons button)
    {
        return _currGamePadStates[controllerIndex].IsConnected 
               && _currGamePadStates[controllerIndex].IsButtonDown(button)
               && _prevGamePadStates[controllerIndex].IsButtonUp(button);
    }

    public bool IsGamepadButtonReleased(int controllerIndex, Buttons button)
    {
        return _currGamePadStates[controllerIndex].IsConnected 
               && _currGamePadStates[controllerIndex].IsButtonUp(button)
               && _prevGamePadStates[controllerIndex].IsButtonDown(button);
    }
}