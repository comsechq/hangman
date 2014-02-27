using System;
using System.Reflection;
using Hangman.Commands;
using Sugar.Command;

namespace Hangman
{
    /// <summary>
    /// Hangman console application
    /// </summary>
    public class HangmanConsole : BaseCommandConsole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HangmanConsole"/> class.
        /// </summary>
        public HangmanConsole()
        {
            // Add commands
            Commands.Add(new ExecuteProcess());

            // Only process command line switches start with "--"
            Switches.Clear();
            Switches.Add("--");
        }

        /// <summary>
        /// Displays usage information
        /// </summary>
        /// <returns></returns>
        public override int Default()
        {
            Console.WriteLine("Hangman - Monitor for Hung Command Line Processes");
            Console.WriteLine("Version: 1.0.2");
            Console.WriteLine("https://github.com/comsechq/hangman");
            Console.WriteLine();
            Console.WriteLine("Hangman monitors a command line process and kills it if no data is witten");
            Console.WriteLine("to the standard output within a timeout.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("  hangman --file [executable] --timeout [seconds] --log [file]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            Console.WriteLine("  --file      Executeable process to run");
            Console.WriteLine("  --timeout   Timeout in seconds.  If no data is recieved on either the");
            Console.WriteLine("              standard output or standard error within this period, then ");
            Console.WriteLine("              the process is sent a CTRL-C input then forcably terminated");
            Console.WriteLine("  --log       Log process terminations to given file");
            Console.WriteLine();
            Console.WriteLine("Any additional command line parameters are passed through to the executable");
            Console.WriteLine("process.");

            return (int) ExitCode.Success;
        }
    }
}
