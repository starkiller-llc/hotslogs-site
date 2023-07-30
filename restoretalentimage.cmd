rem @echo off
setlocal
FOR /F "tokens=* USEBACKQ" %%F IN (`git log --diff-filter=D --summary ^| grep delete ^| grep %1 ^| head -1 ^| cut -d" " -f5-`) DO (
SET fpath=%%F
)

if "%fpath%"=="" (
  echo Not found - maybe renamed?
  goto :checkrename
)

FOR /F "tokens=* USEBACKQ" %%G IN (`git log  --oneline --no-abbrev-commit --all -- %fpath% ^| head -1 ^| cut -d" " -f1`) DO (
SET commit=%%G
)

echo copied from %commit%~1

git show %commit%~1^^:./%fpath% > HotsLogsApi/wwwroot/Images/Talents/%2%1.png
endlocal
exit /b 0

:checkrename

FOR /F "tokens=* USEBACKQ" %%Z IN (`git log --summary --diff-filter R ^| grep %1 ^| head -1 ^| cut -d" " -f3- ^| cut -d"=" -f2 ^| cut -c3- ^| cut -d"}" -f1`) DO (
SET rpath=%%Z
)

echo %rpath%

if "%rpath%"=="" (
  echo Not renamed - exiting
  exit /b 1
)

echo Renamed from %rpath%

echo copy HotsLogsApi\wwwroot\Images\Talents\%rpath% HotsLogsApi\wwwroot\Images\Talents\%2%1.png
copy HotsLogsApi\wwwroot\Images\Talents\%rpath% HotsLogsApi\wwwroot\Images\Talents\%2%1.png

exit /b 0
