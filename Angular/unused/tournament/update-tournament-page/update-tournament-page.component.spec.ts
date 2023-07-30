import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateTournamentPageComponent } from './update-tournament-page.component';

describe('UpdateTournamentPageComponent', () => {
  let component: UpdateTournamentPageComponent;
  let fixture: ComponentFixture<UpdateTournamentPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UpdateTournamentPageComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateTournamentPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
