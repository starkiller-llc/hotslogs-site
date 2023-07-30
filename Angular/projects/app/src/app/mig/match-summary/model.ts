export interface MatchSummaryRequest {
  ReplayId: number;
}

export interface AwardRow {
  Code: string;
  Text: string;
}

export interface MatchDetail {
  MatchAwards: string;
  MatchAwards2: AwardRow[];
  PlayerID: number;
  RealPlayerID: number;
  PlayerName: string;
  VoteUp: boolean;
  VoteDown: boolean;
  ShowVoteIcons: boolean;
  Character: string;
  CharacterURL: string;
  HeroPortraitImageURL: string;
  CharacterLevel: number;
  CharacterLevelNumber: number;
  TalentImageURL01: string;
  TalentImageURL04: string;
  TalentImageURL07: string;
  TalentImageURL10: string;
  TalentImageURL13: string;
  TalentImageURL16: string;
  TalentImageURL20: string;
  TalentNameDescription01: string;
  TalentNameDescription04: string;
  TalentNameDescription07: string;
  TalentNameDescription10: string;
  TalentNameDescription13: string;
  TalentNameDescription16: string;
  TalentNameDescription20: string;
  Team: boolean;
  MMRBefore: number;
  MMRChange: number;
  Reputation: number;
  HeaderStart: string;
  TalentName01: string;
  TalentName04: string;
  TalentName07: string;
  TalentName10: string;
  TalentName13: string;
  TalentName16: string;
  TalentName20: string;
}

export interface CharacterScoreResultsTotal {
  Team: string;
  Takedowns: string;
  SoloKills: string;
  Assists: string;
  Deaths: string;
  TimeSpentDead: string;
  HeroDamage: string;
  SiegeDamage: string;
  Healing: string;
  SelfHealing: string;
  DamageTaken: string;
  MercCampCaptures: string;
  ExperienceContribution: string;
}

export interface TeamObjective {
  Team: boolean;
  TimeSpan: string;
  PlayerName?: any;
  Character?: any;
  CharacterURL?: any;
  HeroPortraitURL: string;
  TeamObjectiveType: string;
  Value: string;
}

export interface ScoreResult {
  PlayerID: number;
  PlayerName: string;
  Character: string;
  CharacterURL: string;
  Team: boolean;
  Takedowns: string;
  SoloKills: string;
  Assists: string;
  Deaths: string;
  TimeSpentDead: string;
  HeroDamage: string;
  SiegeDamage: string;
  Healing: string;
  SelfHealing: string;
  DamageTaken: string;
  MercCampCaptures: number;
  ExperienceContribution: string;
  ScoreTooltip: string;
}

export interface TalentUpgradeRow {
  PlayerId: number;
  PlayerName: string;
  HeroPortraitImageURL: string;
  Character: string;
  CharacterURL: string;
  Team: boolean;
  TalentImageURL: string;
  TalentName: string;
  ReplayLengthPercentAtValue0: number;
  ReplayLengthPercentAtValue1: number;
  ReplayLengthPercentAtValue2: number;
  ReplayLengthPercentAtValue3: number;
  ReplayLengthPercentAtValue4: number;
  ReplayLengthPercentAtValue5: number;
}

export interface HeroBanRow {
  Team: boolean;
  BanPhase: number;
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
}

export interface TalentUpgradesStacksRow {
  PlayerId: number;
  PlayerName: string;
  HeroPortraitImageURL: string;
  Character: string;
  CharacterURL: string;
  Team: boolean;
  TalentImageURL: string;
  TalentName: string;
  Stacks: number;
}

export interface MatchSummaryResult {
  PermalinkVisible: boolean;
  ReplayDownloadVisible: boolean;
  ReplayDownloadHref?: any;
  PanelReplayViewerVisible: boolean;
  MapName: string;
  PermalinkHref: string;
  MatchDetails: MatchDetail[];
  CharacterScoreResultsTotals: CharacterScoreResultsTotal[];
  HeroBans?: HeroBanRow[];
  TalentUpgrades?: TalentUpgradeRow[];
  LengthPercentAtValue5Hide: boolean;
  PanelMatchLogVisible: boolean;
  ChartXpSummaryJson: string;
  TalentUpgradesStacks?: TalentUpgradesStacksRow[];
  HideTeamObjectivesHeroField: boolean;
  TeamObjectives: TeamObjective[];
  ScoreResults: ScoreResult[];
  ReplayLength: Date;
  ReplayTime: Date;
}

export interface VoteRequest {
  Up: boolean;
  PlayerId: number;
  ReplayId: number;
  Perform: boolean;
}

export interface VoteResponse {
  ReplayId: number;
  Success: boolean;
  ErrorMessage: string;
  Up?: boolean;
  Down?: boolean;
  VotingPlayer: number;
  TargetPlayer: number;
  SelfRep: number;
  TargetRep: number;
  NeededRep: number;
  Rep: number;
}
