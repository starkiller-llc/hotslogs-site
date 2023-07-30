Set oShell = CreateObject ("Wscript.Shell")
Dim strArgs
strArgs = "cmd /c install.cmd"
oShell.Run strArgs, 0, true
