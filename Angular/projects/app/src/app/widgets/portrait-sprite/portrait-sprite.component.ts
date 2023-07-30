import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-portrait-sprite',
  templateUrl: './portrait-sprite.component.html',
  styleUrls: ['./portrait-sprite.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PortraitSpriteComponent implements OnInit {
  @Input() hero: string;
  @Input() tooltip = '';
  @Input() center = true;
  @Input() scale = 1;
  
  constructor() { }

  ngOnInit(): void {
  }

}
