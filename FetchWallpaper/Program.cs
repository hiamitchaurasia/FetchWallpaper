using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Runtime.InteropServices; 

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
        private const string API_CALL_RANDOM_FEATURED = API + "wallpapers/type/random_featured/number/100/apikey/" + API_KEY + "/format/json";

        private const int DELAY = 3600 * 1000;
        private const int DELAY_ERROR = 60 * 1000;
        
        // Main method where this program starts
        static void Main(string[] args) {
            // Hide the console window
            ShowWindow(GetConsoleWindow(), 0);

            // Prepare
            bool run = true;
            bool succes = false;

            // Run
            while (run) {
                try {
                    // Start the engines
                    succes = new Program().start(args);
                } catch (Exception e) {
                    Logger.severe(e.Message);
                }

                // Check if we had errors and break this
                if (succes) {
                    Thread.Sleep(DELAY);
                } else {
                    Thread.Sleep(DELAY_ERROR);
                    // reset
                    succes = false;
                }
            }
        }

        /// <summary>
        /// Method that executes the program
        /// </summary>
        public bool start(string[] args) {
            // CALL API method and check if the response is valid
            Response wallpapersResponse = MakeRequest(API_CALL_RANDOM_FEATURED);
            if (wallpapersResponse == null)
                return false;

            if (wallpapersResponse.wallpapers.Length == 0) {
                Logger.error("No wallpapers found. Something wrong with the API?");
                return false;
            }

            // Create list of wanted categories
            List<int> categories = new List<int>();

            for (int i = 0; i < args.Length; i++) {
                int value;
                // try parse the value
                if (int.TryParse(args[i], out value))
                    categories.Add(value);
            }

            // Check if we have valid categories, else just set the first wallpaper in response
            if (categories.Count == 0) {
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
                if (categories.Contains(temp.catid)) {
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
        /// This function will query the given url and parses it to a deserialized object
        /// </summary>
        /// <param name="requestUrl">The url you want to query</param>
        /// <returns>The deserialized object</returns>
        private Response MakeRequest(string requestUrl) {
            Logger.info("Fetching json");

            try {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(
                            String.Format("Server error (HTTP {0}: {1}).",
                            response.StatusCode, response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    Response jsonResponse = objResponse as Response;
                    // Return the response
                    return jsonResponse;
                }
            } catch (Exception e) {
                Logger.error(e.Message);
            }

            // Request failed, return null;
            return null;
        }
    }

}
