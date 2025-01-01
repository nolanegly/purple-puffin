# Goal
Develop my first polished video game that can be played on Windows and Mac OS.

Polished refers to the level of feature completeness, such as having background music, sound effects, menu screens - a "complete" game that can be picked up and easily played by casual players. 

Polished does NOT refer to the level of sophistication in game mechanics, degree of detail in graphics and music, etc.

For simplicity I am solely targeting desktop environments; web, mobile, and console versions are a non-goal.

# Architecture Decision Records

## Choose runtime
I am using the MonoGame framework. Reasoning include:
- I'm already very experienced with C#.
- I had some minor exposure to XNA when it was first released, and am already somewhat familiar with the API.

I am using a low-level framework instead of the more featured engine Godot because:
- I think learning "the guts" of games will be fun and educational.
- I am willing to give up faster development speed and "time to market" for more control and learning.

## Choose development tools
Resharper
- I want to learn a new IDE, and am already familiar with Visual Studio.
- I have a license via my JetBrains subscription, I don't have an active personal license for VS.

Git
- The default VCS everyone should use, unless there's a good overriding reason it isn't an option.
