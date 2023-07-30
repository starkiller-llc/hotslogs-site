import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { TalentBuildComponent } from './talent-build.component';

describe('TalentBuildComponent', () => {
  let component: TalentBuildComponent;
  let fixture: ComponentFixture<TalentBuildComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TalentBuildComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TalentBuildComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
