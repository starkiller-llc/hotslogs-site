import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentBracketRoundComponent } from './tournament-bracket-round.component';

describe('TournamentBracketRoundComponent', () => {
  let component: TournamentBracketRoundComponent;
  let fixture: ComponentFixture<TournamentBracketRoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TournamentBracketRoundComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TournamentBracketRoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
