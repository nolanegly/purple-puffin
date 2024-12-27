# What's the simplest thing I can do
- [X] Draw hello world on the screen
- [X] Quit the game when a key is pressed
- [X] Add a main menu screen with Myra (UI library)
- [ ] Add an options screen with a music volume control
  - will acquaint me with UI control library
- [ ] Add a credits screen that cites music source
  - Juhani Junkala, https://juhanijunkala.com/
  - music tracks have been released under CC0 creative commons license
- [ ] Build and run on Mac OS!
  - Start doing this iteratively, to detect compatibility issues as early as possible

# Later
- [ ] Draw a sprite
- [ ] Move a sprite in response to player input
- [ ] Deal with resolution changes
- [ ] Deal with window size changes
- [ ] Deal with gamepad disconnecting during play


# Polish
- [ ] Add a transition between title scene and game scene.
  - See C:\dev\MonoGame-Samples\NetRumble\ScreenManager\ScreenManager.cs for an example fade method
  - Might need to have concept of "transitioning" to ignore user input during the transition
  - Also see code comment about subtle issue changing active scene between Update() and Draw()
- [ ] Style the Myra main menu scene
  - Font/text size
- [ ] Automatically set the version on Main Menu from assembly's value