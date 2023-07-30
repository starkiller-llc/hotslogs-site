# How to Add a New Hero to Hotslogs

## Database Changes

Add a new row to table 'LocalizationAlias'.  This table is used for new Heroes and new Maps.  This adds the Hero to most places on the site.  Once added, this new Hero will be added to different Hero dropdown menus and such.  There isn't an editor for this, so you'll have to insert into the database manually.  I use MySQL Workbench, which is pretty good for inserting/updating single rows.

Columns in LocalizationAlias:

**IdentifierID:** Unique identifier integer.  Just use the next available integer.  Heroes start at 1, and Maps start at 1001.  This identifier is used in the 'ReplayCharacter.CharacterID' database row of each player in a game.

**Type:** Enum, Heroes = 1, Maps = 0

**Primary Name:** Full English name, as written in the replay file.  For new heroes when you don't have access to a replay file, I just guess at how the name will be written.  If this isn't exactly the same as how the Hero name is written in the replay file, then the replay parser won't know which hero was played, and new replays parsed with this hero will be saved with the default CharacterID of 0.  These replays will need to be reparsed after you find the correct Hero name and update this record.  See the bottom of this article for how to double check this value.

**Attribute Name:** A string of 4 case sensitive characters that identifies this Hero in certain parts of game files.  This isn't used too much, but it is important.  I need this exact value in order to automatically extract hero talent names and descriptions from the HotS game client.  I think this attribute name is also used to extract Hero bans from ranked play replay files.  It might be used for other statistics as well.  Usually it is just the first 4 characters of a Hero's name, but sometimes it is something different.  See the bottom of this article for how to double check this value.

**Group:** This is Blizzard's classification.  Pick from 'Warrior', 'Assassin', 'Support', 'Specialist'.  This doesn't yet support multi class heroes, so just pick whichever class is most popular for that hero.

**Sub Group:** This is Hotslogs' classification, which is more specific than Blizzard's.  https://www.hotslogs.com/Info/HeroSubRole.  Pick whichever you think is most appropriate for a new hero.  Often times this isn't obvious, and you'll get some users who complain that a hero should be in a different sub role depending on how they are played.  These two columns here, 'Group' and 'Sub Group', drive statistics based on these roles.  For example, on the player profile, you can see your win rate when playing as 'Assassin' group heroes, or 'Bruiser' sub group heroes.  There is also some sitewide Team Composition statistics that use these groups and sub groups.  It is fine to change a heroes' Group or Sub Group later on if you change your mind or if the community really wants something different.  Just be aware that changes will retroactively apply to player profiles and such.

**Aliases CSV:** This is a CSV field of 'Primary Names' of this Hero, in different languages.  If you don't enter these, then when replay files are uploaded in those languages, they won't properly record the Hero, and will need to be reparsed after you enter the names.  I don't usually try to get this before the Hero release, because it's very difficult to get spelling correct in different languages :)  See the bottom of this article for how to identify the correct translations after replays are uploaded.

Here's an example record.  You should probably also look at the database and all the different records for more examples

**Identifier ID**: 78

**Type:** 1

**Primary Name:** Fenix

**Attribute Name:** FENX

**Group:** Assassin

**SubGroup:** Sustained Damage

**Aliases CSV:** 피닉스,菲尼克斯,Феникс,Fénix

# Hero Portrait Image

Create a 75 x 75 pixel png image of the Hero, usually centered on the heroes' face.  You can look at other heroes for examples.  The file name should be the Primary Name for this hero, with no spaces or punctuation.  The filename is case sensitive because our CDN, Amazon CloudFront, is case sensitive.

Upload this image to the Amazon S3 bucket 'hotslogs-cloudfront', which will be deployed to the CloudFront CDN automatically.  The path in this bucket is '/Images/Heroes/Portraits'.  I use the free version of 'S3 Browser' to upload this, but you can use any Amazon S3 bucket client.  This CDN is used for 90% of hero image use on the site.  For example: https://d1i1jxrdh2kvwy.cloudfront.net/Images/Heroes/Portraits/Fenix.png

Copy this same image to the website's local Images folder as well, as some of the web application's server code needs access to the raw file.  Copy the file here (Heroes.WebApplication\Images\Heroes\Portraits), and make sure you also include the image in the Visual Studio project, or the image won't be deployed when you publish.

# Hero Name Translations

The Aliases CSV column in the database is used to parse the replay file.  To also support displaying hero names on Hotslogs in different languages, you also need to add the translations to the web application.  If you don't update a specific language, then it will just display the English hero name.  Over the past couple years, I've only been updating Hero names in English, Korean, Russian, Simplified Chinese, and Traditional Chinese.  The rest of the languages are neglected, but it was too much effort for me to maintain all.

In Visual Studio, open the folder 'App_GlobalResources', and then open the language file you want to update.  For example, 'LocalizedText.ko.resx' is the Korean localization file.  This is basically a list of keys and values, and each language can have a different value for one key.  The key format for hero names is 'GenericHero{0}', and then the Hero name as it is written in the image file, which is the primary name without any spaces or punctuation.  For example, 'GenericHeroTheButcher', and the value would be the hero name as you would like it to display for a Korean user browsing Hotslogs.com.  You can see these values in action by looking at the home page of Hotslogs.com, and changing the language dropdown at the bottom of the page.

## How to Verify Primary Name, Attribute Name, Aliases CSV

_Ben will fill this in later_