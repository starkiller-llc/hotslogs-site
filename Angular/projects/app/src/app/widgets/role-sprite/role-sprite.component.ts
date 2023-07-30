import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-role-sprite',
  templateUrl: './role-sprite.component.html',
  styleUrls: ['./role-sprite.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleSpriteComponent implements OnInit {
  @Input() role: string;
  @Input() tooltip = '';

  constructor() { }

  ngOnInit(): void {
  }

}
