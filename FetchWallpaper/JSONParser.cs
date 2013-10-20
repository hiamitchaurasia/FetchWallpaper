using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace FetchWallpaper {

    /// <summary>
    /// This class represents a response from the API
    /// </summary>
    [DataContract]
    public class WallpaperResponse {
        [DataMember(Name = "wallpapers")]
        public Wallpaper[] wallpapers { get; set; }
    }

    /// <summary>
    /// This class represents a response from the API
    /// </summary>
    [DataContract]
    public class TotalPagesResponse {
        [DataMember(Name = "total_pages")]
        public int totalPages { get; set; }
    }

}
