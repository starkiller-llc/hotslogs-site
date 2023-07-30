import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-role-buttons',
  templateUrl: './role-buttons.component.html',
  styleUrls: ['./role-buttons.component.scss']
})
export class RoleButtonsComponent implements OnInit {
  roles = [
    { Key: "Melee Assassin", Value: "#f78c6b" },
    { Key: "Ranged Assassin", Value: "#ef476f" },
    { Key: "Support", Value: "#073b4c" },
    { Key: "Healer", Value: "#118ab2" },
    { Key: "Bruiser", Value: "#06d6a0" },
    { Key: "Tank", Value: "#ffd166" }
  ];
  @Output() roleClick = new EventEmitter<string>();

  constructor() { }

  ngOnInit(): void {
  }

}
