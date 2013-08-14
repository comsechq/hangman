using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using Sugar.Command;

namespace Hangman.Commands
{
    /// <summary>
    /// Executes a child process and terminates it if it doesn't produce
    /// and output for the specified amount of time.
    /// </summary>
    public class ExecuteProcess : BoundCommand<ExecuteProcess.Options>
    {
        /// <summary>
        /// Command options
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Gets or sets the name of the file.
            /// </summary>
            /// <value>
            /// The name of the file.
            /// </value>
            [Parameter("file", Required = true)]
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the timeout.
            /// </summary>
            /// <value>
            /// The timeout.
            /// </value>
            [Parameter("timeout", Default = "600")]
            public int Timeout { get; set; }

            /// <summary>
            /// Gets or sets the name of the log file.
            /// </summary>
            /// <value>
            /// The name of the log file.
            /// </value>
            [Parameter("log", Required = false)]
            public string LogFileName { get; set; }
        }

        private DateTime lastOutput;
        private string logFileName;
        private string windowTitle;
        private Process process;
        private Timer timer;
        private int timeout;

        /// <summary>
        /// Executes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override int Execute(Options options)
        {
            logFileName = options.LogFileName;

            InitializeTimer(options);
            InitializeProcess(options);

            timer.Start();
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            timer.Stop();

            return process.ExitCode;
        }

        /// <summary>
        /// Initializes the timer used to determine if the process has hung.
        /// </summary>
        /// <param name="options">The options.</param>
        private void InitializeTimer(Options options)
        {
            timer = new Timer {Interval = 1000};
            timer.Elapsed += timer_Elapsed;

            lastOutput = DateTime.Now;
            timeout = options.Timeout;
        }

        /// <summary>
        /// Initializes the process to monitor.
        /// </summary>
        /// <param name="options">The options.</param>
        private void InitializeProcess(Options options)
        {
            var parameters = Parameters.Current;
            parameters.Remove("file");
            parameters.Remove("timeout");
            parameters.Remove("log");

            var startInfo = new ProcessStartInfo
            {
                FileName = options.FileName,
                Arguments = parameters.ToString(),
                WindowStyle = ProcessWindowStyle.Minimized,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += process_OutputDataReceived;

            windowTitle = options.FileName + " " + parameters;
        }

        /// <summary>
        /// Handles the Elapsed event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var elapsed = Convert.ToInt32((DateTime.Now - lastOutput).TotalSeconds);

            if (elapsed > 9)
            {
                Console.Title = "Hung " + elapsed + " secs - " + windowTitle;
            }
            else
            {
                Console.Title = windowTitle;
            }


            if (elapsed > timeout)
            {
                Console.WriteLine("Process timed out, terminating...");
                process.Kill();

                if (!string.IsNullOrEmpty(logFileName))
                {
                    LogProcessKill(logFileName, process);
                }
            }
        }

        /// <summary>
        /// Logs the process kill.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="killedProcess">The killed process.</param>
        private void LogProcessKill(string fileName, Process killedProcess)
        {
            try
            {
                var content = string.Format("{0:yyyy-MM-dd HH:mm:ss} : Killed Process '{1}' (PID: {2})", DateTime.Now, killedProcess.ProcessName, killedProcess.Id);

                File.AppendAllText(fileName, content);
            }
            catch
            {
                // Nothing
            }
        }

        /// <summary>
        /// Handles the OutputDataReceived event of the process control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);

            lastOutput = DateTime.Now;
        }
    }
}
