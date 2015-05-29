@echo off
:CreateInstaller
if exist "%ProgramFiles(x86)%\NSIS\Unicode\makensis.exe" (
    if exist "%ProgramFiles(x86)%\NSIS\Unicode\Plugins\FindProcDLL.dll" (
        for /F %%I IN (bin\version.dat) DO (
            pushd ScriptForInstaller
            make.bat %%I
            popd
        )
        goto :EOF
    )
)

echo NSIS is not found or not ready. Will not create the installer.
exit /b 1
