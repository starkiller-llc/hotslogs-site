@echo off
pushd publish-webhook
call powershell "./pub.ps1" %*
popd
