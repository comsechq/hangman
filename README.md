Hangman - Hung Process Monitor
==============================

Windows console application to detect and restart hung console processes.  Usage:

    Hangman - Monitor for Hung Command Line Processes
    https://github.com/comsechq/hangman
    
    Hangman monitors a command line process and kills it if no data is witten
    to the standard output within a timeout.

    Usage:

    hangman --file [executable] --timeout [seconds] --log [file]

    Options:

      --file      Executeable process to run
      --timeout   Timeout in seconds.  If no data is recieved on either the
                  standard output or standard error within this period, then
                  the process is forcably terminated
      --log       Log process terminations to given file

    Any additional command line parameters are passed through to the executable
    process.

You can download the [current version here][1].


[1]: https://github.com/comsechq/hangman/releases/download/v1.0.1/Hangman-v1.0.1.zip "Version 1.0.1"
