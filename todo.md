# What's the simplest thing I can do
- [X] Draw hello world on the screen
- [X] Quit the game when a key is pressed
- [X] Add a main menu screen with Myra (UI library)
- [X] Add an options screen with a music volume control
  - will acquaint me with UI control library
- [X] Add a fade out transition effect between two scenes
- [ ] Add general scene transition support
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
- [ ] Investigate augmenting/replacing the SceneState with a .NET library for state machines
  - Instead of recording OldState and NewState on a "Transition" SceneType, it might make more sense
    to define each allowed transition between states, e.g. TitleToMainMenuTransition, MainMenuToOptionsMenuTransition,
    etc. But that might be a lot of transition nodes to add to the state chart, and harder to manage than a
    single reusable Transitioning state with Old and New properties.


# Polish
- [ ] Add a transition between title scene and game scene.
  - See C:\dev\MonoGame-Samples\NetRumble\ScreenManager\ScreenManager.cs for an example fade method
  - Might need to have concept of "transitioning" to ignore user input during the transition
  - Also see code comment about subtle issue changing active scene between Update() and Draw()
- [ ] Style the Myra scenes
  - Font/text size
- [ ] Automatically set the version on Main Menu from assembly's value
- [ ] Play sound effects when clicking menu items