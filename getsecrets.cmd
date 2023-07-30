git status --porcelain --ignored | grep -v /$ | grep -v .user$ | grep -v /App_Data/ | grep -v .tgz$ | cut -c4- | tee secret_file_paths.txt
