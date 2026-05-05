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
  selector: 'app-admin-wallet',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="wallet-page">
      <!-- Header -->
      <div class="wallet-header">
        <div>
          <h1 class="wallet-title">Platform Wallet</h1>
          <p class="wallet-sub">Manage platform funds and application fee collection.</p>
        </div>
      </div>

      <!-- Main row: Balance + Actions -->
      <div class="main-row">
        <!-- Balance Card -->
        <div class="balance-card">
          <div class="bal-label">PLATFORM BALANCE</div>
          <div class="bal-amount">
            <span class="bal-currency">&#8377;</span>
            <span class="bal-number">{{ walletSummary()?.balance | number:'1.0-0' }}</span>
          </div>
          <div class="bal-meta">
            <span class="bal-type">Master Wallet</span>
            <span class="bal-cur">{{ walletSummary()?.currency || 'INR' }}</span>
          </div>
          <div class="bal-last">
            Last activity: {{ (walletSummary()?.recentEntries?.[0]?.createdAtUtc | date:'medium') || 'Never' }}
          </div>
        </div>

        <!-- Action Cards -->
        <div class="action-col">
          <button class="action-card action-add" (click)="showAddMoney.set(true)">
            <div class="action-icon-wrap add-icon-wrap">
              <span class="material-symbols-outlined">add_card</span>
            </div>
            <div class="action-text">
              <span class="action-title">Inject Funds</span>
              <span class="action-sub">via Razorpay</span>
            </div>
          </button>

          <div class="info-card">
            <div class="info-row">
              <span class="info-label">Application Fee</span>
              <span class="info-value">&#8377;{{ walletConfig().applicationFee }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Min / Max Top-up</span>
              <span class="info-value">&#8377;{{ walletConfig().minTopUpAmount }} / &#8377;{{ walletConfig().maxTopUpAmount }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Currency</span>
              <span class="info-value">{{ walletConfig().currency }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Transactions -->
      <div class="tx-section">
        <div class="tx-header">
          <h3 class="tx-title">Recent Platform Transactions</h3>
        </div>
        <div *ngIf="(walletSummary()?.recentEntries?.length ?? 0) > 0; else noTx" class="tx-list">
          <div class="tx-row" *ngFor="let e of walletSummary()?.recentEntries ?? []">
            <div class="tx-dot" [class.tx-credit]="e.direction === 'Credit'" [class.tx-debit]="e.direction !== 'Credit'">
              <span class="material-symbols-outlined">{{ e.direction === 'Credit' ? 'add' : 'remove' }}</span>
            </div>
            <div class="tx-info">
              <div class="tx-type">{{ e.entryType }}</div>
              <div class="tx-remark">{{ e.remarks }}</div>
              <div class="tx-date">{{ e.createdAtUtc | date:'MMM d, y · h:mm a' }}</div>
            </div>
            <div class="tx-amt" [class.credit]="e.direction === 'Credit'" [class.debit]="e.direction !== 'Credit'">
              {{ e.direction === 'Credit' ? '+' : '-' }}&#8377;{{ e.amount | number:'1.0-0' }}
            </div>
          </div>
        </div>
        <ng-template #noTx>
          <div class="no-tx">
            <span class="material-symbols-outlined no-tx-icon">receipt_long</span>
            <p>No transactions yet.</p>
          </div>
        </ng-template>
      </div>

      <!-- Add Money Overlay -->
      <div class="overlay" *ngIf="showAddMoney()" (click)="showAddMoney.set(false)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="modal-icon add-modal-icon">
              <span class="material-symbols-outlined">add_card</span>
            </div>
            <div>
              <div class="modal-title">Inject Platform Funds</div>
              <div class="modal-sub">via Razorpay — secure payment</div>
            </div>
            <button class="modal-close" (click)="showAddMoney.set(false)">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
          <form [formGroup]="topupForm" (ngSubmit)="createOrder()">
            <div class="field-group">
              <label>Amount</label>
              <div class="input-row">
                <span class="prefix">&#8377;</span>
                <input type="number" formControlName="amount" placeholder="Enter amount" class="modal-input" />
              </div>
              <small>Min: &#8377;{{ walletConfig().minTopUpAmount }} &nbsp;|&nbsp; Max: &#8377;{{ walletConfig().maxTopUpAmount }}</small>
            </div>
            <button type="submit" class="modal-btn add-btn" [disabled]="loading() || !topupForm.valid">
              <span class="material-symbols-outlined" *ngIf="!loading()">account_balance_wallet</span>
              <span class="material-symbols-outlined spin" *ngIf="loading()">autorenew</span>
              {{ loading() ? 'Processing...' : 'Pay via Razorpay' }}
            </button>
          </form>
        </div>
      </div>

      <!-- Toasts -->
      <div class="toast-wrap" *ngIf="errorMessage() || successMessage()">
        <div class="toast error-toast" *ngIf="errorMessage()">
          <span class="material-symbols-outlined">error</span> {{ errorMessage() }}
        </div>
        <div class="toast success-toast" *ngIf="successMessage()">
          <span class="material-symbols-outlined">check_circle</span> {{ successMessage() }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .wallet-page { max-width: 1000px; margin: 0 auto; padding: 2rem 1.5rem; font-family: 'Inter', system-ui, sans-serif; }

    .wallet-header { margin-bottom: 1.75rem; }
    .wallet-title { font-size: 1.9rem; font-weight: 700; color: #92400e; margin: 0; }
    .wallet-sub { color: #64748b; font-size: 0.9rem; margin: 0.25rem 0 0; }

    .main-row { display: grid; grid-template-columns: 1fr 260px; gap: 1.25rem; margin-bottom: 1.25rem; }
    @media (max-width: 640px) { .main-row { grid-template-columns: 1fr; } }

    /* Balance */
    .balance-card {
      background: linear-gradient(135deg, #78350f 0%, #d97706 100%);
      border-radius: 20px; padding: 2rem; color: white; display: flex;
      flex-direction: column; gap: 0.4rem; min-height: 180px;
    }
    .bal-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.12em; opacity: 0.55; text-transform: uppercase; }
    .bal-amount { display: flex; align-items: flex-end; gap: 0.25rem; margin: 0.5rem 0 0.25rem; }
    .bal-currency { font-size: 2rem; font-weight: 400; opacity: 0.7; align-self: flex-start; margin-top: 0.4rem; }
    .bal-number { font-size: 3.5rem; font-weight: 700; line-height: 1; }
    .bal-meta { display: flex; gap: 0.75rem; align-items: center; margin-top: 0.5rem; }
    .bal-type { font-size: 0.75rem; background: rgba(255,255,255,0.15); border-radius: 20px; padding: 0.25rem 0.75rem; }
    .bal-cur { font-size: 0.75rem; opacity: 0.5; }
    .bal-last { font-size: 0.72rem; opacity: 0.5; margin-top: auto; }

    /* Action column */
    .action-col { display: flex; flex-direction: column; gap: 1rem; }

    .action-card {
      display: flex; align-items: center; gap: 1rem;
      border-radius: 16px; padding: 1.25rem 1.5rem;
      border: none; cursor: pointer; text-align: left;
      transition: transform 0.15s, box-shadow 0.15s;
    }
    .action-card:hover { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(0,0,0,0.12); }
    .action-add { background: #78350f; color: white; }

    .action-icon-wrap { width: 42px; height: 42px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
    .add-icon-wrap { background: rgba(255,255,255,0.15); color: white; }

    .action-text { display: flex; flex-direction: column; gap: 0.1rem; }
    .action-title { font-size: 1rem; font-weight: 600; }
    .action-sub { font-size: 0.75rem; opacity: 0.6; }

    /* Info card */
    .info-card {
      background: white; border: 1px solid #e2e8f0;
      border-radius: 16px; padding: 1rem 1.25rem;
      display: flex; flex-direction: column; gap: 0.6rem; flex: 1;
    }
    .info-row { display: flex; justify-content: space-between; align-items: center; }
    .info-label { font-size: 0.78rem; color: #94a3b8; }
    .info-value { font-size: 0.82rem; font-weight: 600; color: #374151; }

    /* Transactions */
    .tx-section { background: white; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; }
    .tx-header { padding: 1rem 1.5rem; border-bottom: 1px solid #f1f5f9; }
    .tx-title { font-size: 0.95rem; font-weight: 700; color: #1e293b; margin: 0; }
    .tx-list { }
    .tx-row { display: flex; align-items: center; gap: 1rem; padding: 0.9rem 1.5rem; border-bottom: 1px solid #f8fafc; }
    .tx-row:last-child { border-bottom: none; }
    .tx-row:hover { background: #f8fafc; }
    .tx-dot { width: 34px; height: 34px; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .tx-credit { background: #dcfce7; color: #16a34a; }
    .tx-debit { background: #fee2e2; color: #dc2626; }
    .tx-info { flex: 1; }
    .tx-type { font-size: 0.85rem; font-weight: 600; color: #1e293b; }
    .tx-remark { font-size: 0.75rem; color: #94a3b8; }
    .tx-date { font-size: 0.72rem; color: #cbd5e1; }
    .tx-amt { font-size: 0.9rem; font-weight: 700; white-space: nowrap; }
    .credit { color: #16a34a; }
    .debit { color: #dc2626; }
    .no-tx { padding: 2.5rem; text-align: center; color: #94a3b8; }
    .no-tx-icon { font-size: 2.5rem; display: block; margin-bottom: 0.5rem; }

    /* Overlay */
    .overlay { position: fixed; inset: 0; z-index: 1000; background: rgba(0,0,0,0.5); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; animation: fadeO 0.2s ease; }
    @keyframes fadeO { from { opacity: 0; } to { opacity: 1; } }
    .modal { background: white; border-radius: 20px; padding: 2rem; width: 100%; max-width: 420px; margin: 1rem; box-shadow: 0 20px 60px rgba(0,0,0,0.2); animation: slideM 0.25s cubic-bezier(0.34,1.56,0.64,1); }
    @keyframes slideM { from { opacity: 0; transform: scale(0.92) translateY(20px); } to { opacity: 1; transform: scale(1) translateY(0); } }
    .modal-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.75rem; }
    .modal-icon { width: 46px; height: 46px; border-radius: 14px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .add-modal-icon { background: #78350f; color: white; }
    .modal-title { font-size: 1.1rem; font-weight: 700; color: #0f172a; }
    .modal-sub { font-size: 0.8rem; color: #94a3b8; }
    .modal-close { margin-left: auto; background: #f1f5f9; border: none; border-radius: 50%; width: 34px; height: 34px; display: flex; align-items: center; justify-content: center; cursor: pointer; color: #64748b; }
    .modal-close:hover { background: #e2e8f0; }
    .field-group { margin-bottom: 1.25rem; }
    .field-group label { display: block; font-size: 0.825rem; font-weight: 600; color: #374151; margin-bottom: 0.45rem; }
    .input-row { display: flex; align-items: center; border: 1.5px solid #e2e8f0; border-radius: 12px; overflow: hidden; background: #f8fafc; }
    .input-row:focus-within { border-color: #d97706; background: white; }
    .prefix { padding: 0 0.75rem; color: #64748b; font-size: 1rem; }
    .modal-input { flex: 1; border: none; background: transparent; padding: 0.75rem 0.75rem 0.75rem 0; font-size: 1rem; color: #0f172a; outline: none; }
    .field-group small { display: block; margin-top: 0.35rem; font-size: 0.75rem; color: #94a3b8; }
    .modal-btn { width: 100%; padding: 0.85rem; border: none; border-radius: 12px; font-size: 0.95rem; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 0.6rem; transition: opacity 0.2s; margin-top: 0.5rem; }
    .modal-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .add-btn { background: #78350f; color: white; }
    .spin { animation: doSpin 1s linear infinite; }
    @keyframes doSpin { 100% { transform: rotate(360deg); } }

    /* Toasts */
    .toast-wrap { position: fixed; bottom: 2rem; right: 2rem; z-index: 2000; display: flex; flex-direction: column; gap: 0.75rem; }
    .toast { display: flex; align-items: center; gap: 0.75rem; padding: 0.875rem 1.25rem; border-radius: 12px; font-weight: 500; font-size: 0.9rem; box-shadow: 0 4px 16px rgba(0,0,0,0.12); animation: slideIn 0.3s ease; }
    @keyframes slideIn { from { opacity: 0; transform: translateX(50px); } to { opacity: 1; transform: translateX(0); } }
    .error-toast { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; }
    .success-toast { background: #f0fdf4; border: 1px solid #bbf7d0; color: #15803d; }
  `]
})
export class AdminWalletComponent implements OnInit {
  private applicationService = inject(ApplicationService);
  private fb = inject(FormBuilder);

  walletSummary = signal<WalletSummaryResponse | null>(null);
  walletConfig = signal<WalletConfigResponse>({ applicationFee: 500, minTopUpAmount: 100, maxTopUpAmount: 500000, currency: 'INR' });

  topupForm: FormGroup;
  loading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');
  showAddMoney = signal(false);

  constructor() {
    this.topupForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(100), Validators.max(500000)]]
    });
  }

  ngOnInit() { this.loadWalletData(); }

  loadWalletData() {
    this.applicationService.getAdminWalletSummary().subscribe({
      next: (s) => this.walletSummary.set(s),
      error: () => this.showError('Failed to load wallet data')
    });
    this.applicationService.getWalletConfig().subscribe({
      next: (config) => {
        this.walletConfig.set(config);
        this.topupForm.get('amount')?.setValidators([Validators.required, Validators.min(config.minTopUpAmount), Validators.max(config.maxTopUpAmount)]);
        this.topupForm.get('amount')?.updateValueAndValidity();
      },
      error: (err) => console.error(err)
    });
  }

  createOrder() {
    if (!this.topupForm.valid) return;
    this.loading.set(true);
    this.applicationService.createAdminTopUpOrder({ amount: this.topupForm.value.amount, currency: 'INR' }).subscribe({
      next: (r: CreateTopUpOrderResponse) => this.handleRazorpay(r),
      error: (err) => { this.loading.set(false); this.showError(err.error?.message || 'Failed to create order'); }
    });
  }

  private handleRazorpay(order: CreateTopUpOrderResponse) {
    new Razorpay({
      key: order.keyId, amount: order.amount * 100, currency: order.currency,
      name: 'CapFinLoan Platform', description: 'Platform Wallet Top-up',
      order_id: order.providerOrderId,
      handler: (r: any) => this.verifyPayment(r),
      theme: { color: '#d97706' },
      modal: { ondismiss: () => { this.loading.set(false); this.showError('Payment cancelled'); } }
    }).open();
  }

  private verifyPayment(p: any) {
    this.applicationService.verifyAdminTopUp({
      providerOrderId: p.razorpay_order_id,
      providerPaymentId: p.razorpay_payment_id,
      providerSignature: p.razorpay_signature
    }).subscribe({
      next: (r) => {
        this.loading.set(false);
        this.walletSummary.set(r.wallet);
        this.topupForm.reset();
        this.showAddMoney.set(false);
        this.showSuccess('Platform wallet credited successfully!');
      },
      error: (err) => { this.loading.set(false); this.showError(err.error?.message || 'Verification failed'); }
    });
  }

  private showError(msg: string) { this.errorMessage.set(msg); setTimeout(() => this.errorMessage.set(''), 5000); }
  private showSuccess(msg: string) { this.successMessage.set(msg); setTimeout(() => this.successMessage.set(''), 5000); }
}
