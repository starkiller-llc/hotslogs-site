rem vdir "HOTSlogs - Prod/api/"
rem apppool "HotsLogsApi"

rd /s /q c:\HOTSLogs-Api-pub
dotnet publish -c Release HotsLogsApi -o c:\HOTSLogs-Api-pub
c:\Windows\system32\inetsrv\appcmd.exe set vdir "HOTSlogs - Prod/api/" -physicalPath:"c:\HOTSLogs-Api-pub"
sleep 2
c:\Windows\system32\inetsrv\appcmd.exe recycle apppool "HotsLogsApi"
sleep 10
ren c:\HOTSLogs-Api HOTSLogs-Api-tmp
xcopy /s /y c:\HOTSLogs-Api-pub c:\HOTSLogs-Api\
c:\Windows\system32\inetsrv\appcmd.exe set vdir "HOTSlogs - Prod/api/" -physicalPath:"c:\HOTSLogs-Api"
rd /s /q c:\HOTSLogs-Api-tmp
