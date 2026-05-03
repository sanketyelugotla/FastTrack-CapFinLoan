import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApplicationService } from '../../../core/services/application.service';
import {
    WalletSummaryResponse,
    CreateTopUpOrderRequest,
    CreateTopUpOrderResponse,
    VerifyTopUpRequest,
    WalletConfigResponse
} from '../../../core/models/application.models';

declare var Razorpay: any;

@Component({
    selector: 'app-wallet',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    template: `
    <div class="wallet-dashboard fade-in">
      <div class="glass-header">
        <div class="header-content">
          <h1>My Wallet</h1>
          <p class="subtitle">Securely manage your funds and top up your account instantly</p>
        </div>
      </div>

      <!-- Top Row: Add Funds (left) + Fee Info (right) -->
      <div class="content-grid">
        <!-- Left Column: Balance & TopUp -->
        <div class="left-column">
          <!-- Balance Card -->
          <div class="glass-card balance-card">
            <div class="card-body">
              <h2 class="section-title">Current Balance</h2>
              <div class="balance-display">
                <span class="currency-symbol">₹</span>
                <span class="balance-amount">{{ walletSummary()?.balance | number: '1.2-2' }}</span>
              </div>
              <div class="balance-meta">
                <span class="badge badge-primary">{{ walletSummary()?.ownerType }} Wallet</span>
                <span class="last-updated">
                  Last active: {{ (walletSummary()?.recentEntries?.[0]?.createdAtUtc | date:'medium') || 'Never' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Top-up Form -->
          <div class="glass-card topup-card">
            <div class="card-header border-bottom">
              <div class="icon-circle"><i class="material-symbols-outlined">add_circle</i></div>
              <h3>Add Funds</h3>
            </div>
            <div class="card-body">
              <form [formGroup]="topupForm" (ngSubmit)="createOrder()">
                <div class="form-group">
                  <label for="amount">Top-up Amount</label>
                  <div class="input-wrapper">
                    <span class="input-prefix">₹</span>
                    <input
                      type="number"
                      id="amount"
                      formControlName="amount"
                      placeholder="Enter amount"
                      class="glass-input"
                    />
                  </div>
                  <small class="help-text">Min: ₹{{ walletConfig().minTopUpAmount }} | Max: ₹{{ walletConfig().maxTopUpAmount }}</small>
                </div>
                
                <button
                  type="submit"
                  class="btn-glass-primary full-width"
                  [disabled]="loading() || !topupForm.valid"
                >
                  <span class="material-symbols-outlined" *ngIf="!loading()">account_balance_wallet</span>
                  <span class="material-symbols-outlined rotating" *ngIf="loading()">autorenew</span>
                  {{ loading() ? 'Processing securely...' : 'Add Funds via Razorpay' }}
                </button>
              </form>
            </div>
          </div>
        </div>

        <!-- Right Column: Fee Info -->
        <div class="right-column">
          <div class="glass-card fee-info-card">
            <div class="card-header border-bottom">
              <div class="icon-circle"><span class="material-symbols-outlined">info</span></div>
              <h3>Wallet & Fee Info</h3>
            </div>
            <div class="card-body">
              <div class="fee-row highlight-row">
                <span class="fee-label">Application Submission Fee</span>
                <strong class="fee-value">₹{{ walletConfig().applicationFee }}</strong>
              </div>
              <p class="fee-desc">This amount is automatically deducted from your wallet when you submit a new loan application. Ensure your wallet has sufficient balance before submitting.</p>
              <div class="fee-row">
                <span class="fee-label">Min Top-up Amount</span>
                <strong class="fee-value-neutral">₹{{ walletConfig().minTopUpAmount }}</strong>
              </div>
              <div class="fee-row">
                <span class="fee-label">Max Top-up Amount</span>
                <strong class="fee-value-neutral">₹{{ walletConfig().maxTopUpAmount }}</strong>
              </div>
              <div class="fee-row">
                <span class="fee-label">Currency</span>
                <strong class="fee-value-neutral">{{ walletConfig().currency }}</strong>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Bottom Row: Transactions (full width) -->
      <div class="glass-card transactions-card mt-card">
        <div class="card-header border-bottom">
          <h3>Recent Transactions</h3>
        </div>
        
        <div class="card-body p-0">
          <div *ngIf="(walletSummary()?.recentEntries?.length ?? 0) > 0; else noTransactions" class="transaction-list">
            <div class="transaction-item" *ngFor="let entry of walletSummary()?.recentEntries ?? []">
              <div class="tx-icon" [ngClass]="entry.direction === 'Credit' ? 'tx-in' : 'tx-out'">
                <span class="material-symbols-outlined">
                  {{ entry.direction === 'Credit' ? 'arrow_downward' : 'arrow_upward' }}
                </span>
              </div>
              <div class="tx-details">
                <div class="tx-title">{{ entry.entryType }}</div>
                <div class="tx-remarks">{{ entry.remarks }}</div>
                <div class="tx-date">{{ entry.createdAtUtc | date: 'MMM d, y, h:mm a' }}</div>
              </div>
              <div class="tx-amount" [ngClass]="entry.direction === 'Credit' ? 'text-success' : 'text-danger'">
                {{ entry.direction === 'Credit' ? '+' : '-' }}₹{{ entry.amount | number: '1.2-2' }}
              </div>
            </div>
          </div>
          <ng-template #noTransactions>
            <div class="empty-state">
              <span class="material-symbols-outlined empty-icon">receipt_long</span>
              <p>No transactions found</p>
              <span class="text-muted">Your wallet activity will appear here</span>
            </div>
          </ng-template>
        </div>
      </div>

      <!-- Toast Notifications -->
      <div class="toast-container" *ngIf="errorMessage() || successMessage()">
        <div class="glass-toast error-toast" *ngIf="errorMessage()">
          <span class="material-symbols-outlined">error</span>
          <span>{{ errorMessage() }}</span>
        </div>
        <div class="glass-toast success-toast" *ngIf="successMessage()">
          <span class="material-symbols-outlined">check_circle</span>
          <span>{{ successMessage() }}</span>
        </div>
      </div>
    </div>
  `,
    styles: [`
    /* Light Theme Glassmorphic */
    :host {
      display: block;
      min-height: 100vh;
      background: linear-gradient(135deg, #f8f9fa 0%, #f0f2f5 100%);
      color: #191C1E;
      font-family: 'Inter', system-ui, -apple-system, sans-serif;
    }

    .wallet-dashboard {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1.5rem;
    }

    .fade-in {
      animation: fadeIn 0.6s ease-out;
    }

    /* Glass Panels */
    .glass-header {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(0, 0, 0, 0.08);
      border-radius: 16px;
      padding: 1.5rem;
      margin-bottom: 1.5rem;
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
    }

    .header-content h1 {
      margin: 0 0 0.5rem 0;
      font-size: 1.8rem;
      font-weight: 700;
      background: linear-gradient(to right, #001736, #265678);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .subtitle {
      margin: 0;
      color: #64748b;
      font-size: 0.95rem;
    }

    /* Grid Layout */
    .content-grid {
      display: grid;
      grid-template-columns: 1fr 1.5fr;
      gap: 1.5rem;
    }

    @media (max-width: 992px) {
      .content-grid {
        grid-template-columns: 1fr;
      }
    }

    .left-column {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .mt-card { margin-top: 1.5rem; }

    .glass-card {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(16px);
      -webkit-backdrop-filter: blur(16px);
      border: 1px solid rgba(0, 0, 0, 0.08);
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .glass-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
      border-color: rgba(0, 0, 0, 0.12);
    }

    .glow-effect {
      position: relative;
    }

    .glow-effect::before {
      content: '';
      position: absolute;
      top: -50%;
      left: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(96, 165, 250, 0.08) 0%, transparent 70%);
      pointer-events: none;
    }

    .card-header {
      padding: 1.25rem;
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .border-bottom {
      border-bottom: 1px solid rgba(0, 0, 0, 0.08);
    }

    .card-header h3 {
      margin: 0;
      font-size: 1.05rem;
      font-weight: 600;
      color: #191C1E;
    }

    .card-body {
      padding: 1.25rem;
    }
    
    .p-0 {
      padding: 0;
    }

    /* Balance Section */
    .section-title {
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 0.8px;
      color: #64748b;
      margin: 0 0 0.8rem 0;
    }

    .balance-display {
      display: flex;
      align-items: baseline;
      margin-bottom: 1.2rem;
    }

    .currency-symbol {
      font-size: 1.4rem;
      color: #001736;
      margin-right: 0.25rem;
    }

    .balance-amount {
      font-size: 2.2rem;
      font-weight: 800;
      color: #191C1E;
      letter-spacing: -1px;
    }

    .balance-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 0.8rem;
      border-top: 1px solid rgba(0, 0, 0, 0.08);
    }

    .badge {
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
      font-size: 0.7rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .badge-primary {
      background: rgba(0, 23, 54, 0.1);
      color: #001736;
      border: 1px solid rgba(0, 23, 54, 0.2);
    }

    .last-updated {
      font-size: 0.75rem;
      color: #94a3b8;
    }

    /* Form Controls */
    .icon-circle {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: rgba(0, 23, 54, 0.1);
      color: #001736;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .form-group {
      margin-bottom: 1.2rem;
    }

    .form-group label {
      display: block;
      margin-bottom: 0.6rem;
      color: #334155;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-prefix {
      position: absolute;
      left: 1rem;
      color: #64748b;
      font-size: 1.1rem;
      font-weight: 500;
    }

    .glass-input {
      width: 100%;
      padding: 0.8rem 0.8rem 0.8rem 2.3rem;
      background: #f8f9fa;
      border: 1px solid rgba(0, 0, 0, 0.1);
      border-radius: 12px;
      color: #191C1E;
      font-size: 0.95rem;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .glass-input:focus {
      outline: none;
      border-color: #001736;
      background: #ffffff;
      box-shadow: 0 0 0 3px rgba(0, 23, 54, 0.1);
    }

    .help-text {
      display: block;
      margin-top: 0.4rem;
      color: #94a3b8;
      font-size: 0.75rem;
    }

    /* Buttons */
    .btn-glass-primary {
      background: linear-gradient(135deg, #001736 0%, #264778 100%);
      color: white;
      border: none;
      padding: 0.8rem 1.2rem;
      border-radius: 12px;
      font-size: 0.95rem;
      font-weight: 600;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.75rem;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(0, 23, 54, 0.2);
    }

    .btn-glass-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0, 23, 54, 0.3);
      background: linear-gradient(135deg, #264778 0%, #264778 100%);
    }

    .btn-glass-primary:disabled {
      background: #cbd5e1;
      color: #94a3b8;
      cursor: not-allowed;
      box-shadow: none;
    }

    .full-width {
      width: 100%;
    }

    .rotating {
      animation: spin 1s linear infinite;
    }

    /* Fee Card */
    .fee-info-card .card-body { padding: 1.25rem; }

    .fee-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.6rem 0;
      border-bottom: 1px solid rgba(0, 0, 0, 0.05);
    }

    .fee-row:last-of-type { border-bottom: none; }

    .highlight-row {
      background: rgba(0, 23, 54, 0.04);
      border-radius: 8px;
      padding: 0.75rem;
      margin-bottom: 0.5rem;
      border-bottom: none;
    }

    .fee-label { font-size: 0.85rem; color: #64748b; }
    .fee-value { font-size: 0.9rem; color: #001736; font-weight: 700; }
    .fee-value-neutral { font-size: 0.9rem; color: #334155; }

    .fee-desc {
      font-size: 0.8rem;
      color: #94a3b8;
      margin: 0.5rem 0 1rem;
      line-height: 1.5;
    }

    /* Transactions */
    .transaction-list {
      display: flex;
      flex-direction: column;
    }

    .transaction-item {
      display: flex;
      align-items: center;
      padding: 1rem 1.25rem;
      border-bottom: 1px solid rgba(0, 0, 0, 0.06);
      transition: background 0.2s;
    }

    .transaction-item:hover {
      background: rgba(0, 0, 0, 0.02);
    }

    .transaction-item:last-child {
      border-bottom: none;
    }

    .tx-icon {
      width: 44px;
      height: 44px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 1rem;
    }

    .tx-in { background: rgba(34, 197, 94, 0.1); color: #15803d; }
    .tx-out { background: rgba(239, 68, 68, 0.1); color: #991b1b; }

    .tx-details {
      flex: 1;
    }

    .tx-title {
      font-weight: 600;
      color: #191C1E;
      margin-bottom: 0.2rem;
      font-size: 0.95rem;
    }

    .tx-remarks {
      font-size: 0.8rem;
      color: #64748b;
      margin-bottom: 0.2rem;
    }

    .tx-date {
      font-size: 0.7rem;
      color: #94a3b8;
    }

    .tx-amount {
      font-weight: 700;
      font-size: 1rem;
      letter-spacing: 0.5px;
      color: #191C1E;
    }

    .text-success { color: #15803d; }
    .text-danger { color: #991b1b; }

    .empty-state {
      padding: 3rem 2rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }

    .empty-icon {
      font-size: 3rem;
      color: #cbd5e1;
      margin-bottom: 1rem;
    }

    .empty-state p {
      color: #64748b;
      font-size: 1rem;
      font-weight: 500;
      margin: 0 0 0.5rem 0;
    }

    /* Toasts */
    .toast-container {
      position: fixed;
      bottom: 2rem;
      right: 2rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      z-index: 1000;
    }

    .glass-toast {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1rem 1.5rem;
      border-radius: 12px;
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
      font-weight: 500;
      font-size: 0.95rem;
      animation: slideIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    }

    .error-toast {
      background: rgba(239, 68, 68, 0.15);
      border: 1px solid rgba(239, 68, 68, 0.3);
      color: #991b1b;
    }

    .success-toast {
      background: rgba(34, 197, 94, 0.15);
      border: 1px solid rgba(34, 197, 94, 0.3);
      color: #15803d;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    @keyframes slideIn {
      from { opacity: 0; transform: translateX(50px); }
      to { opacity: 1; transform: translateX(0); }
    }

    @keyframes spin {
      100% { transform: rotate(360deg); }
    }
  `]
})
export class WalletComponent implements OnInit {
    private applicationService = inject(ApplicationService);
    private fb = inject(FormBuilder);

