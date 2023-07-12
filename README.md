# The FBI

[![CodeFactor](https://www.codefactor.io/repository/github/the-mighty-mo/fbibot/badge)](https://www.codefactor.io/repository/github/the-mighty-mo/fbibot)
[![.NET Build](https://github.com/the-mighty-mo/FBIBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/the-mighty-mo/FBIBot/actions/workflows/dotnet.yml)

Discord Admin bot, created for Hack Week.

Built using .NET 7, Discord.Net, Microsoft.Data.Sqlite, and a modified version of CaptchaGen.NetCore v1.1.1.

**Someone has to keep the server in check, and who better than the FBI?**

This bot requires permissions: 424275126

## Build instructions

1. Download/clone the repository.
2. Open `FBIBot.sln` in Visual Studio on a Windows machine with .NET 6.  
**NOTE:** This project was created in Visual Studio 2022 and may not be backwards compatible with older versions of Visual Studio.
3. Add your Client ID, Token, and Discriminator to `SecurityInfo.cs` so the bot can come online and respond to commands.  
**NOTE:** It is recommended that you set your bot's profile picture to FBIBot/FBI.png and its name to "The FBI" for maximum effect.
4. Build the project in Release mode (Any CPU) and run `FBIBot.exe` in `FBIBot\bin\Release\net6.0`.

### Some notes

Currently, warnings don't do anything automatically. Additionally, not all messages send "useful" error messages. Lastly, commands with a given length of time will only work if the bot keeps running (this will be fixed in a future release).  
Also, we'll install more surveillance systems soon.

Thank you for your cooperation. ~~Not that you had a choice.~~  
\- The FBI, under the direction of Supreme Leader Josedolf Staller
