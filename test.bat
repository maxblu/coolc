@ECHO OFF

:: for %%i in (test\*.cool) do bin\coolc.exe %%i

:: for %%i in (test\*.cl) do (
:: 	bin\coolc.exe "%%i"
:: 	if "%ERRORLEVEL%" NEQ "0" (
::     	echo [ERROR:"%ERRORLEVEL%"....Exiting]
::     	pause
::     	exit
:: 	)
:: )


@echo.
@echo Cool Code
@echo.
for %%i in (test\*.cl) do (
	bin\coolc.exe "%%i"
)

@pause