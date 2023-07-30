import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-talent-sprite',
  templateUrl: './talent-sprite.component.html',
  styleUrls: ['./talent-sprite.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TalentSpriteComponent implements OnInit {
  @Input() hero: string;
  @Input() talent: string;
  @Input() tooltip = '';

  constructor() { }

  ngOnInit(): void {
  }

}
