using Discord;
using Discord.Interactions;
using FBIBot.ParamEnums;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    [Group("automod", "Configures AutoMod")]
    public class AutoMod : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("caps", "Takes down, with a warning, VERY LOUD PROTESTS")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiCapsAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiCaps.GetAntiCapsAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-caps is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} REALLY LOUD PROTESTS.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiCaps.SetAntiCapsAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiCaps.RemoveAntiCapsAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("invite", "Detects invites to the socialist party and *kindly* removes them")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiInviteAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiInvite.GetAntiInviteAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-invite is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} invitations to the socialist party.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiInvite.SetAntiInviteAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiInvite.RemoveAntiInviteAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("link", "Disposes of all messages containing links to communist propaganda websites")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiLinkAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiLink.GetAntiLinkAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-link is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages linking external, communist propaganda.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiLink.SetAntiLinkAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiLink.RemoveAntiLinkAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("mass-mention", "Takes down, with a warning, messages mentioning all the rich people the user wants to eat")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiMassMentionAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiMassMention.GetAntiMassMentionAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-mass-mention is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages mentioning the rich they want to eat.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiMassMention.SetAntiMassMentionAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiMassMention.RemoveAntiMassMentionAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("single-spam", "Detects if the user sends one big, spammy message and takes it down with a warning")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSingleSpamAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-single-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} *big*, spammy, anti-American messages.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiSingleSpam.SetAntiSingleSpamAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiSingleSpam.RemoveAntiSingleSpamAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("spam", "Detects if users send multiple identical messages and takes them down with a warning")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSpamAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiSpam.GetAntiSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} spammy, anti-American messages.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiSpam.SetAntiSpamAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiSpam.RemoveAntiSpamAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("zalgo", "Detects if a message was leaked from Area 51 and take it down with a warning")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiZalgoAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AntiZalgo.GetAntiZalgoAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that anti-zalgo is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages leaked from Area 51.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiZalgo.SetAntiZalgoAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiZalgo.RemoveAntiZalgoAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}