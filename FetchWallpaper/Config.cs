using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FetchWallpaper {

    /// <summary>
    /// This class keeps the configuration of this program
    /// </summary>
    public class Config {

        private static int delay = 60;

        /// <summary>
        /// The time the program waits to fetch a new wallpaper
        /// </summary>
        public static int Delay {
            get { return delay; }
            set { delay = value; }
        }

        private static int mode = 0;

        /// <summary>
        /// The mode we are currently running
        ///     1 means we are fetching a random wallpaper from the api
        ///     2 means we are fetching a random wallpaper from the user his favorites
        /// </summary>
        public static int Mode {
            get { return mode; }
            set { mode = mode + value; }
        }

        private static int memberId = 0;

        /// <summary>
        /// This will set the user we want to use for mode 2
        /// This is required
        /// </summary>
        public static int MemberID {
            get { return memberId; }
            set { memberId = value; }
        }

        private static int folderId = 0;

        /// <summary>
        /// This will set the folder id for mode 2
        /// This is optional
        /// </summary>
        public static int FolderID {
            get { return folderId; }
            set { folderId = value; }
        }

        private static List<int> categories = new List<int>();

        /// <summary>
        /// This keeps all the categories the user wants for random mode
        /// </summary>
        public static List<int> Categories {
            get { return categories;  }
        }
    }

}
