using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FetchWallpaper {

    public class Logger {

        // Constants
        private const string LOG_FILE = "log.txt";
        private const long LOG_SIZE = 51200;

        /// <summary>
        /// This will log a message as a info message
        /// </summary>
        /// <param name="message">Log the message as a info message</param>
        public static void info(string message) {
            write("[INFO] " + message);
        }

        /// <summary>
        /// This will log a message as a warning message
        /// </summary>
        /// <param name="message">Log the message as a warning message</param>
        public static void warn(string message) {
            write("[WARN] " + message);
        }

        /// <summary>
        /// This will log a message as a error message
        /// </summary>
        /// <param name="message">Log the message as a warning message</param>
        public static void error(string message) {
            write("[ERROR] " + message);
        }

        /// <summary>
        /// This will log a message as a severe message
        /// </summary>
        /// <param name="message">Log the message as a severe message</param>
        public static void severe(string message) {
            write("[SEVERE] " + message);
        }

        /// <summary>
        /// This will write the given message to the log file
        /// </summary>
        /// <param name="message">The message you want to log</param>
        private static void write(string message) {
            try {
                // Check if we need to rotate the logfile
                if (getLogFileSize() > LOG_SIZE)
                    rotate();

                // Create the text
                string text = getCurrentTime() + " > " + message;

                // Print to console too
                Console.WriteLine(text);

                // Append the new text
                using (StreamWriter sw = File.AppendText(LOG_FILE))
                    sw.WriteLine(text);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// This function will try to get the size of the current log file
        /// </summary>
        /// <returns>The size of the </returns>
        private static long getLogFileSize() {
            // Check if the log file exists
            if (!File.Exists(LOG_FILE))
                return 0;

            // Return the size;
            return new FileInfo(LOG_FILE).Length;
        }

        /// <summary>
        /// This will return the current time
        /// </summary>
        /// <returns>the current time</returns>
        private static string getCurrentTime() {
            return DateTime.Now.ToString();
        }

        /// <summary>
        /// This method will make sure the log file doesn't get too big.
        /// The first line of the log file will be delete when invoking this function.
        /// </summary>
        private static void rotate() {
            string tempFile = LOG_FILE + ".rotated";
            
            // Move it to a temp file
            File.Move(LOG_FILE, tempFile);

            // Write to file
            using (StreamWriter sw = File.AppendText(LOG_FILE)) {
                // Read the file
                using (StreamReader sr = new StreamReader(tempFile)) {
                    // Ignore the first one
                    sr.ReadLine();

                    // Read and write till the end
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        sw.WriteLine(line);
                }
            }

            // Delete the temp file
            File.Delete(tempFile);
        }

    }

}
