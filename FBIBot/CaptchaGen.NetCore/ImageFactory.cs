using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

// Fixed Linux incompatibilities with this library (Format64bppArgb -> Format32bppArgb)
// Re-introduced lowercase letters into the CAPTCHA (if unique from capital variant)
// Fixed text drawing just outside the image by moving the starting corner away from the image corner
// Original at https://www.nuget.org/packages/CaptchaGen.NetCore/1.1.1?_src=template
namespace CaptchaGen.NetCore
{
    /// <summary>
    /// Generates a captcha image based on the captcha code string given.
    /// yangzhongke(http://www.youzack.com) migrated from https://github.com/vishnuprasadv/CaptchaGen/blob/master/CaptchaGen/ImageFactory.cs
    /// </summary>
    public static class ImageFactory
    {
        private const string FONTFAMILY = "Arial";

        /// <summary>
        /// Create a random string that consits of 'digitCount' characters
        /// </summary>
        /// <param name="digitCount"></param>
        /// <returns></returns>
        public static string CreateCode(int digitCount = 5)
        {
            char[] chars = {'A','B','C','D','G','H','K','M','N','P','Q','R','S','T','W','X','Y',
                'a', 'b', 'd', 'e', 'g', 'm', 'q', 'r', '3','4','5','6','8'}; // Edit 1: added lowercase letters
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < digitCount; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Background color to be used.
        /// Default value = Color.Wheat
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.Wheat;

        /// <summary>
        /// Actual image generator. Internally used.
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <param name="distortion">Distortion required</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream BuildImage(string captchaCode, int imageHeight, int imageWidth,
            int fontSize, int distortion = 18)
        {
            int newX, newY;
            Random random = new Random();
            MemoryStream memoryStream = new MemoryStream();
            using (Bitmap captchaImage = new Bitmap(imageWidth, imageHeight,
                PixelFormat.Format32bppArgb)) // Edit 2: Format64bppArgb -> Format32bppArgb
            using (Bitmap cache = new Bitmap(imageWidth, imageHeight,
                PixelFormat.Format32bppArgb)) // Edit 3: Format64bppArgb -> Format32bppArgb
            using (Graphics g = Graphics.FromImage(captchaImage))
            using (Font txtFont = new Font(FONTFAMILY, fontSize, FontStyle.Italic))
            {
                g.Clear(BackgroundColor);
                g.DrawString(captchaCode, txtFont, Brushes.Gray, new PointF(10f, 10f)); // Edit 4: Moved the starting point away from the corner

                //Draw interfering lines
                for (int i = 0; i < 8; i++)
                {
                    int startX = random.Next(imageWidth);
                    int startY = random.Next(imageHeight);
                    int endX = random.Next(imageWidth);
                    int endY = random.Next(imageHeight);

                    g.DrawLine(Pens.Gray, startX, startY, endX, endY);
                }

                //Distort the image with a wave function
                for (int y = 0; y < imageHeight; y++)
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        newX = (int)(x + (distortion * Math.Sin(Math.PI * y / 64.0)));
                        newY = (int)(y + (distortion * Math.Cos(Math.PI * x / 64.0)));
                        if (newX < 0 || newX >= imageWidth)
                            newX = 0;
                        if (newY < 0 || newY >= imageHeight)
                            newY = 0;
                        cache.SetPixel(x, y, captchaImage.GetPixel(newX, newY));
                    }
                }
                ImageFormat imgFormat = ImageFormat.Jpeg;
                cache.Save(memoryStream, imgFormat);
                memoryStream.Position = 0;
                return memoryStream;
            }
        }
    }
}