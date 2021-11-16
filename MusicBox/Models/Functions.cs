using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace MusicBox.Models
{
    public static class Functions
    {
        public static ImageSource BitmapToImageSource(Bitmap bitmap, bool disposable)
        {
            using var memory = new MemoryStream();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                //memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                if (disposable)
                    bitmap.Dispose();
                memory.Dispose();
                return bitmapImage;
        }
        public static string FindStringResource(string needKey)
        {
            try
            {
                string result = Application.Current.FindResource(needKey) as string;
                return result;
            }
            catch
            {
                return "";
            }
        }
        public static string Encrypt(string text, string key)
        {
            var rc6 = new RC6.RC6(256, Encoding.UTF8.GetBytes(key));
            var result = rc6.EncodeRc6(text);
            return Convert.ToBase64String(result);
        }
        public static string Decrypt(string cryptoText, string key)
        {
            var rc6 = new RC6.RC6(256, Encoding.UTF8.GetBytes(key));
            var result = rc6.DecodeRc6(Convert.FromBase64String(cryptoText));
            return Encoding.UTF8.GetString(result);
        }
        private static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        public static string GetHashString(string inputString)
        {

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static PointF FindCenter(PointF firstPoint, PointF secondPoint)
        {
            var x = (firstPoint.X + secondPoint.X) / 2;
            var y = (firstPoint.Y + secondPoint.Y) / 2;
            return new PointF(x,y);
        }
        public static string ParseCapabilityToString(VideoCapabilities capability)
        {
            return
                $"Resolution: {capability.FrameSize.Width}x{capability.FrameSize.Height} Frame rate: {capability.AverageFrameRate}";
        }
        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
