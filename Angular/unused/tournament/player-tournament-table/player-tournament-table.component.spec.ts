import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerTournamentTableComponent } from './player-tournament-table.component';

describe('PlayerTournamentTableComponent', () => {
  let component: PlayerTournamentTableComponent;
  let fixture: ComponentFixture<PlayerTournamentTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PlayerTournamentTableComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PlayerTournamentTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
