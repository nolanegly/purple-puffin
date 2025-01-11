using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PurplePuffin.Events;

namespace PurplePuffin;

public class InputState
{
    private KeyboardState _prevKeyboardState;
    private KeyboardState _currKeyboardState;
    
    private readonly GamePadState[] _prevGamePadStates = new GamePadState[GamePad.MaximumGamePadCount];
    private readonly GamePadState[] _currGamePadStates = new GamePadState[GamePad.MaximumGamePadCount];
    
    private readonly List<EventBase> _eventsToReturn = new();

    public EventBase[] GetState()
    {
        _prevKeyboardState = _currKeyboardState;
        _currKeyboardState = Keyboard.GetState();

        for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            _prevGamePadStates[i] = _currGamePadStates[i];
            _currGamePadStates[i] = GamePad.GetState(i);

            if (_prevGamePadStates[i].IsConnected && !_currGamePadStates[i].IsConnected)
            {
                _eventsToReturn.Add(new GamepadDisconnectedEvent(i));
            }
            else if (!_prevGamePadStates[i].IsConnected && _currGamePadStates[i].IsConnected)
            {
                _eventsToReturn.Add(new GamepadConnectedEvent(i));
            }
        }
        
        var result = _eventsToReturn.ToArray();
        _eventsToReturn.Clear();
        return result;
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

    public float GamepadLeftTriggerDegree(int controllerIndex)
    {
        return !_currGamePadStates[controllerIndex].IsConnected 
            ? 0.0f 
            : _currGamePadStates[controllerIndex].Triggers.Left;
    }
    
    public float GamepadRightTriggerDegree(int controllerIndex)
    {
        return !_currGamePadStates[controllerIndex].IsConnected 
            ? 0.0f 
            : _currGamePadStates[controllerIndex].Triggers.Right;
    }
    
    public Vector2 GamepadLeftThumbstickDegree(int controllerIndex)
    {
        return !_currGamePadStates[controllerIndex].IsConnected 
            ? Vector2.Zero 
            : _currGamePadStates[controllerIndex].ThumbSticks.Left;
    }
    
    public Vector2 GamepadRightThumbstickDegree(int controllerIndex)
    {
        return !_currGamePadStates[controllerIndex].IsConnected 
            ? Vector2.Zero 
            : _currGamePadStates[controllerIndex].ThumbSticks.Right;
    }
}