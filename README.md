Hangman - Hung Process Monitor
---

Windows console application to detect and restart hung console processes. You can download the [latest version here][1].

##Usage

```
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
```

##Example

```
hangman.exe --file c:\path\to\myconsoleapp.exe --timeout 90 -param1 value1 -param2 value2
```

The command above will start `hangman.exe`. This hangman will in turn spawn `myconsoleapp.exe` as a new process and redirects its output as well its input.
If the `myconsoleapp.exe` does not output anything for 90 second or more a `CTRL+C` will be sent to the input of `myconsoleapp.exe`.
If `myconsoleapp.exe` listens for `CTRL+C` it can then execute some code to end whatever it's doing gracefully.

##License

This project is licensed under the terms of the [MIT license](https://github.com/comsechq/hangman/blob/master/LICENSE.txt). 

By submitting a pull request for this project, you agree to license your contribution under the MIT license to this project.

[1]: https://github.com/comsechq/hangman/releases "Releases"
