using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Runtime.InteropServices;

using NDesk.Options;

namespace FetchWallpaper {

    /// <summary>
    /// Main class to start this application
    /// </summary>
    class Program {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Constants for this program
        private const string API = "http://www.wallpaperup.com/api/";
        private const string API_KEY = "APIKEY";

        private const string API_CALL_RANDOM_FEATURED = API + "wallpapers/type/random_featured/number/100/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_MEMBER_FAVOURITE = API + "wallpapers/type/fav_cat/apikey/" + API_KEY + "/format/json/";
        private const string API_CALL_MEMBER_FAVOURITE_PAGES = API + "total_pages_nr/type/fav_cat/apikey/" + API_KEY + "/format/json/";

        // Delegate 
        private delegate bool executeMode();
        
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

                // Quit here
                return;
            }

            // Check if the user wants the help file
            if (help) {
                showHelp(options);
                // Quit here
                return;
            }

            // Check the mode
            if (Config.Mode > 2) {
                Logger.error("Wrong mode, you can only select 1 mode");

                // Show help
                Console.WriteLine();
                showHelp(options);
                // Quit here
                return;
            } else if (Config.Mode == 2) {
                // Check if the user id is given
                if (Config.MemberID == 0) {
                    Logger.error("You need to specify the user id in MEMBER MODE");

                    // Show help
                    Console.WriteLine();
                    showHelp(options);
                    // Quit here
                    return;
                }

                Logger.info("Entering member mode");
                startRunning(new Program().fetchMemberWallpaper);
            } else {
                Logger.info("Entering random mode");
                startRunning(new Program().fetchRandomWallpaperFromCategory);
            }
            
