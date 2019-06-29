# The FBI

[![CodeFactor](https://www.codefactor.io/repository/github/josedolf-staller/fbibot/badge)](https://www.codefactor.io/repository/github/josedolf-staller/fbibot)
[![Build Status](https://hallb1016.visualstudio.com/FBIBot/_apis/build/status/josedolf-staller.FBIBot?branchName=master)](https://hallb1016.visualstudio.com/FBIBot/_build/latest?definitionId=2&branchName=master)

Discord Admin bot, created for Hack Week.

**Someone has to keep the server in check, and who better than the FBI?**

*Email all bug reports to hallb1016@gmail.com*

This bot requires permissions: 289533110

## Build instructions

1. Download/clone the repository.
2. Open `FBIBot.sln` in Visual Studio on a Windows machine with .NET Framework 4.7.2.  
**NOTE:** This project was created in Visual Studio 2019, and it may not be backwards compatible with older versions of Visual Studio.
3. Add your Client ID, Token, and Discriminator to `SecurityInfo.cs` so the bot can come online and respond to commands.  
**NOTE:** It is recommended that you set your bot's profile picture to FBIBot/FBI.png and its name to "The FBI" for maximum effect.
4. Restore NuGet packages (if necessary before build).
5. Build the project in Release mode (Any CPU) and run `FBIBot.exe` (in FBIBot\bin\Release).

### Some notes

Currently, warnings don't do anything automatically due to running out of time after setting up all the regex and automod stuff. Additionally, the regex stuff is not perfect, and messages send generic exceptions instead of good error messages in some cases. However, these will all be added as soon as possible after Discord selects the winners of Discord Hack Week.

Thank you for your cooperation. ~~Because you don't have a choice but to cooperate.~~  
\- The FBI, under the direction of Supreme Leader Josedolf Staller
