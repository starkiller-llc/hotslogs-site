## HOTS Logs Uploader

This is the project that builds the HOTS Logs Uploader

The uploader uploads replay files directly to AWS S3, using an AWS access key with permissions specifically to write to the AWS S3 replay bucket.  The access key does not have permission to read or list files in that bucket.  After uploading, it makes a GET request to a Hotslogs.com/UploadFile, which tells Hotslogs.com to parse the replay and add it to the database.

Here is the code that uploads the replay file: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSLogsUploader/Form1.cs#L161
Here is the code on Hotslogs.com that saves the replay data to the database: https://github.com/zamnetwork/HOTSLogs/blob/master/Heroes.WebApplication/UploadFile.aspx.cs#L38

The uploader also detects when your local machine is starting or finishing a Heroes of the Storm match, and opens the Match Preview or Match Summary page on Hotslogs.com.

The Match Summary page is simple enough - we just check if a replay file was uploaded recently, and display summary statistics.
https://github.com/zamnetwork/HOTSLogs/blob/master/Heroes.WebApplication/Player/MatchSummary.ascx.cs

The Match Preview page is more complicated.  We can see players in a match using part of a replay file that is created during the Heroes of the Storm match loading screen.  I haven't been able to find selected heroes in this partial replay file though, so I instead use OCR to read player and hero names off of the loading screen image.  The OCR code is too bulky to ship in the uploader, so instead I host the OCR code on Hotslogs.com, and the uploader instead sends us parts of a screenshot that have the hero names.
https://github.com/zamnetwork/HOTSLogs/blob/master/Heroes.WebApplication/Player/MatchPreview.aspx.cs

The uploader can automatically update itself too.  It's pretty crude.  The uploader is just a single .exe right now.  It checks if there's a new version, and if so, writes a .bat file that overwrites the uploader .exe and launches the new one.

To publish a new version of the uploader:
1. Increment the version number here: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSLogsUploader/Form1.cs#L28
1. Build the uploader project in Release mode, and then use ilmerge to merge all the dll dependencies into one standalone .exe: https://bitbucket.org/wvd-vegt/ilmergegui
1. Copy the new merged .exe here: https://github.com/zamnetwork/HOTSLogs/tree/master/Heroes.WebApplication/HOTSLogsUploader
1. Publish the Hotslogs.com web application

Make sure you increment the version number before you build the uploader.  If you publish Hotslogs.com with a new version number, but the uploader has the old version number, then users' uploaders will constantly reupdate themselves.

## HOTS Windows Service

This automatically generates most of the aggregate statistics shown on Hotslogs.

Here is the start of the service.  These are the individual tasks, and how often they run:

https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Service1.cs#L36-L47

Currently, the tasks that run are:

* Generate Group Finder Listings (RedisGroupFinderListing, No longer used)
* Generate listing of Shared Replays (RedisSharedReplays)
* Calculate Player MMR (MMRProcessRecentDates and MMRProcessOlderDates)
* Generate the Leaderboard (MMRProcessLeaderboard)
* Aggregate Sitewide Statistics (RedisSitewideCharacterAndMapStatistics and RedisSitewideCharacterAndMapStatisticsRecent)
* Import replays from HotsApi (HotsApiGobbler)
* Generate Hero Leaderboard (ProcessHeroLeaderboard)
* Aggregate Sitewide Heat Maps (GatherHeatMap)
* Aggregate Sitewide Match Awards (RedisMatchAwards)
* Export a chunk of user-downloadable data (ExportReplayData)

You can find the code for each of these tasks here: https://github.com/zamnetwork/HOTSLogs/tree/master/HOTSWindowsService/Services

For example, in RedisSitewideCharacterAndMapStatistics, here is the database query and code used to generate the statistics for the table on the home page: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Services/RedisSitewideCharacterAndMapStatistics.cs#L995-L1109

Database query: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Services/RedisSitewideCharacterAndMapStatistics.cs#L997-L1028

Database query parameters, such as date range, game mode, league, or specific build of HotS: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Services/RedisSitewideCharacterAndMapStatistics.cs#L1039-L1042

Here is where the query output is stored in Redis.  We run the above database query for every possible combination of filters that are available on Hotslogs.com: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Services/RedisSitewideCharacterAndMapStatistics.cs#L2452-L2532

Here you can see how the Redis key is formatted: https://github.com/zamnetwork/HOTSLogs/blob/master/HOTSWindowsService/Services/RedisSitewideCharacterAndMapStatistics.cs#L2462

"HOTSLogs:SitewideCharacterStatisticsV2:Current:" + leagueID + ":" + gameMode

So for example, the statistics shown on the home page are for all leagues, and game mode 'Hero league'.  In the Redis key, 'leagueID' would be -1, and gameMode would be 4.

Sure enough, in the web application, we can see the JSON being pulled from this Redis key: https://github.com/zamnetwork/HOTSLogs/blob/master/Heroes.WebApplication/Default.aspx.cs#L33

## Heroes.WebApplication

_To Do..._

## Heroes.ReplayParser

_To Do..._

## Misc

### Data Helper

A project with code that is used by several other projects

### HOTS Test Application

This is just a sandbox console application for me to debug arbitrary code.

### File Move Utility, HOTS Logs Replay Exporter

Some things I worked on briefly for Heroes of the Dorm.  They are incomplete and not really great.  Probably never use them.

### Custom Profile Image, League Images, hotslogs-logos

Raw, high resolution images for different parts of the site

### artificial_reason_121

This is the base Bootstrap template I started Hotslogs.com from: https://wrapbootstrap.com/theme/artificial-reason-responsive-full-pack-WB0307B17

### Translations.ods

This is information on community members that helped me years ago to translate parts of Hotslogs into different languages.

### To Do

My partial thoughts and ideas on new things that could be added to Hotslogs.com

### SQL Queries

Some saved queries that I run manually once in awhile.  These are not used by the web application, they are just some manual queries I've written for different reasons.