using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

using NDesk.Options;
using System.IO;

namespace FetchWallpaper {

    /// <summary>
    /// Main class to start this application
    /// </summary>
    class Program {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // The mutex for this program
        private static Mutex mutex;

        // Constants for this program
        private const string API = "http://www.wallpaperup.com/api/";
        private const string API_KEY = "APIKEY";

        private const string API_CALL_RANDOM_FEATURED = API + "wallpapers/type/random_featured/number/1/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_CATEGORY_PAGES = API + "total_pages_nr/type/category/number/1/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_CATEGORY_WALLPAPER = API + "wallpapers/type/category/number/1/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_MEMBER_FAVOURITE = API + "wallpapers/type/fav_cat/number/1/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_MEMBER_FAVOURITE_PAGES = API + "total_pages_nr/type/fav_cat/number/1/apikey/" + API_KEY + "/format/json/";

        private const string CATEGORY_ID = "category_id";
        private const string MEMBER_ID = "member_id";
        private const string FOLDER_ID = "fav_cat_id";
        private const string PAGE = "page";

        // Prepare a system tray icon
        private NotifyIcon tray = new NotifyIcon();
        private Thread notifyThread;        

        // Prepare objects for our loop and program
        private readonly object sharedMonitor = new object();
        private int lastPage;
        private bool running;

        // Delegate 
        private delegate bool ExecuteMode();
        
        // Main method where this program starts
        static void Main(string[] args) {
            bool help = false;

            // The current parameter
            int param = 0;

            // Prepare options
            OptionSet options = new OptionSet() {
                {"d|delay=", "The amount of time in minutes the program waits to fetch a new wallpaper", (int v) => { Config.Delay = v; param = 1;} },
                {"u|user=", "The id of the user you want the favourites from. Only usable in MEMBER mode.", (int v) => { Config.MemberID = v; param = 2; } },
                {"f|folder=", "The id of the user folder. Only usable in MEMBER mode.", (int v) => { Config.FolderID = v; param = 3; } },
                {"c", "The id's of the categories you want in RANDOM mode.", v => param = 10 },
                {"h|help", "Show this message and exit", v => { help = true; param = 20; } },
                {"<>", v => {
                        // Check if we are adding categories
                        int category = 0;
                        if (param == 10 && int.TryParse(v, out category)) {
                            Config.Categories.Add(category);
                        } 
                        
                        // Check if the user wants a member wallpaper
                        else if (v.Equals("member")) {
                            Config.Mode = 2;
                        }

                        // Check if the user wants a random wallpaper
                        else if (v.Equals("random")) {
                            Config.Mode = 1;
                        } 
                        
                        // Invalid arguments
                        else {
                            help = true;
                        }
                    }
                }
            };

            // Parse the arguments
            List<string> extra;
            try {
                extra = options.Parse(args);
            } catch (OptionException) {
                Logger.error("Failed to parse the given arguments");

                // Show help
                Console.WriteLine();
                showHelp(options);
                return;
            }

            // Check if the user wants the help file
            if (help || Config.Mode > 2 || (Config.Mode == 2 && Config.MemberID == 0)) {
                showHelp(options);
                return;
            }

            // Check if we have already an instance of this program
            if (isAlreadyRunning()) {
                Logger.error("This application is already running. Exiting here.");
                return;
            }

            // Start running the program
            new Program().startRunning();
        }

        /// <summary>
        /// Check if this program is already running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        private static bool isAlreadyRunning() {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;

            bool bCreatedNew;
            mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);

            if (bCreatedNew)
                mutex.ReleaseMutex();

