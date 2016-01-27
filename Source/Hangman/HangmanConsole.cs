﻿using System;
using Hangman.Commands;
using Sugar.Command;

namespace Hangman
{
    /// <summary>
    /// Hangman console application
    /// </summary>
    public class HangmanConsole : BaseConsole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HangmanConsole"/> class.
        /// </summary>
        public HangmanConsole()
        {
            // Only process command line switches start with "--"
            Switches.Clear();
            Switches.Add("--");
        }

        /// <summary>
        /// Entry point for the program logic
        /// </summary>
        protected override int Main()
        {
            var exitCode = Arguments.Count > 0 ? Run(typeof(ExecuteProcess), Arguments) : Default();

            return exitCode;
        }

        /// <summary>
        /// Runs the specified parameters.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int Run(Type commandType, Parameters parameters)
        {
            // Assign current parameters
            Parameters.SetCurrent(parameters.ToString());

            var command = (ICommand) Activator.CreateInstance(commandType);
            
            command.BindParameters(parameters);
            
            return command.Execute();
        }
        
        /// <summary>
        /// Displays usage information
        /// </summary>
        /// <returns></returns>
        public int Default()
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
