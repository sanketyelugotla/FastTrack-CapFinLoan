import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { WalletSummaryResponse } from '../../../core/models/application.models';

@Component({
    selector: 'app-admin-wallet-card',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="wallet-card">
      <div class="card-header">
        <h3>💰 Platform Wallet</h3>
        <button (click)="refreshWallet()" class="refresh-btn" [disabled]="loading">
          {{ loading ? 'Loading...' : 'Refresh' }}
        </button>
      </div>

      <div *ngIf="wallet; else loading" class="card-content">
        <div class="balance-display">
          <div class="balance-info">
            <span class="label">Total Balance</span>
            <div class="amount">₹{{ wallet.balance | number: '1.2-2' }}</div>
            <span class="currency">{{ wallet.currency }}</span>
          </div>
          <div class="stats">
            <div class="stat-item">
              <span class="stat-label">Owner Type</span>
              <span class="stat-value">{{ wallet.ownerType }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">Recent Transactions</span>
              <span class="stat-value">{{ wallet.recentEntries.length }}</span>
            </div>
          </div>
        </div>

        <div class="recent-transactions" *ngIf="wallet.recentEntries && wallet.recentEntries.length > 0">
          <h4>Recent Activity</h4>
          <div class="transaction-list">
            <div *ngFor="let entry of wallet.recentEntries.slice(0, 5)" class="transaction-row">
              <div class="transaction-left">
                <span class="type-badge" [class.credit]="entry.direction === 'Credit'" [class.debit]="entry.direction === 'Debit'">
                  {{ entry.entryType }}
                </span>
                <div class="details">
                  <p class="remarks">{{ entry.remarks }}</p>
                  <p class="time">{{ entry.createdAtUtc | date: 'short' }}</p>
                </div>
              </div>
              <span class="amount" [class.credit]="entry.direction === 'Credit'" [class.debit]="entry.direction === 'Debit'">
                {{ entry.direction === 'Credit' ? '+' : '-' }}₹{{ entry.amount | number: '1.2-2' }}
              </span>
            </div>
          </div>
        </div>

        <div *ngIf="errorMessage" class="alert alert-danger">
          {{ errorMessage }}
        </div>
      </div>

      <ng-template #loading>
        <div class="loading">Loading wallet data...</div>
      </ng-template>
    </div>
  `,
    styles: [`
    .wallet-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      overflow: hidden;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border-bottom: 1px solid #e0e0e0;
      background: #f8f9fa;
    }

    .card-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #1a1a1a;
    }

    .refresh-btn {
      padding: 6px 12px;
      background: #4CAF50;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 12px;
      cursor: pointer;
      transition: background 0.2s;
    }

    .refresh-btn:hover:not(:disabled) {
      background: #45a049;
    }

    .refresh-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .card-content {
      padding: 20px;
    }

    .balance-display {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
      margin-bottom: 20px;
    }

    .balance-info {
      display: flex;
      flex-direction: column;
      padding: 15px;
      background: linear-gradient(135deg, #4CAF50, #45a049);
      border-radius: 8px;
      color: white;
    }

    .label {
      font-size: 12px;
      opacity: 0.9;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 8px;
    }

    .amount {
      font-size: 32px;
      font-weight: bold;
      margin: 5px 0;
    }

    .currency {
      font-size: 12px;
      opacity: 0.8;
    }

    .stats {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px;
      background: #f0f0f0;
      border-radius: 4px;
    }

    .stat-label {
      font-size: 13px;
      color: #666;
      font-weight: 500;
    }

    .stat-value {
      font-size: 14px;
      font-weight: 600;
      color: #1a1a1a;
    }

    .recent-transactions {
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #e0e0e0;
    }

    .recent-transactions h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      color: #1a1a1a;
      font-weight: 600;
    }

    .transaction-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .transaction-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px;
      background: #f9f9f9;
      border-left: 3px solid #ddd;
      border-radius: 4px;
    }

    .transaction-left {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }

    .type-badge {
      padding: 4px 8px;
      border-radius: 3px;
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
      white-space: nowrap;
      min-width: 80px;
      text-align: center;
    }

    .type-badge.credit {
      background: #e8f5e9;
      color: #2e7d32;
    }

    .type-badge.debit {
      background: #ffebee;
      color: #c62828;
    }

    .details p {
      margin: 0;
      font-size: 12px;
    }

    .remarks {
      color: #333;
      font-weight: 500;
    }

    .time {
      color: #999;
      margin-top: 2px;
    }

    .amount {
      font-weight: 600;
      font-size: 13px;
    }

    .amount.credit {
      color: #2e7d32;
    }

    .amount.debit {
      color: #c62828;
    }

    .loading {
      padding: 40px 20px;
      text-align: center;
      color: #999;
      font-size: 14px;
    }

    .alert {
      padding: 12px;
      border-radius: 4px;
      font-size: 13px;
      margin-top: 15px;
    }

    .alert-danger {
      background: #ffebee;
      color: #c62828;
      border-left: 3px solid #f44336;
    }

    @media (max-width: 768px) {
      .balance-display {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AdminWalletCardComponent implements OnInit {
    private adminService = inject(AdminService);

    wallet: WalletSummaryResponse | null = null;
    errorMessage = '';
    loading = false;

    ngOnInit() {
        this.refreshWallet();
    }

    refreshWallet() {
        this.loading = true;
        this.errorMessage = '';
        this.adminService.getWalletSummary().subscribe({
            next: (data) => {
                this.wallet = data;
                this.loading = false;
            },
            error: (err) => {
                this.errorMessage = 'Failed to load wallet data';
                this.loading = false;
                console.error(err);
            }
        });
    }
}
