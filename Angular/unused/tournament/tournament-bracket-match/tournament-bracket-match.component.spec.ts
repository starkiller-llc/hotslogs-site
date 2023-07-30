import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentBracketMatchComponent } from './tournament-bracket-match.component';

describe('TournamentBracketMatchComponent', () => {
  let component: TournamentBracketMatchComponent;
  let fixture: ComponentFixture<TournamentBracketMatchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TournamentBracketMatchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TournamentBracketMatchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
