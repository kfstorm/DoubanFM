@echo off
setlocal
set nosign=false
set version=%1
if "%1" == "" echo Version number needed.
if "%1" == "" goto :eof
if "%2" == "" echo Binary directory needed.
if "%2" == "" goto :eof
set projectdir=%cd%\%2
pushd %~dp0
path %WindowsSdkDir%bin;%PATH%
set imagedir=%cd%\Images
set tempdir=%cd%\Temp
set outputdir=%cd%\Output
set compile=C:\Program Files (x86)\NSIS\Unicode\makensis.exe
set setup=DoubanFMSetup_%version%.exe

:copy
xcopy "%imagedir%" "%tempdir%\" /Q/E/Y
if %errorlevel% NEQ 0 goto :clear
xcopy "%projectdir%" "%tempdir%\bin\" /Q/E/Y
if %errorlevel% NEQ 0 goto :clear
xcopy "DoubanFM.nsi" "%tempdir%\" /Q/Y
if %errorlevel% NEQ 0 goto :clear
:compile
"%compile%" "/XOutFile \"%setup%\"" "/X!define PRODUCT_VERSION \"%version%\"" "/X!define DOUBANFM_BINARYDIR \"%tempdir%\bin\"" "%tempdir%\DoubanFM.nsi"
if %errorlevel% NEQ 0 goto :clear
xcopy "%tempdir%\%setup%" "%outputdir%\" /Q/Y
if %errorlevel% NEQ 0 goto :clear

:clear
set error=%errorlevel%
if exist "%tempdir%" rmdir "%tempdir%" /s /q
goto :eof

:eof
endlocal