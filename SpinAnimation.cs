using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoPMCLI
{
    /// <summary>
    /// Create spinning console busy animation running on a background thread
    /// </summary>
    public static class SpinAnimation
    {
        // Spinner background thread
        private static System.ComponentModel.BackgroundWorker spinner = InitialiseBackgroundWorker();

        // Starting position of spinner changes to current position on start
        private static int spinnerPosition = 25;

        // Pause time in milliseconds between each character in the spin animation
        private static int spinWait = 25;

        // Field and property to inform client if spinner is currently running
        private static bool isRunning;

        public static bool IsRunning => isRunning;

        /// <summary>
        /// Worker thread factory
        /// </summary>
        /// <returns>Background worker thread</returns>
        private static System.ComponentModel.BackgroundWorker InitialiseBackgroundWorker()
        {
            var obj = new System.ComponentModel.BackgroundWorker();

            // Allow cancellation to be able to stop the spinner
            obj.WorkerSupportsCancellation = true;

            // Anonymous method for background thread's DoWork event
            obj.DoWork += delegate
            {
                // Set the spinner position to the current console position
                spinnerPosition = Console.CursorLeft;

                // Run animation unless a cancellation is pending
                while (!obj.CancellationPending)
                {
                    // Characters to iterate through during animation
                    char[] spinChars = { '|', '/', '-', '\\' };

                    // Iterate through the animation character array
                    foreach (char spinChar in spinChars)
                    {
                        // Reset the cursor position to the spinner position
                        Console.CursorLeft = spinnerPosition;

                        // Write the current character to the console
                        Console.Write("  " + spinChar + " Processing...");

                        // Pause for smooth animation - set by the Start method
                        System.Threading.Thread.Sleep(spinWait);
                    }
                }
            };

            return obj;
        }

        /// <summary>
        /// Start the animation
        /// </summary>
        /// <param name="spinWait">Wait time between spin steps in milliseconds</param>
        public static void Start(int spinWait)
        {
            // Set the running flag
            isRunning = true;

            // Process spinWait value
            SpinAnimation.spinWait = spinWait;

            // Start the animation unless already started
            if (!spinner.IsBusy)
                spinner.RunWorkerAsync();
            else
                throw new InvalidOperationException("Cannot start spinner whilst spinner is already running");
        }

        /// <summary>
        /// Overloaded Start method with default wait value
        /// </summary>
        public static void Start()
        {
            Start(25);
        }

        /// <summary>
        /// Stop the spin animation
        /// </summary>
        public static void Stop()
        {
            // Stop the animation
            spinner.CancelAsync();

            // Wait for cancellation to complete
            while (spinner.IsBusy)
                System.Threading.Thread.Sleep(100);

            // Reset the cursor position
            Console.CursorLeft = spinnerPosition;

            // Set the running flag
            isRunning = false;
        }
    }
}
