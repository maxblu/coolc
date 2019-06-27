@echo off
:: nologo = Doesn't output the csc welcome message
:: t = target exe module uiapp(winform)
:: o = optimize
:: recurse:*.cs = compiles all the matching files
:: out = output program name

:: csc src/coolc/Program.cs /t:exe /out:bin/coolc.exe /nologo /o /lib:src/packages/Antlr4.Runtime.Standard.4.7.0/lib/net35/
::E:/Desktop/coolc/coolc/src/coolc/AST/grammar/antlr4.bat

MSBuild.exe src/coolc/coolc.csproj /property:Configuration=Debug /nologo /verbosity:quiet
rem MSBuild.exe src/coolc/coolc.csproj /property:Configuration=Debug /nologo /verbosity:diag /ds
