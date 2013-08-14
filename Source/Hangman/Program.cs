namespace Hangman
{
    class Program
    {
        /// <summary>
        /// Main entry point into the application
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            var console = new HangmanConsole();

            console.Run(args);
        }
    }
}
