@echo off
if not defined VisualStudioVersion (
    if defined VS140COMNTOOLS (
        call "%VS140COMNTOOLS%\VsDevCmd.bat"
        goto :Build
    )

    if defined VS120COMNTOOLS (
        call "%VS120COMNTOOLS%\VsDevCmd.bat"
        goto :Build
    )

    if defined VS110COMNTOOLS (
        call "%VS110COMNTOOLS%\VsDevCmd.bat"
        goto :Build
    )

    if defined VS100COMNTOOLS (
        call "%VS100COMNTOOLS%\VsDevCmd.bat"
        goto :Build
    )

    echo Error: build.cmd requires Visual Studio 2010, 2012, 2013 or 2015.  
    exit /b 1
)

:Build
MSBuild DoubanFM.sln /t:Rebuild /p:Configuration=Release
if %errorlevel% NEQ 0 exit /b 1

:Copy
rd bin /s /q
robocopy DoubanFM\bin\Release bin *.dll *.exe *.config version.dat /MIR /XD app.publish
