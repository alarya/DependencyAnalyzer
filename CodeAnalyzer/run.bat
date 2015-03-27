:run.bat
:
:runs CodeAnalyzer
"Executive/bin/debug/Executive.exe" Parser *.cs,*.txt,*.doc 
"Executive/bin/debug/Executive.exe" Parser *.cs,*.txt,*.doc /s
"Executive/bin/debug/Executive.exe" Parser *.cs /sx
"Executive/bin/debug/Executive.exe" Parser *.cs /r
"Executive/bin/debug/Executive.exe" Parser *.cs /srx
