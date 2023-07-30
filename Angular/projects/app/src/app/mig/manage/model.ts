export interface PlayerProfileSlim {
  Id: number;
  Name: string;
  Region: number;
  BattleTag?: number;
}

export interface PayPalOptions {
  User: string;
  MonthlyPlan: string;
  YearlyPlan: string;
}

export interface AccountData {
  Main: PlayerProfileSlim;
  Alts: PlayerProfileSlim[];
  SubscriptionId?: string;
  PayPal: PayPalOptions;
  Regions: number[];
}
