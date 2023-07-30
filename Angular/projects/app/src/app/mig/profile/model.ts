import { MapDetailsStat } from '../overview/model';
import { TalentStatistic } from '../talent-details/model';

export interface ProfileRequest {
  GameMode: string;
  PlayerId?: number;
  EventId?: number;
  GameModeForMmr: string;
  Time: string;
  Tab: number;
  HeroDetails?: string;
  MapDetails?: string;
  ReplayDetails?: number;
}

export interface ProfileCharacterStatisticsRow {
  HeroPortraitURL?: string;
  PrimaryName?: string;
  Character?: string;
  CharacterURL?: string;
  CharacterLevel?: number;
  GamesPlayed?: string;
  GamesPlayedValue?: number;
  AverageLength?: Date;
  WinPercent?: string;
  WinPercentValue?: number;

  // client properties
  Summary?: TalentStatistic[];
  SummaryLoading?: boolean;
}

export interface ProfileFriendsRow {
  HeroPortraitURL: string;
  PlayerID: number;
  PlayerName: string;
  FavoriteHero: string;
  FavoriteHeroURL: string;
  GamesPlayedWith: string;
  WinPercent: string;
  CurrentMMR?: number;
}

export interface MapStatisticsRow {
  MapImageURL?: string;
  Map?: string;
  MapNameLocalized?: string;
  GamesPlayed?: string;
  GamesPlayedValue?: number;
  AverageLength?: Date;
  WinPercent?: string;
  WinPercentValue?: number;

  // client properties
  Summary?: MapDetailsStat[];
  SummaryLoading?: boolean;
}

export interface ProfileWinRateVsRow {
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
  GamesPlayed: string;
  WinPercent: string;
  Role: string;
  AliasCSV: string;

  RelativeWinPercent: number;
}

export interface ProfileSharedReplayRow {
  /** @format int32 */
  ReplayShareID?: number;

  /** @format int32 */
  ReplayID?: number;

  /** @format int32 */
  UpvoteScore?: number;
  GameMode?: string;
  Title?: string;
  Map?: string;
  ReplayLength?: Date;
  ReplayLengthMinutes?: Date;
  Characters?: string;

  /** @format double */
  AverageCharacterLevel?: number;

  /** @format double */
  AverageMMR?: number;

  /** @format date-time */
  TimestampReplay?: string;

  /** @format date-time */
  TimestampReplayDate?: string;
}

export interface ProfileSharedReplayDetailRow {
  /** @format int32 */
  ReplayID?: number;

  /** @format int32 */
  PlayerID?: number;
  PlayerName?: string;
  Character?: string;
  CharacterURL?: string;

  /** @format int32 */
  CharacterLevel?: number;
  TalentImageURL01?: string;
  TalentImageURL04?: string;
  TalentImageURL07?: string;
  TalentImageURL10?: string;
  TalentImageURL13?: string;
  TalentImageURL16?: string;
  TalentImageURL20?: string;
  TalentNameDescription01?: string;
  TalentNameDescription04?: string;
  TalentNameDescription07?: string;
  TalentNameDescription10?: string;
  TalentNameDescription13?: string;
  TalentNameDescription16?: string;
  TalentNameDescription20?: string;
  Team?: boolean;

  /** @format int32 */
  MmrBefore?: number;

  /** @format int32 */
  MmrChange?: number;
  TalentName01?: string;
  TalentName04?: string;
  TalentName07?: string;
  TalentName10?: string;
  TalentName13?: string;
  TalentName16?: string;
  TalentName20?: string;
}

export interface PlayerProfileCharacterRoleStatistic {
  Role?: string;

  /** @format int32 */
  GamesPlayed?: number;

  /** @format double */
  WinPercent?: number;
}

export interface ProfileResult {
  Title?: string;
  Unauthorized?: boolean;
  HeaderLinks?: string;
  CharacterStats?: ProfileCharacterStatisticsRow[];
  HeroDetails?: TalentStatistic[];
  MapStats?: MapStatisticsRow[];
  MapDetails?: MapDetailsStat[];
  WinRateVsStats?: ProfileWinRateVsRow[];
  WinRateWithStats?: ProfileWinRateVsRow[];
  FriendsStats?: ProfileFriendsRow[];
  RivalsStats?: ProfileFriendsRow[];
  ReplaySearchStats?: ProfileSharedReplayRow[];
  ReplayDetails?: ProfileSharedReplayDetailRow[];
  MilestoneChart?: string;
  WinRateChart?: string;
  RoleStats?: PlayerProfileCharacterRoleStatistic[];
  GeneralInformation: [string, string][];
}
