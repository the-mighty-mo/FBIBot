﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public class RequireMod : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services) =>
            Context.User is SocketGuildUser user
                ? await VerifyUser.IsMod(user)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("You are not an assistant of the FBI and cannot use this command.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}