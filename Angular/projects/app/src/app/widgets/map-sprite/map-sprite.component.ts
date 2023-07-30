import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-map-sprite',
  templateUrl: './map-sprite.component.html',
  styleUrls: ['./map-sprite.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapSpriteComponent implements OnInit {
  @Input() map: string;

  constructor() { }

  ngOnInit(): void {
  }

}
