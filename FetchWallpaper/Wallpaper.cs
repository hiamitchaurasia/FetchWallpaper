using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace FetchWallpaper {

    /// <summary>
    /// This class represents a wallpaper
    /// </summary>
    [DataContract]
    public sealed class Wallpaper {

        // Contract for json parsing
        [DataMember(Name = "wallpaper_id")]
        public int id { get; set; }
        [DataMember(Name = "title")]
        public string title { get; set; }
        [DataMember(Name = "category_id")]
        public int catid { get; set; }
        [DataMember(Name = "width")]
        public int width { get; set; }
        [DataMember(Name = "height")]
        public int height { get; set; }
        [DataMember(Name = "date_added")]
        public long dateAdded { get; set; }
        [DataMember(Name = "rating")]
        public double rating { get; set; }
        [DataMember(Name = "image_url")]
        public string url { get; set; }

        // Constants
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        /// <summary>
        /// Enum defining styles
        /// </summary>
        public enum Style : int {
            Tiled, Centered, Stretched
        }

        /// <summary>
        /// Thanks Neil N! 
        /// http://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net
        /// </summary>
        /// <param name="style"></param>
        public void Set(Style style) {
            Stream s = new WebClient().OpenRead(this.url);
            Image img = Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched) {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered) {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled) {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            // Set wallpaper
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

    }


}
