import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-award-sprite',
  templateUrl: './award-sprite.component.html',
  styleUrls: ['./award-sprite.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AwardSpriteComponent implements OnInit {
  @Input() award: string;
  @Input() tooltip = '';

  constructor() { }

  ngOnInit(): void {
  }

}