    walletSummary = signal<WalletSummaryResponse | null>(null);
    walletConfig = signal<WalletConfigResponse>({
        applicationFee: 500,
        minTopUpAmount: 100,
        maxTopUpAmount: 500000,
        currency: 'INR'
    });

    topupForm: FormGroup;
    loading = signal(false);
    errorMessage = signal('');
    successMessage = signal('');

    constructor() {
        this.topupForm = this.fb.group({
            amount: ['', [Validators.required, Validators.min(100), Validators.max(500000)]]
        });
    }

    ngOnInit() {
        this.loadWalletData();
    }

    loadWalletData() {
        this.applicationService.getWalletSummary().subscribe({
            next: (summary) => {
                this.walletSummary.set(summary);
            },
            error: (err) => {
                this.errorMessage.set('Failed to load wallet data');
                console.error(err);
                setTimeout(() => this.errorMessage.set(''), 5000);
            }
        });

        this.applicationService.getWalletConfig().subscribe({
            next: (config) => {
                this.walletConfig.set(config);
                this.topupForm.get('amount')?.setValidators([
                    Validators.required,
                    Validators.min(config.minTopUpAmount),
                    Validators.max(config.maxTopUpAmount)
                ]);
                this.topupForm.get('amount')?.updateValueAndValidity();
            },
            error: (err) => console.error(err)
        });
    }

