interface SitewideCharacterStatistic {
  heroPortraitURL: string;
  character: string;
  gamesPlayed: number;
  gamesBanned: number;
  averageLength: {
    ticks: number;
    days: number;
    hours: number;
    milliseconds: number;
    minutes: number;
    seconds: number;
    totalDays: number;
    totalHours: number;
    totalMilliseconds: number;
    totalMinutes: number;
    totalSeconds: number;
  },
  winPercent: number;
  averageScoreResult: {
    t: number;
    s: number;
    a: number;
    d: number;
    hd: number;
    siD: number;
    stD: number;
    md: number;
    cd: number;
    suD: number;
    tcCdEH: {
      ticks: number;
      days: number;
      hours: number;
      milliseconds: number;
      minutes: number;
      seconds: number;
      totalDays: number;
      totalHours: number;
      totalMilliseconds: number;
      totalMinutes: number;
      totalSeconds: number;
    },
    h: number;
    sh: number;
    dt: number;
    ec: number;
    tk: number;
    tsd: {
      ticks: number;
      days: number;
      hours: number;
      milliseconds: number;
      minutes: number;
      seconds: number;
      totalDays: number;
      totalHours: number;
      totalMilliseconds: number;
      totalMinutes: number;
      totalSeconds: number;
    },
    mcc: number;
    wtc: number;
    me: number;
  }
}

export interface SitewideCharacterStatistics {
  sitewideCharacterStatisticArray: SitewideCharacterStatistic[];
  dateTimeBegin: string | Date;
  dateTimeEnd: string | Date;
  league: number;
  gameMode: number;
  lastUpdated: string | Date;
}
