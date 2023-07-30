import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MigrationService } from '../../services/migration.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.scss']
})
export class LogoutComponent implements OnInit {

  constructor(svc: MigrationService, router: Router) {
    svc.logout().subscribe({
      complete: () => router.navigate(['/default']),
    });
  }

  ngOnInit(): void {
  }

}
