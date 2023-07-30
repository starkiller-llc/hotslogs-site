import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { Stat } from '../model';

@Component({
  selector: 'app-awards-table',
  templateUrl: './awards-table.component.html',
  styleUrls: ['./awards-table.component.scss']
})
export class AwardsTableComponent implements OnInit {
  colIcons = {
    PercentMVP: '0MVP',
    PercentHighestKillStreak: '0Dominator',
    PercentMostKills: '0Finisher',
    PercentHatTrick: '0HatTrick',
    PercentMostHeroDamageDone: '0Painbringer',
    PercentMostStuns: '0Stunner',
    PercentMostRoots: '0Trapper',
    PercentMostSiegeDamageDone: '0SiegeMaster',
    PercentMostHealing: '0Healing',
    PercentClutchHealer: '0ClutchHealer',
    PercentMostProtection: '0Protector',
    PercentMostDamageTaken: '0Bulwark',
    PercentMostXPContribution: '0Experienced',
    PercentMostMercCampsCaptured: '0Headhunter',
    PercentZeroDeaths: '0SoleSurvivor',
    PercentMapSpecific: '0MasteroftheCurse',
    PercentMostDragonShrinesCaptured: '0Shriner',
    PercentMostCurseDamageDone: '0MasteroftheCurse',
    PercentMostCoinsPaid: '0Moneybags',
    PercentMostSkullsCollected: '0Moneybags',
    PercentMostDamageToPlants: '0GardenTerror',
    PercentMostTimeInTemple: '0TempleMaster',
    PercentMostGemsTurnedIn: '0Jeweler',
    PercentMostImmortalDamage: '0ImmortalSlayer',
    PercentMostDamageToMinions: '0GuardianSlayer',
    PercentMostAltarDamage: '0Cannoneer',
    PercentMostDamageDoneToZerg: '0ZergCrusher',
    PercentMostNukeDamageDone: '0DaBomb',
  };
  colTooltips = {
    PercentMVP: 'MVP',
    PercentHighestKillStreak: 'Highest Kill Streak',
    PercentMostKills: 'Most Kills',
    PercentHatTrick: 'First Several Kills of Match',
    PercentMostHeroDamageDone: 'Most Hero Damage Done',
    PercentMostStuns: 'Most Stuns',
    PercentMostRoots: 'Most Roots',
    PercentMostSiegeDamageDone: 'Most Siege Damage Done',
    PercentMostHealing: 'Most Healing',
    PercentClutchHealer: 'Most Clutch Heals',
    PercentMostProtection: 'Most Protection',
    PercentMostDamageTaken: 'Most Damage Taken',
    PercentMostXPContribution: 'Most XP Contribution',
    PercentMostMercCampsCaptured: 'Most Merc Camps Captured',
    PercentZeroDeaths: 'Zero Deaths',
    PercentMapSpecific: 'Map Specific',
    PercentMostDragonShrinesCaptured: 'Most Dragon Shrines Captured',
    PercentMostCurseDamageDone: 'Most Curse Damage Done',
    PercentMostCoinsPaid: 'Most Coins Paid',
    PercentMostSkullsCollected: 'Most Skulls Collected',
    PercentMostDamageToPlants: 'Most Damage To Plants',
    PercentMostTimeInTemple: 'Most Time In Temple',
    PercentMostGemsTurnedIn: 'Most Gems Turned In',
    PercentMostImmortalDamage: 'Most Immortal Damage',
    PercentMostDamageToMinions: 'Most Damage To Infernal Shrine Guardian Minions',
    PercentMostAltarDamage: 'Most Altar Damage',
    PercentMostDamageDoneToZerg: 'Most Damage Done To Zerg',
    PercentMostNukeDamageDone: 'Most Nuke Damage Done',
  };
  colsStandard = [
    'PercentMVP',
    'PercentHighestKillStreak',
    'PercentMostXPContribution',
    'PercentMostHeroDamageDone',
    'PercentMostSiegeDamageDone',
    'PercentMostDamageTaken',
    'PercentMostHealing',
    'PercentMostStuns',
    'PercentMostMercCampsCaptured',
    'PercentMapSpecific',
    'PercentMostKills',
    'PercentHatTrick',
    'PercentClutchHealer',
    'PercentMostProtection',
    'PercentZeroDeaths',
    'PercentMostRoots',
  ];
  colsMap = [
    'PercentMostDragonShrinesCaptured',
    'PercentMostCurseDamageDone',
    'PercentMostCoinsPaid',
    'PercentMostSkullsCollected',
    'PercentMostDamageToPlants',
    'PercentMostTimeInTemple',
    'PercentMostGemsTurnedIn',
    'PercentMostImmortalDamage',
    'PercentMostDamageToMinions',
    'PercentMostAltarDamage',
    'PercentMostDamageDoneToZerg',
    'PercentMostNukeDamageDone',
  ];
  genColumns = [
    'heroImg',
    'Character',
    'GamesPlayedTotal',
    'GamesPlayedWithAward',
  ];
  columns = this.genColumns;
  allCols = [...this.colsStandard, ...this.colsMap];

  @Input() stats: Stat[];
  @Input() standard = true;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.standard) {
      this.columns = [...this.genColumns, ...this.colsStandard];
    } else {
      this.columns = [...this.genColumns, ...this.colsMap];
    }
  }
}
