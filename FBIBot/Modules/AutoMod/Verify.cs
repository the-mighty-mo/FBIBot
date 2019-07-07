﻿using CaptchaGen;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace FBIBot.Modules.AutoMod
{
    public class Verify : ModuleBase<SocketCommandContext>
    {
        [Command("verify")]
        public async Task VerifyAsync([Remainder] string response = "")
        {
            if (await GetVerifiedAsync(Context.User))
            {
                await GiveVerificationAsync();
                await Context.User.SendMessageAsync("We already decided you *probably* aren't a communist spy. We suggest you don't try your luck.");
                return;
            }

            List<string> captchas = await GetCaptchaAsync(Context.User);
            if (response.Length == 0 || captchas.Count == 0)
            {
                await SendCaptchaAsync();
                return;
            }

            if (response.ToLower() != captchas[0].ToLower())
            {
                int maxAttempts = 5;
                int attempts = await GetAttemptsAsync();
                attempts++;

                if (attempts >= maxAttempts)
                {
                    await RemoveCaptchaAsync(Context.User);
                    await Context.User.SendMessageAsync("You have run out of attempts, you communist spy.\n" +
                        "If you would like to try again, please get a new captcha by typing `\\verify`.");
                    return;
                }

                await SetAttemptsAsync(attempts);
                await Context.User.SendMessageAsync($"Incorrect. You have {maxAttempts - attempts} {(attempts == 1 ? "attempt" : "attempts")} remaining.");
                return;
            }

            await GiveVerificationAsync();
            await Context.User.SendMessageAsync("We have confirmed you are *probably* not a communist spy. You may proceed.");
            await SetVerifiedAsync(Context.User);

            List<Task> cmds = new List<Task>();
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                if (await Config.SetVerify.GetVerificationRoleAsync(g) == null)
                {
                    continue;
                }
                SocketGuildUser user = g.GetUser(Context.User.Id);
                cmds.Add(SendToModLog.SendToModLogAsync(SendToModLog.LogType.Verify, Context.Guild.CurrentUser, user));
            }
            await Task.WhenAll(cmds);
        }

        async Task SendCaptchaAsync() => await SendCaptchaAsync(Context.Guild.GetUser(Context.User.Id));

        public static async Task SendCaptchaAsync(SocketGuildUser u)
        {
            string captchaCode = "";
            List<string> badCaptcha = new List<string>() { "I", "l", "0", "O" };

            do
            {
                captchaCode = CaptchaCodeFactory.GenerateCaptchaCode(6);
            }
            while (captchaCode.Any(x => badCaptcha.Contains(x.ToString())));

            Task save = SetCaptchaAsync(captchaCode, u);

            var imageStream = ImageFactory.GenerateImage(captchaCode);
            imageStream.Position = 0;

            Image image = Image.FromStream(imageStream);
            image.Save($"{u.Id}.png", ImageFormat.Png);

            await save;
            await u.SendFileAsync($"{u.Id}.png", $"Please type `\\verify` followed by a space and this captcha code to continue{(u.Guild != null ? $" to {u.Guild.Name}" : "")}.\n");

            image.Dispose();
            File.Delete($"{u.Id}.png");
        }

        async Task GiveVerificationAsync()
        {
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                SocketRole role = await Config.SetVerify.GetVerificationRoleAsync(g);
                if (role != null && g.CurrentUser.GetPermissions(g.DefaultChannel).ManageRoles)
                {
                    await g.GetUser(Context.User.Id).AddRoleAsync(role);
                }
            }
        }

        public static async Task<List<string>> GetCaptchaAsync(SocketUser u)
        {
            List<string> captchas = new List<string>();

            string read = "SELECT captcha FROM Captcha WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(read, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    captchas.Add((string)reader["captcha"]);
                }
                reader.Close();
            }

            return await Task.Run(() => captchas);
        }

        public static async Task SetCaptchaAsync(string captcha, SocketUser u)
        {
            string update = "UPDATE Captcha SET captcha = @captcha WHERE user_id = @user_id;";
            string insert = "INSERT INTO Captcha (user_id, captcha) SELECT @user_id, @captcha WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                cmd.Parameters.AddWithValue("@captcha", captcha);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveCaptchaAsync(SocketUser u)
        {
            string removeCaptcha = "DELETE FROM Captcha WHERE user_id = @user_id;";
            string removeAttempts = "DELETE FROM Attempts WHERE user_id = @user_id;";

            using (SqliteCommand cmd = new SqliteCommand(removeCaptcha + removeAttempts, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        async Task<int> GetAttemptsAsync()
        {
            int attempts = 0;

            string read = "SELECT attempts FROM Attempts WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(read, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    attempts = int.Parse(reader["attempts"].ToString());
                }
                reader.Close();
            }

            return await Task.Run(() => attempts);
        }

        async Task SetAttemptsAsync(int attempts)
        {
            string update = "UPDATE Attempts SET attempts = @attempts WHERE user_id = @user_id;";
            string insert = "INSERT INTO Attempts (user_id, attempts) SELECT @user_id, @attempts WHERE (SELECT Changes() = 0);\n";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                cmd.Parameters.AddWithValue("@attempts", attempts);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> GetVerifiedAsync(SocketUser u)
        {
            bool isVerified = false;

            string verify = "SELECT user_id FROM Verified WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(verify, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                isVerified = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => isVerified);
        }

        public static async Task SetVerifiedAsync(SocketUser u)
        {
            await RemoveCaptchaAsync(u);

            string verify = "INSERT INTO Verified (user_id) SELECT @user_id WHERE NOT EXISTS (SELECT 1 FROM Verified WHERE user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(verify, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveVerifiedAsync(SocketUser u)
        {
            string delete = "DELETE FROM Verified WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
