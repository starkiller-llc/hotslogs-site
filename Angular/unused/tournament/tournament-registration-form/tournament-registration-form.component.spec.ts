import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentRegistrationFormComponent } from './tournament-registration-form.component';

describe('TournamentRegistrationFormComponent', () => {
  let component: TournamentRegistrationFormComponent;
  let fixture: ComponentFixture<TournamentRegistrationFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TournamentRegistrationFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TournamentRegistrationFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
