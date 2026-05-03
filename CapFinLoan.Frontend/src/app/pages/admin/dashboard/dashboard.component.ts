import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { AdminDashboardResponse } from '../../../core/models/admin.models';
import { AdminWalletCardComponent } from '../wallet-card/admin-wallet-card.component';

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, AdminWalletCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  data = signal<AdminDashboardResponse | null>(null);

  ngOnInit() {
    this.adminService.getDashboard().subscribe(d => this.data.set(d));
  }
}
