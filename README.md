# The FBI

[![CodeFactor](https://www.codefactor.io/repository/github/the-mighty-mo/fbibot/badge)](https://www.codefactor.io/repository/github/the-mighty-mo/fbibot)
[![Build Status](https://hallb1016.visualstudio.com/FBIBot/_apis/build/status/the-mighty-mo.FBIBot?branchName=master)](https://hallb1016.visualstudio.com/FBIBot/_build/latest?definitionId=5&branchName=master)

Discord Admin bot, created for Hack Week.

Built using .NET 5, Discord.Net v2.2.0, Microsoft.Data.Sqlite v5.0.2, and a modified version of CaptchaGen.NetCore v1.1.1.

**Someone has to keep the server in check, and who better than the FBI?**

This bot requires permissions: 424275126

## Build instructions

1. Download/clone the repository.
2. Open `FBIBot.sln` in Visual Studio on a Windows machine with .NET 5.  
**NOTE:** This project was created in Visual Studio 2019 and may not be backwards compatible with older versions of Visual Studio.
3. Add your Client ID, Token, and Discriminator to `SecurityInfo.cs` so the bot can come online and respond to commands.  
**NOTE:** It is recommended that you set your bot's profile picture to FBIBot/FBI.png and its name to "The FBI" for maximum effect.
4. Restore NuGet packages (if necessary before build).
5. Build the project in Release mode (Any CPU) and run `FBIBot.exe` in `FBIBot\bin\Release\netcoreapp3.1`.

### Some notes

Currently, warnings don't do anything automatically due to running out of time after setting up all the regex and automod stuff. Additionally, the regex stuff is not perfect, and not all messages send "useful" error messages. Lastly, commands with a given length of time will only work if the bot stays open since I have not figured out how to log data in such a way that the bot would be able to repeal mod actions after a restart. However, these will all be added/fixed as soon as possible after Discord selects the winners of Discord Hack Week.  
Also, we'll install more surveillance systems soon.

Thank you for your cooperation. ~~Because you don't have a choice but to cooperate.~~  
\- The FBI, under the direction of Supreme Leader Josedolf Staller
