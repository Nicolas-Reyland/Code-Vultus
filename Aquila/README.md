# Aquila
You can find an Aquila interpreter here. It is still in development. Come back in June 2021 to have a final product!
If you reallt want to test it, look at the Parser.cs file.

To compile the Interpreter:

On Windows (never tested):
```
cd src
dotnet build
```
On Linux:
```
cd src/Parser
mcs -target:exe -out:program.exe *.cs
```

You can run an Aquila code file by giving its path as argument:

On Windows:
```
program.exe "C:\Path\To\File.aq"
```

On Linux:
```
mono program.exe "/path/to/file.aq"
```

It will print the return value to the stdout (if there is none, prints "none").

To get the interactive mode, start the compiled program with "interactive" as argument.

On Windows:
```
program.exe interactive
```

On Linux:
```
mono program.exe interactive
```

The documentation is unfinished, but you can stil peak into it :^) !

Credits:
 - Nicolas Reyland
 - Daryl Djelou
