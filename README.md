# The FBI

[![CodeFactor](https://www.codefactor.io/repository/github/josedolf-staller/fbibot/badge)](https://www.codefactor.io/repository/github/josedolf-staller/fbibot)
[![Build Status](https://hallb1016.visualstudio.com/FBIBot/_apis/build/status/josedolf-staller.FBIBot?branchName=master)](https://hallb1016.visualstudio.com/FBIBot/_build/latest?definitionId=2&branchName=master)

Discord Admin bot, created for Hack Week.

**Someone has to keep the server in check, and who better than the FBI?**

## Build instructions

1. Download/clone the repository
2. Open `FBIBot.sln` in Visual Studio on a Windows machine with .NET Framework 4.7.2.  
**NOTE:** This project was created in Visual Studio 2019, and it may not be backwards compatible with older versions of Visual Studio.
3. Add your Client ID, Token, and Discriminator to `SecurityInfo.cs` so the bot can come online and respond to commands.
4. Restore NuGet packages (if necessary before build).
5. Build the project in Release mode (Any CPU) and run `FBIBot.exe` (in FBIBot\bin\Release).
