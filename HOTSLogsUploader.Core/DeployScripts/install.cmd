mkdir %LOCALAPPDATA%\HotsLogsUploaderSetup
copy files2.tgz %LOCALAPPDATA%\HotsLogsUploaderSetup
cd %LOCALAPPDATA%\HotsLogsUploaderSetup
tar xzf files2.tgz --strip-components=1
del files2.tgz
setup.exe
