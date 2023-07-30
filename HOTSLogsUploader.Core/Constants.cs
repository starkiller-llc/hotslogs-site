namespace HOTSLogsUploader.Core
{
    public static class Constants
    {
#if LOCALDEBUG
        public const string hotslogsUrl = @"http://localhost:4200";
        public const string hotslogsApiUrl = @"https://localhost:44321/";
#elif DEBUG
        public const string hotslogsUrl = @"https://dev.hotslogs.com";
        public const string hotslogsApiUrl = @"https://dev.hotslogs.com/api/";
#else
        public const string hotslogsUrl = @"https://www.hotslogs.com";
        public const string hotslogsApiUrl = @"https://www.hotslogs.com/api/";
#endif
        public const string heroesProfilesUploadEndpoint = @"https://api.heroesprofile.com/api/upload/alt";

        public const int MaximumWaitTime = 30000;
        public const int MaximumFileSize = 20000000;
        public const int FingerprintQueryPageSize = 20;
        public const int FileUploadPageSize = 10;

        public const string ReplayExtensionFilter = "*.StormReplay";
        public const string UpdateFileName = "update.exe";
        public const string BackupInstanceExt = ".bak";

        public static readonly string[] ReplayDateFormat =
        {
            "yyyy-MM-dd HH.mm.ss"
        };
    }
}
