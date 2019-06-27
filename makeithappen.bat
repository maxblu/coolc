@echo off

echo Cleanning...
call clean.bat

echo Executing ANTLR-4.7...
call run-antlr4.bat

if "%ERRORLEVEL%" NEQ "0" (
	echo.
	echo [ERROR:"%ERRORLEVEL%"....Exiting]
	pause
	exit
)

@echo Moving...
MOVE /Y src\coolc\Grammar\CoolLexer.cs src\coolc\Lexer\CoolLexer.cs
MOVE /Y src\coolc\Grammar\CoolLexer.tokens src\coolc\Lexer\CoolLexer.tokens
MOVE /Y src\coolc\Grammar\CoolParser.cs src\coolc\Parser\CoolParser.cs
MOVE /Y src\coolc\Grammar\Cool.tokens src\coolc\Parser\Cool.tokens
rem MOVE /Y src\coolc\Grammar\CoolBaseListener.cs src\coolc\Utils\CoolBaseListener.cs
MOVE /Y src\coolc\Grammar\CoolBaseVisitor.cs src\coolc\Utils\CoolBaseVisitor.cs
rem MOVE /Y src\coolc\Grammar\CoolListener.cs src\coolc\Utils\CoolListener.cs
MOVE /Y src\coolc\Grammar\CoolVisitor.cs src\coolc\Utils\CoolVisitor.cs

if "%ERRORLEVEL%" NEQ "0" (
	echo.
	echo [ERROR:"%ERRORLEVEL%"....Exiting]
	pause
	exit
)

::@dir src\coolc\Grammar
echo Building...
call build.bat

if "%ERRORLEVEL%" NEQ "0" (
	echo.
	echo [ERROR:"%ERRORLEVEL%"....Exiting]
	pause
	exit
)

rem echo.
rem echo Saving...
rem call save.bat


echo.
echo Testing...
call test.bat

echo.
pause