            return !bCreatedNew;
        }

        /// <summary>
        /// This will show the help file
        /// </summary>
        private static void showHelp(OptionSet options) {
            Console.WriteLine("Usage: FetchWallpaper [OPTIONS] <random|member>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// This will prepare the system tray icon
        /// </summary>
        private void prepareTrayIcon() {
            // Create a thread where the notification tray can catch events
            notifyThread = new Thread(
                delegate() {
                    tray.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("FetchWallpaper.icon.ico"));
                    tray.ContextMenuStrip = new ContextMenuStrip();
                    tray.ContextMenuStrip.Items.Add("Refresh", null, new EventHandler(refreshWallpaper));
                    tray.ContextMenuStrip.Items.Add("Quit", null, new EventHandler(exitApplication));
                    tray.Text = "Fetch wallpaper application";
                    tray.Visible = true;

                    Application.Run();
                }
            );

            notifyThread.Start();
        }

        /// <summary>
        /// This method will prepare the application to search for a new wallpaper
        /// </summary>
        private void refreshWallpaper(object sender, EventArgs e) {
            Logger.info("User request to refresh the wallpaper");

            // Show message to user
            tray.ShowBalloonTip(2000, null, "Downloading a new wallpaper ..", new ToolTipIcon());

            // Force the loop to continue
            lock (sharedMonitor) {
                Monitor.Pulse(sharedMonitor);
            }
        }

        /// <summary>
        /// This method will prepare the application to shutdown
        /// </summary>
        private void exitApplication(object sender, EventArgs e) {
            tray.Visible = false;
            tray.Dispose();

            // Stop listening for events
            Application.Exit();

            // Force the loop to quit
            lock (sharedMonitor) {
                running = false;
                Monitor.Pulse(sharedMonitor);
            }            
        }

        /// <summary>
        /// This method will launch the program in the correct mode
        /// </summary>
        public void startRunning() {
            ExecuteMode executeModeFunction = null;
            
            // Check in what mode we are in
            switch (Config.Mode) {
                case 2: executeModeFunction = fetchMemberWallpaper; break;
                default: executeModeFunction = fetchRandomWallpaperFromCategory; break;
            }

            // Hide the console window
            ShowWindow(GetConsoleWindow(), 0);
            
            // Create a tray icon
            prepareTrayIcon();

            // Prepare
            running = true;
            bool succes = false;

            // Main loop for waiting and executing
            while (running) {
                try {
                    // Start the engines
                    succes = executeModeFunction();
                } catch (Exception e) {
                    succes = false;
                    Logger.severe(e.StackTrace);
                }

                lock (sharedMonitor) {
                    // Check if we had errors and break this
                    if (succes) {
                        Logger.info("Waiting " + Config.Delay + " minutes to fetch a new wallpaper");
                        Monitor.Wait(sharedMonitor, Config.Delay * 60 * 1000);
                    } else {
                        Logger.info("Something went wrong, trying again in 1 minute");
                        Monitor.Wait(sharedMonitor, 60000);
                    }
                }
            }
        }

        /// <summary>
        /// This method will fetch a random wallpaper
        /// </summary>
        private bool fetchRandomWallpaperFromCategory() {
            Logger.info("Entering random mode");

            // Check if we have categories specified, else request a random featured one
            if (Config.Categories.Count == 0) {
                // Fetch a random featured wallpaper
                WallpaperResponse wallpaperResponse = MakeRequest<WallpaperResponse>(API_CALL_RANDOM_FEATURED);

                if (wallpaperResponse == null || (wallpaperResponse != null && wallpaperResponse.wallpapers.Length == 0)) {
                    Logger.error("No wallpapers found or there something wrong with the API.");
                    return false;
                }

                // Set the found wallpaper
                Wallpaper wallpaper = wallpaperResponse.wallpapers[0];
                wallpaper.Set(Wallpaper.Style.Stretched);

                return true;
            }

            Random random = new Random();

            // Choose a random category
            int category = Config.Categories[random.Next(Config.Categories.Count)];
            Logger.info("Using category " + category + " to fetch a wallpaper from");

            // Fetch the total number of wallpapers in this category
            string categoryPagesUrl = API_CALL_CATEGORY_PAGES + CATEGORY_ID + "/" + category;
            TotalPagesResponse pagesResponse = MakeRequest<TotalPagesResponse>(categoryPagesUrl);

            if (pagesResponse == null || (pagesResponse != null && pagesResponse.totalPages == 0)) {
                Logger.error("No wallpapers found in this category or there something wrong with the API.");
                return false;
            }

            // Fetch a random wallpaper from this category
            int wpid = random.Next(1, pagesResponse.totalPages + 1);
            while (wpid == lastPage && pagesResponse.totalPages > 1)
                wpid = random.Next(1, pagesResponse.totalPages + 1);

            string categoryWallpaperUrl = API_CALL_CATEGORY_WALLPAPER + CATEGORY_ID + "/" + category + "/" + PAGE + "/" + wpid;
            WallpaperResponse wallpaperCategoryResponse = MakeRequest<WallpaperResponse>(categoryWallpaperUrl);

            if (wallpaperCategoryResponse == null || (wallpaperCategoryResponse != null && wallpaperCategoryResponse.wallpapers.Length == 0)) {
                Logger.error("No wallpapers found or there something wrong with the API.");
                return false;
            }

            // Set the found wallpaper
            Wallpaper wallpaperCategory = wallpaperCategoryResponse.wallpapers[0];
            wallpaperCategory.Set(Wallpaper.Style.Stretched);

            lastPage = wpid;

            return true;
        }

        /// <summary>
        /// This method will fetch a wallpaper from the given user his favourites
        /// </summary>
        private bool fetchMemberWallpaper() {
            Logger.info("Entering member mode");

            string url = API_CALL_MEMBER_FAVOURITE;
            string urlPages = API_CALL_MEMBER_FAVOURITE_PAGES;

            // Add member id
            Logger.info("Using member id " + Config.MemberID);
            url = url + MEMBER_ID + "/" + Config.MemberID + "/";
            urlPages = urlPages + MEMBER_ID + "/" + Config.MemberID + "/";
            
            // Check if a folder id was specified
            if (Config.FolderID > 0) {
                Logger.info("Using folder id " + Config.FolderID);
                url = url + FOLDER_ID + "/" + Config.FolderID + "/";
                urlPages = urlPages + FOLDER_ID + "/" + Config.FolderID + "/";
            }

            Random random = new Random();

            // Fetch the total wallpapers in the user his favourites
            TotalPagesResponse pagesResponse = MakeRequest<TotalPagesResponse>(urlPages);

            if (pagesResponse == null || (pagesResponse != null && pagesResponse.totalPages == 0)) {
                Logger.error("No wallpapers found in this user his favourites or there something wrong with the API.");
                return false;
            }

            // Fetch a random wallpaper from the user his favourites
            int wpid = random.Next(1, pagesResponse.totalPages + 1);
            while (wpid == lastPage && pagesResponse.totalPages > 1)
                wpid = random.Next(1, pagesResponse.totalPages + 1);

            url = url + PAGE + "/" + wpid;
            WallpaperResponse wallpaperResponse = MakeRequest<WallpaperResponse>(url);

            if (wallpaperResponse == null || (wallpaperResponse != null && wallpaperResponse.wallpapers.Length == 0)) {
                Logger.error("No wallpapers found or there something wrong with the API.");
                return false;
            }

            // Set the found wallpaper
            Wallpaper wallpaperMember = wallpaperResponse.wallpapers[0];
            wallpaperMember.Set(Wallpaper.Style.Stretched);

            lastPage = wpid;

            return true;
        }

        /// <summary>
        /// This function will query the given url and parses it to a deserialized object
        /// </summary>
        /// <param name="requestUrl">The url you want to query</param>
        /// <returns>The deserialized object</returns>
        private T MakeRequest<T>(string requestUrl) {
            try {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(
                            String.Format("Server error (HTTP {0}: {1}).",
                            response.StatusCode, response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                    return (T) jsonSerializer.ReadObject(response.GetResponseStream());
                }
            } catch (Exception e) {
                Logger.error(e.Message);
            }

            // Request failed, return null;
            return default(T);
        }
    }

}
