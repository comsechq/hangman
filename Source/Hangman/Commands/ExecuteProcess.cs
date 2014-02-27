using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using Sugar.Command;
using Timer = System.Timers.Timer;

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
        private bool sentCtrlC;
        
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

            Console.CancelKeyPress += OnCancelledByKeyPress;

            var startInfo = new ProcessStartInfo
                                {
                                    FileName = options.FileName,
                                    Arguments = parameters.ToString(),
                                    WindowStyle = ProcessWindowStyle.Minimized,
                                    UseShellExecute = false,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
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
        /// Called when the CTRL-C or CTRL-BREAK is triggered.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConsoleCancelEventArgs"/> instance containing the event data.</param>
        private void OnCancelledByKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Cancel the event for hangman only on the first CTRL-C or CTRL-BREAK
            // The event will propagate to all child processes
            e.Cancel = !sentCtrlC;
            sentCtrlC = true;
        }

        /// <summary>
        /// Generates the console control event.
        /// </summary>
        /// <param name="dwCtrlEvent">The dw control event (0 for CTRL-C, 1 for CTRL-BREAK).</param>
        /// <param name="dwProcessGroupId">The dw process group identifier (0 to send to all processes).</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

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
                if (!sentCtrlC)
                {
                    Console.WriteLine("Sending CTRL-C to process...");
                    GenerateConsoleCtrlEvent(0, 0);
                }

                if (!string.IsNullOrEmpty(logFileName))
                {
                    if (!string.IsNullOrEmpty(logFileName))
                    {
                        LogHangmanEvent(logFileName, HangmanSignal.CtrlC, process);
                    }
                }
            }
            
            // If it's been too long and the child process is stil running, kill it!
            if (elapsed > timeout + 10)
            {
                Console.WriteLine("Sending kill signal...");
                process.Kill();

                if (!string.IsNullOrEmpty(logFileName))
                {
                    LogHangmanEvent(logFileName, HangmanSignal.Kill, process);
                }
            }
        }

        /// <summary>
        /// Enumeration of the differnent signals that hangman can send to its child process.
        /// </summary>
        private enum HangmanSignal
        {
            CtrlC,
            Kill
        }

        /// <summary>
        /// Logs the process kill.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="sentSignal">The sent signal.</param>
        /// <param name="process">The process.</param>
        private void LogHangmanEvent(string fileName, HangmanSignal sentSignal, Process process)
        {
            try
            {
                var content = string.Format("{0:yyyy-MM-dd HH:mm:ss} : Sent {1} signal to process '{2}' (PID: {3}){4}",
                                            DateTime.Now, sentSignal, process.ProcessName, process.Id,
                                            Environment.NewLine);

                File.AppendAllText(fileName, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while loging the output: " + ex.Message);

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
