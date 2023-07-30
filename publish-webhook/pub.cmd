@echo off
if [%1]==[] goto usage
%MSBUILD% ..\Heroes.WebApplication\HeroesWebApplication.csproj -p:DeployOnBuild=true -p:PublishProfile=HOTSLogs-Main-%1 -p:Configuration=Release
if %errorlevel% neq 0 (
    set a=%errorlevel%
    @echo Build failed - not publishing.
    exit /b %a%
)
cscript.exe /Nologo %systemdrive%\inetpub\adminscripts\adsutil.vbs set /w3svc/1/ROOT/Path C:\HOTSLogs-Main-%1
npm start
goto :eof
:usage
@echo Usage: %0 ^<A^|B^|C^>
@echo.
@echo This will build and publish Heroes.WebApplication into C:\HOTSLogs-Main-A or B or C
@echo.
@echo Current website served from:
cscript.exe /Nologo %systemdrive%\inetpub\adminscripts\adsutil.vbs get /w3svc/1/ROOT/Path | cut -c45- | rev | cut -c3- | rev
exit /B 1