            Console.ReadLine();
        }

        /// <summary>
        /// This will show the help file
        /// </summary>
        static void showHelp(OptionSet options) {
            Console.WriteLine("Usage: FetchWallpaper [OPTIONS] <random|member>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// 
        /// </summary>
        static void startRunning(executeMode mode) {
            // Hide the console window
            ShowWindow(GetConsoleWindow(), 0);

            // Prepare
            bool run = true;
            bool succes = false;

            // Run
            while (run) {
                try {
                    // Start the engines
                    succes = mode();
                } catch (Exception e) {
                    Logger.severe(e.Message);
                }

                // Check if we had errors and break this
                if (succes) {
                    Logger.info("Waiting " + Config.Delay + " minutes to fetch a new wallpaper");
                    Thread.Sleep(Config.Delay * 60 * 1000);
                } else {
                    Logger.info("Something went wrong, trying again in 1 minute");
                    Thread.Sleep(60000);
                }
            }
        }

        /// <summary>
        /// This method will fetch a random wallpaper
        /// </summary>
        public bool fetchRandomWallpaperFromCategory() {
            // CALL API method and check if the response is valid
            WallpaperResponse wallpapersResponse = MakeRequest<WallpaperResponse>(API_CALL_RANDOM_FEATURED) as WallpaperResponse;
            if (wallpapersResponse == null)
                return false;

            if (wallpapersResponse.wallpapers.Length == 0) {
                Logger.error("No wallpapers found. Something wrong with the API?");
                return false;
            }

            // Check if we have valid categories, else just set the first wallpaper in response
            if (Config.Categories.Count == 0) {
                Logger.info("No categories found, using the first wallpaper.");

                Wallpaper first = wallpapersResponse.wallpapers[0];

                try {
                    // Set the wallpaper
                    first.Set(Wallpaper.Style.Stretched);

                    Logger.info("Using wallpaper " + first.title);
                } catch (Exception e) {
                    Logger.error(e.Message);
                }

                // We are done
                return true;
            }

            // Create a hashtable with wallpapers per category
            Logger.info("Creating hash table with all possible wallpapers.");
            Dictionary<int, List<Wallpaper>> wallpapers = new Dictionary<int, List<Wallpaper>>();

            for (int i = 0; i < wallpapersResponse.wallpapers.Length; i++) {
                // Get the wallpaper
                Wallpaper temp = wallpapersResponse.wallpapers[i];
                // Check if the category is one of the specified categories
                if (Config.Categories.Contains(temp.catid)) {
                    // Check if the category exists
                    if (wallpapers.ContainsKey(temp.catid)) {
                        // Add to existing list
                        wallpapers[temp.catid].Add(temp);
                    } else {
                        // Create new list
                        List<Wallpaper> l = new List<Wallpaper>();
                        l.Add(temp);
                        // Add to the hash
                        wallpapers.Add(temp.catid, l);
                    }
                }
            }

            // This wallpaper will be used
            Wallpaper pickedWallpaper = wallpapersResponse.wallpapers[0];
            if (wallpapers.Count > 0) {
                // Choose a random category and value if the hash contains wallpapers
                Random rand = new Random();
                List<Wallpaper> pickedCategoryList = wallpapers.Values.ElementAt(rand.Next(wallpapers.Keys.Count));
                pickedWallpaper = pickedCategoryList[rand.Next(pickedCategoryList.Count)];
            }

            try {
                // Set the wallpaper
                pickedWallpaper.Set(Wallpaper.Style.Stretched);
                Logger.info("Using wallpaper " + pickedWallpaper.title + " " + pickedWallpaper.url);
            } catch (Exception e) {
                Logger.error(e.Message);
            }

            // We are done
            return true;
        }

        /// <summary>
        /// This method will fetch a wallpaper from the given user his favourites
        /// </summary>
        public bool fetchMemberWallpaper() {
            string url = API_CALL_MEMBER_FAVOURITE;
            string urlPages = API_CALL_MEMBER_FAVOURITE_PAGES;

            // Add member id
            Logger.info("Using member id " + Config.MemberID);
            url = url + "member_id/" + Config.MemberID + "/";
            urlPages = urlPages + "member_id/" + Config.MemberID + "/";
            
            // Check if a folder id was specified
            if (Config.FolderID > 0) {
                Logger.info("Using folder id " + Config.FolderID);
                url = url + "fav_cat_id/" + Config.FolderID + "/";
                urlPages = urlPages + "fav_cat_id/" + Config.FolderID + "/";
            }

            // Fetch the amount of pages
            TotalPagesResponse pages = MakeRequest<TotalPagesResponse>(urlPages) as TotalPagesResponse;
            if (pages == null)
                return false;

            Logger.info("User has " + pages.totalPages + " page(s) of wallpapers");

            // Fetch all wallpapers
            List<Wallpaper> wallpapers = new List<Wallpaper>();
            for (int i = 1; i <= pages.totalPages; i++) {
                string temp = url + "page/" + i + "/";
                
                // Fetch the page of wallpapers
                WallpaperResponse response = MakeRequest<WallpaperResponse>(temp) as WallpaperResponse;
                if (response != null) {
                    wallpapers.AddRange(response.wallpapers.ToList());
                }
            }

            // Check if we have found some wallpapers
            if (wallpapers.Count == 0) {
                Logger.warn("No wallpapers found. Try another user or folder.");
                return false;
            }
            
            // Choose a random wallpaper
            Random rand = new Random();

            try {
                Wallpaper pickedWallpaper = wallpapers[rand.Next(wallpapers.Count)];
                // Set the wallpaper
                pickedWallpaper.Set(Wallpaper.Style.Stretched);
                Logger.info("Using wallpaper " + pickedWallpaper.title + " " + pickedWallpaper.url);
            } catch (Exception e) {
                Logger.error(e.Message);
            }

            // We are done
            return true;
        }

        /// <summary>
        /// This function will query the given url and parses it to a deserialized object
        /// </summary>
        /// <param name="requestUrl">The url you want to query</param>
        /// <returns>The deserialized object</returns>
        private object MakeRequest<T>(string requestUrl) {
            Logger.info("Fetching json");

            try {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(
                            String.Format("Server error (HTTP {0}: {1}).",
                            response.StatusCode, response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                    return jsonSerializer.ReadObject(response.GetResponseStream());
                }
            } catch (Exception e) {
                Logger.error(e.Message);
            }

            // Request failed, return null;
            return null;
        }
    }

}
