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
        [RequireOwner()]
        public async Task VerifyAsync()
        {
            string captchaCode = CaptchaCodeFactory.GenerateCaptchaCode(6);
            Task save = SaveToSQL(captchaCode);

            var imageStream = ImageFactory.GenerateImage(captchaCode);
            imageStream.Position = 0;

            Image image = Image.FromStream(imageStream);
            image.Save($"{Context.User.Id}.png", ImageFormat.Png);

            await save;
            await Context.User.SendFileAsync($"{Context.User.Id}.png", $"Please type this captcha code to continue to {Context.Guild.Name}.");

            image.Dispose();
            File.Delete($"{Context.User.Id}.png");
        }

        public async Task SaveToSQL(string captcha)
        {
            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                string createView = "CREATE VIEW IF NOT EXISTS captchainsert AS SELECT Users.id, Users.user_id, Captcha.captcha FROM Users LEFT OUTER JOIN Captcha ON Users.id = Captcha.id;";
                string createTrigger = "CREATE TRIGGER IF NOT EXISTS insertcaptcha INSTEAD OF INSERT ON captchainsert\n" +
                    "BEGIN\n" +
                    "INSERT INTO Users(user_id) SELECT NEW.user_id WHERE NOT EXISTS(SELECT* FROM Users WHERE Users.user_id = NEW.user_id);\n" +
                    "UPDATE Captcha SET captcha = NEW.captcha WHERE id = (SELECT id FROM Users WHERE Users.user_id = NEW.user_id);\n" +
                    "INSERT INTO Captcha(captcha) SELECT NEW.captcha WHERE(Select Changes() = 0);\n" +
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
    }
}
