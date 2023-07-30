rem vdir "HOTSlogs - Prod/ang/"
rem apppool "HotsLogsApp"

pushd Angular
call npm run build:app
popd
@echo on
c:\Windows\system32\inetsrv\appcmd.exe set vdir "HOTSlogs - Prod/ang/" -physicalPath:"c:\HotsLogsApp-pub"
sleep 2
c:\Windows\system32\inetsrv\appcmd.exe recycle apppool "HotsLogsApp"
sleep 10
ren c:\HOTSLogsApp HOTSLogsApp-tmp2
xcopy /s /y c:\HOTSLogsApp-pub c:\HOTSLogsApp\
c:\Windows\system32\inetsrv\appcmd.exe set vdir "HOTSlogs - Prod/ang/" -physicalPath:"c:\HOTSLogsApp"
rd /s /q c:\HOTSLogsApp-tmp2