    createOrder() {
        if (!this.topupForm.valid) {
            this.errorMessage.set('Please enter a valid amount');
            setTimeout(() => this.errorMessage.set(''), 3000);
            return;
        }

        this.loading.set(true);
        this.errorMessage.set('');
        this.successMessage.set('');

        const request: CreateTopUpOrderRequest = {
            amount: this.topupForm.get('amount')?.value,
            currency: 'INR'
        };

        this.applicationService.createTopUpOrder(request).subscribe({
            next: (response: CreateTopUpOrderResponse) => {
                this.handleRazorpayPayment(response);
            },
            error: (err) => {
                this.loading.set(false);
                this.errorMessage.set(err.error?.message || 'Failed to create payment order');
                console.error(err);
                setTimeout(() => this.errorMessage.set(''), 5000);
            }
        });
    }

    private handleRazorpayPayment(orderResponse: CreateTopUpOrderResponse) {
        const options = {
            key: orderResponse.keyId,
            amount: orderResponse.amount * 100,
            currency: orderResponse.currency,
            name: 'CapFinLoan',
            description: 'Wallet Top-up',
            order_id: orderResponse.providerOrderId,
            handler: (response: any) => {
                this.verifyPayment(response);
            },
            prefill: {
                name: 'CapFinLoan User',
                contact: '9999999999'
            },
            theme: {
                color: '#3b82f6'
            },
            modal: {
                ondismiss: () => {
                    this.loading.set(false);
                    this.errorMessage.set('Payment cancelled');
                    setTimeout(() => this.errorMessage.set(''), 3000);
                }
            }
        };

        const razorpay = new Razorpay(options);
        razorpay.open();
    }

    private verifyPayment(paymentResponse: any) {
        const verifyRequest: VerifyTopUpRequest = {
            providerOrderId: paymentResponse.razorpay_order_id,
            providerPaymentId: paymentResponse.razorpay_payment_id,
            providerSignature: paymentResponse.razorpay_signature
        };

        this.applicationService.verifyTopUp(verifyRequest).subscribe({
            next: (response) => {
                this.loading.set(false);
                this.successMessage.set('✓ Top-up successful! Your wallet has been credited.');
                this.walletSummary.set(response.wallet);
                this.topupForm.reset();
                setTimeout(() => this.successMessage.set(''), 5000);
            },
            error: (err) => {
                this.loading.set(false);
                this.errorMessage.set(err.error?.message || 'Payment verification failed');
                console.error(err);
                setTimeout(() => this.errorMessage.set(''), 5000);
            }
        });
    }
}
