import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateTournamentPageComponent } from './create-tournament-page.component';

describe('CreateTournamentPageComponent', () => {
  let component: CreateTournamentPageComponent;
  let fixture: ComponentFixture<CreateTournamentPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateTournamentPageComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateTournamentPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
