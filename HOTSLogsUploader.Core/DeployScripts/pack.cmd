pushd ..\bin
tar czf ..\deployscripts\files2.tgz testpub
popd
iexpress /q /n setup.sed
