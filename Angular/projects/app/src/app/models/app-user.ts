export interface AppUser {
  id: number;
  email: string;
  isAdmin: boolean;
  isBnetAuthorized: boolean;
  premiumExpiration?: Date;
  isPremium: boolean;
  supporterSince?: Date;
  mainPlayerId?: number;
  username: string;
  region: number;
  defaultGameMode: number;
  isOptOut: boolean;
}
