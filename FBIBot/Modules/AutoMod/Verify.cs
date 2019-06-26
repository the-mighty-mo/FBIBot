using CaptchaGen;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if (await IsVerifiedAsync())
            {
                await Context.User.SendMessageAsync("We already decided you *probably* aren't a communist spy. We suggest you don't try your luck again.");
                return;
            }

            List<string> captchas = await ReadSQLAsync();
            if (response.Length == 0 || captchas.Count == 0)
            {
                await SendCaptchaAsync();
                return;
            }

            if (response != captchas[0])
            {
                int attempts = await GetAttemptsAsync();
                attempts++;
                if (attempts >= 3)
                {
                    await RemoveCaptchaAsync();
                    await Context.User.SendMessageAsync("You have run out of attempts, you communist spy.\n" +
                        "If you would like to try again, please get a new captcha.");
                    return;
                }

                await UpdateAttemptsAsync(attempts);
                await Context.User.SendMessageAsync($"Incorrect. You have {3 - attempts} {(attempts == 1 ? "attempt" : "attempts")} remaining.");
                return;
            }

            await SaveVerificationAsync();
            await Context.User.SendMessageAsync("We have confirmed you are *probably* not a communist spy. You may proceed.");
        }

        async Task SendCaptchaAsync()
        {
            string captchaCode = CaptchaCodeFactory.GenerateCaptchaCode(6);
            Task save = SaveToSQLAsync(captchaCode);

            var imageStream = ImageFactory.GenerateImage(captchaCode);
            imageStream.Position = 0;

            Image image = Image.FromStream(imageStream);
            image.Save($"{Context.User.Id}.png", ImageFormat.Png);

            await save;
            await Context.User.SendFileAsync($"{Context.User.Id}.png", $"Please type `\\verify` followed by this captcha code to continue.\n");

            image.Dispose();
            File.Delete($"{Context.User.Id}.png");
        }

        async Task SaveToSQLAsync(string captcha)
        {
            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string createView = "CREATE VIEW IF NOT EXISTS captchainsert AS SELECT user_id, captcha FROM Captcha;";
                string createTrigger = "CREATE TRIGGER IF NOT EXISTS insertcaptcha INSTEAD OF INSERT ON captchainsert\n" +
                    "BEGIN\n" +
                    "UPDATE Captcha SET captcha = NEW.captcha WHERE user_id = NEW.user_id;\n" +
                    "INSERT INTO Captcha (user_id, captcha) SELECT NEW.user_id, NEW.captcha WHERE (Select Changes() = 0);\n" +
                    "END;";
                string insert = "INSERT INTO captchainsert (user_id, captcha) VALUES (@user_id, @captcha);";
                string drop = "DROP TRIGGER insertcaptcha; DROP VIEW captchainsert;";

                using (SqliteCommand cmd = new SqliteCommand(createView + createTrigger + insert + drop, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                    cmd.Parameters.AddWithValue("@captcha", captcha);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        async Task<List<string>> ReadSQLAsync()
        {
            List<string> captchas = new List<string>();

            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string read = "SELECT captcha FROM Captcha WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(read, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());

                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        captchas.Add((string)reader["captcha"]);
                    }
                    reader.Close();
                }
            }

            return await Task.Run(() => captchas);
        }

        async Task<int> GetAttemptsAsync()
        {
            int attempts = 0;

            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string read = "SELECT attempts FROM Attempts WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(read, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());

                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        attempts = int.Parse(reader["attempts"].ToString());
                    }
                    reader.Close();
                }
            }

            return await Task.Run(() => attempts);
        }

        async Task UpdateAttemptsAsync(int attempts)
        {
            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string createView = "CREATE VIEW IF NOT EXISTS attemptsupdate AS SELECT user_id, attempts FROM Attempts;";
                string createTrigger = "CREATE TRIGGER IF NOT EXISTS updateattempts INSTEAD OF INSERT ON attemptsupdate\n" +
                    "BEGIN\n" +
                    "UPDATE Attempts SET attempts = NEW.attempts WHERE user_id = NEW.user_id;\n" +
                    "INSERT INTO Attempts (user_id, attempts) SELECT NEW.user_id, NEW.attempts WHERE (Select Changes() = 0);\n" +
                    "END;";
                string insert = "INSERT INTO attemptsupdate (user_id, attempts) VALUES (@user_id, @attempts);";
                string drop = "DROP TRIGGER updateattempts; DROP VIEW attemptsupdate;";

                using (SqliteCommand cmd = new SqliteCommand(createView + createTrigger + insert + drop, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                    cmd.Parameters.AddWithValue("@attempts", attempts);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        async Task RemoveCaptchaAsync()
        {
            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string removeCaptcha = "DELETE FROM Captcha WHERE user_id = @user_id;";
                string removeAttempts = "DELETE FROM Attempts WHERE user_id = @user_id;";

                using (SqliteCommand cmd = new SqliteCommand(removeCaptcha + removeAttempts, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        async Task<bool> IsVerifiedAsync()
        {
            bool isVerified = false;

            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string verify = "SELECT * FROM Verified WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(verify, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());

                    SqliteDataReader reader = cmd.ExecuteReader();
                    isVerified = reader.Read();
                    reader.Close();
                }
            }

            return await Task.Run(() => isVerified);
        }

        async Task SaveVerificationAsync()
        {
            await RemoveCaptchaAsync();

            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string verify = "INSERT INTO Verified (user_id) SELECT @user_id WHERE NOT EXISTS (SELECT 1 FROM Verified WHERE user_id = @user_id);";
                using (SqliteCommand cmd = new SqliteCommand(verify, cn))
                {
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
