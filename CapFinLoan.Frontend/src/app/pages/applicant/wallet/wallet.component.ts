import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationService } from '../../../core/services/application.service';
import {
    WalletSummaryResponse,
    CreateTopUpOrderRequest,
    CreateTopUpOrderResponse,
    VerifyTopUpRequest,
    WalletConfigResponse,
    WithdrawRequest
} from '../../../core/models/application.models';

declare var Razorpay: any;

@Component({
    selector: 'app-wallet',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="wallet-page">
      <!-- Header -->
      <div class="wallet-header">
        <div>
          <h1 class="wallet-title">My Wallet</h1>
          <p class="wallet-sub">Your money, all in one place.</p>
        </div>
      </div>

      <!-- Main row: Balance + Actions -->
      <div class="main-row">
        <!-- Balance Card -->
        <div class="balance-card">
          <div class="bal-label">AVAILABLE BALANCE</div>
          <div class="bal-amount">
            <span class="bal-currency">&#8377;</span>
            <span class="bal-number">{{ walletSummary()?.balance | number:'1.0-0' }}</span>
          </div>
          <div class="bal-meta">
            <span class="bal-type">{{ walletSummary()?.ownerType || 'Applicant' }} Wallet</span>
            <span class="bal-currency-tag">{{ walletSummary()?.currency || 'INR' }}</span>
          </div>
        </div>

        <!-- Action Cards -->
        <div class="action-col">
          <!-- Add Money -->
          <button class="action-card action-add" (click)="openAddMoneyModal()">
            <div class="action-icon-wrap add-icon-wrap">
              <span class="material-symbols-outlined">credit_card</span>
            </div>
            <div class="action-text">
              <span class="action-title">Add Money</span>
              <span class="action-sub">via Razorpay</span>
            </div>
          </button>

          <!-- Withdraw -->
          <button class="action-card action-withdraw" (click)="openWithdrawModal()">
            <div class="action-icon-wrap withdraw-icon-wrap">
              <span class="material-symbols-outlined">account_balance</span>
            </div>
            <div class="action-text">
              <span class="action-title">Withdraw</span>
              <span class="action-sub">To bank account</span>
            </div>
          </button>
        </div>
      </div>

      <!-- Fee Info Banner -->
      <div class="fee-banner">
        <div class="fee-banner-left">
          <span class="material-symbols-outlined fee-icon">currency_rupee</span>
          <div>
            <div class="fee-title">Need a loan?</div>
            <div class="fee-sub">&#8377;{{ walletConfig().applicationFee }} application fee will be deducted from your wallet.</div>
          </div>
        </div>
        <a routerLink="/applicant/apply" class="apply-btn">Apply for Loan &#8594;</a>
      </div>

      <!-- Transactions -->
      <div class="tx-section">
        <div class="tx-tabs">
          <button class="tx-tab" [class.active]="txFilter() === 'all'" (click)="txFilter.set('all')">All Transactions</button>
          <button class="tx-tab" [class.active]="txFilter() === 'debit'" (click)="txFilter.set('debit')">Withdrawals</button>
        </div>

        <div *ngIf="filteredEntries().length > 0; else noTx" class="tx-list">
          <div class="tx-row" *ngFor="let e of filteredEntries()">
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

      <!-- ===== Add Money Overlay ===== -->
      <div class="overlay" *ngIf="showAddMoney()" (click)="closeModals()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="modal-icon add-modal-icon">
              <span class="material-symbols-outlined">credit_card</span>
            </div>
            <div>
              <div class="modal-title">Add Money</div>
              <div class="modal-sub">via Razorpay — secure payment</div>
            </div>
            <button class="modal-close" (click)="closeModals()">
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

      <!-- ===== Withdraw Overlay ===== -->
      <div class="overlay" *ngIf="showWithdraw()" (click)="closeModals()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <div class="modal-icon withdraw-modal-icon">
              <span class="material-symbols-outlined">account_balance</span>
            </div>
            <div>
              <div class="modal-title">Withdraw Funds</div>
              <div class="modal-sub">Available: &#8377;{{ walletSummary()?.balance | number:'1.2-2' }}</div>
            </div>
            <button class="modal-close" (click)="closeModals()">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
          <form [formGroup]="withdrawForm" (ngSubmit)="submitWithdraw()">
            <div class="field-group">
              <label>Amount</label>
              <div class="input-row">
                <span class="prefix">&#8377;</span>
                <input type="number" formControlName="amount" placeholder="Enter amount" class="modal-input" />
              </div>
            </div>
            <div class="field-group">
              <label>Remarks <span class="optional">(optional)</span></label>
              <input type="text" formControlName="remarks" placeholder="Purpose of withdrawal" class="modal-input plain" />
            </div>
            <button type="submit" class="modal-btn withdraw-btn" [disabled]="withdrawLoading() || !withdrawForm.valid">
              <span class="material-symbols-outlined" *ngIf="!withdrawLoading()">south_west</span>
              <span class="material-symbols-outlined spin" *ngIf="withdrawLoading()">autorenew</span>
              {{ withdrawLoading() ? 'Processing...' : 'Withdraw Funds' }}
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

    .wallet-page {
      max-width: 1000px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
      font-family: 'Inter', system-ui, sans-serif;
    }

    /* Header */
    .wallet-header { margin-bottom: 1.75rem; }
    .wallet-title { font-size: 1.9rem; font-weight: 700; color: var(--color-primary, #001736); margin: 0; }
    .wallet-sub { color: #64748b; font-size: 0.9rem; margin: 0.25rem 0 0; }

    /* Main row */
    .main-row {
      display: grid;
      grid-template-columns: 1fr 260px;
      gap: 1.25rem;
      margin-bottom: 1.25rem;
    }
    @media (max-width: 640px) { .main-row { grid-template-columns: 1fr; } }

    /* Balance Card */
    .balance-card {
      background: linear-gradient(135deg, #0f172a 0%, #1e3a5f 100%);
      border-radius: 20px;
      padding: 2rem 2rem 1.75rem;
      color: white;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      min-height: 180px;
    }
    .bal-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.12em; opacity: 0.55; text-transform: uppercase; }
    .bal-amount { display: flex; align-items: flex-end; gap: 0.25rem; margin: 0.5rem 0 0.75rem; }
    .bal-currency { font-size: 2rem; font-weight: 400; opacity: 0.7; line-height: 1; align-self: flex-start; margin-top: 0.3rem; }
    .bal-number { font-size: 3.5rem; font-weight: 700; line-height: 1; }
    .bal-meta { display: flex; gap: 0.75rem; align-items: center; margin-top: auto; }
    .bal-type { font-size: 0.75rem; background: rgba(255,255,255,0.12); border-radius: 20px; padding: 0.25rem 0.75rem; }
    .bal-currency-tag { font-size: 0.75rem; opacity: 0.5; }

    /* Action Cards */
    .action-col { display: flex; flex-direction: column; gap: 1rem; }

    .action-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      border-radius: 16px;
      padding: 1.25rem 1.5rem;
      border: none;
      cursor: pointer;
      text-align: left;
      transition: transform 0.15s, box-shadow 0.15s;
      flex: 1;
    }
    .action-card:hover { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(0,0,0,0.14); }
    .action-card:active { transform: scale(0.98); }

    .action-add { background: #1a2744; color: white; }
    .action-withdraw { background: #2d1f0a; color: #f5c870; }

    .action-icon-wrap {
      width: 42px; height: 42px; border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
    }
    .add-icon-wrap { background: rgba(255,255,255,0.12); color: white; }
    .withdraw-icon-wrap { background: rgba(245,200,112,0.18); color: #f5c870; }

    .action-text { display: flex; flex-direction: column; gap: 0.1rem; }
    .action-title { font-size: 1rem; font-weight: 600; }
    .action-sub { font-size: 0.75rem; opacity: 0.6; }

    /* Fee Banner */
    .fee-banner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      background: white;
      border: 1px solid #e2e8f0;
      border-radius: 16px;
      padding: 1.1rem 1.5rem;
      margin-bottom: 1.25rem;
      gap: 1rem;
    }
    .fee-banner-left { display: flex; align-items: center; gap: 1rem; }
    .fee-icon { font-size: 1.75rem; color: #4ade80; background: #f0fdf4; border-radius: 10px; padding: 0.3rem; }
    .fee-title { font-weight: 600; font-size: 0.9rem; color: #1e293b; }
    .fee-sub { font-size: 0.8rem; color: #64748b; }
    .apply-btn {
      background: transparent; border: 1.5px solid #1e293b;
      border-radius: 10px; padding: 0.55rem 1.1rem;
      font-size: 0.85rem; font-weight: 600; color: #1e293b;
      cursor: pointer; white-space: nowrap; text-decoration: none;
      transition: background 0.2s, color 0.2s;
    }
    .apply-btn:hover { background: #1e293b; color: white; }

    /* Transactions */
    .tx-section {
      background: white;
      border: 1px solid #e2e8f0;
      border-radius: 16px;
      overflow: hidden;
    }
    .tx-tabs {
      display: flex;
      border-bottom: 1px solid #f1f5f9;
      padding: 0 1.5rem;
    }
    .tx-tab {
      padding: 0.9rem 1rem;
      font-size: 0.875rem;
      font-weight: 500;
      background: none;
      border: none;
      color: #94a3b8;
      cursor: pointer;
      border-bottom: 2px solid transparent;
      margin-bottom: -1px;
      transition: all 0.2s;
    }
    .tx-tab.active { color: #0f172a; border-bottom-color: #0f172a; font-weight: 600; }
    .tx-list { }
    .tx-row {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #f8fafc;
      transition: background 0.15s;
    }
    .tx-row:last-child { border-bottom: none; }
    .tx-row:hover { background: #f8fafc; }
    .tx-dot {
      width: 36px; height: 36px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      font-size: 1rem; flex-shrink: 0;
    }
    .tx-credit { background: #dcfce7; color: #16a34a; }
    .tx-debit { background: #fee2e2; color: #dc2626; }
    .tx-info { flex: 1; }
    .tx-type { font-size: 0.875rem; font-weight: 600; color: #1e293b; }
    .tx-remark { font-size: 0.78rem; color: #94a3b8; }
    .tx-date { font-size: 0.75rem; color: #cbd5e1; margin-top: 0.1rem; }
    .tx-amt { font-size: 0.95rem; font-weight: 700; white-space: nowrap; }
    .credit { color: #16a34a; }
    .debit { color: #dc2626; }
    .no-tx {
      padding: 3rem;
      text-align: center;
      color: #94a3b8;
    }
    .no-tx-icon { font-size: 2.5rem; display: block; margin-bottom: 0.5rem; }

    /* Overlay & Modal */
    .overlay {
      position: fixed; inset: 0; z-index: 1000;
      background: rgba(0,0,0,0.5);
      backdrop-filter: blur(4px);
      display: flex; align-items: center; justify-content: center;
      animation: fadeOverlay 0.2s ease;
    }
    @keyframes fadeOverlay { from { opacity: 0; } to { opacity: 1; } }

    .modal {
      background: white;
      border-radius: 20px;
      padding: 2rem;
      width: 100%;
      max-width: 420px;
      margin: 1rem;
      box-shadow: 0 20px 60px rgba(0,0,0,0.2);
      animation: slideModal 0.25s cubic-bezier(0.34,1.56,0.64,1);
    }
    @keyframes slideModal { from { opacity: 0; transform: scale(0.92) translateY(20px); } to { opacity: 1; transform: scale(1) translateY(0); } }

    .modal-header {
      display: flex; align-items: center; gap: 1rem;
      margin-bottom: 1.75rem;
    }
    .modal-icon {
      width: 46px; height: 46px; border-radius: 14px;
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
    }
    .add-modal-icon { background: #1a2744; color: white; }
    .withdraw-modal-icon { background: #2d1f0a; color: #f5c870; }
    .modal-title { font-size: 1.1rem; font-weight: 700; color: #0f172a; }
    .modal-sub { font-size: 0.8rem; color: #94a3b8; }
    .modal-close {
      margin-left: auto; background: #f1f5f9; border: none;
      border-radius: 50%; width: 34px; height: 34px;
      display: flex; align-items: center; justify-content: center;
      cursor: pointer; color: #64748b; transition: background 0.2s;
    }
    .modal-close:hover { background: #e2e8f0; }

    .field-group { margin-bottom: 1.25rem; }
    .field-group label { display: block; font-size: 0.825rem; font-weight: 600; color: #374151; margin-bottom: 0.45rem; }
    .optional { font-weight: 400; color: #94a3b8; }
    .input-row { display: flex; align-items: center; border: 1.5px solid #e2e8f0; border-radius: 12px; overflow: hidden; background: #f8fafc; transition: border-color 0.2s; }
    .input-row:focus-within { border-color: #0f172a; background: white; }
    .prefix { padding: 0 0.75rem; color: #64748b; font-size: 1rem; font-weight: 500; }
    .modal-input {
      flex: 1; border: none; background: transparent;
      padding: 0.75rem 0.75rem 0.75rem 0;
      font-size: 1rem; color: #0f172a; outline: none;
    }
    .modal-input.plain {
      width: 100%; border: 1.5px solid #e2e8f0; border-radius: 12px;
      padding: 0.75rem 1rem; background: #f8fafc; box-sizing: border-box;
    }
    .modal-input.plain:focus { border-color: #0f172a; background: white; outline: none; }
    .field-group small { display: block; margin-top: 0.35rem; font-size: 0.75rem; color: #94a3b8; }

    .modal-btn {
      width: 100%; padding: 0.85rem;
      border: none; border-radius: 12px;
      font-size: 0.95rem; font-weight: 600;
      cursor: pointer; display: flex; align-items: center;
      justify-content: center; gap: 0.6rem;
      transition: opacity 0.2s, transform 0.15s;
      margin-top: 0.5rem;
    }
    .modal-btn:hover:not(:disabled) { opacity: 0.9; transform: translateY(-1px); }
    .modal-btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .add-btn { background: #1a2744; color: white; }
    .withdraw-btn { background: #7c3f00; color: #f5c870; }

    .spin { animation: doSpin 1s linear infinite; }
    @keyframes doSpin { 100% { transform: rotate(360deg); } }

    /* Toasts */
    .toast-wrap {
      position: fixed; bottom: 2rem; right: 2rem; z-index: 2000;
      display: flex; flex-direction: column; gap: 0.75rem;
    }
    .toast {
      display: flex; align-items: center; gap: 0.75rem;
      padding: 0.875rem 1.25rem; border-radius: 12px;
      font-weight: 500; font-size: 0.9rem;
      box-shadow: 0 4px 16px rgba(0,0,0,0.12);
      animation: slideIn 0.3s ease;
    }
    @keyframes slideIn { from { opacity: 0; transform: translateX(50px); } to { opacity: 1; transform: translateX(0); } }
    .error-toast { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; }
    .success-toast { background: #f0fdf4; border: 1px solid #bbf7d0; color: #15803d; }
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
    withdrawForm: FormGroup;
    loading = signal(false);
    withdrawLoading = signal(false);
    errorMessage = signal('');
    successMessage = signal('');

    showAddMoney = signal(false);
    showWithdraw = signal(false);
    txFilter = signal<'all' | 'debit'>('all');

    filteredEntries = () => {
        const entries = this.walletSummary()?.recentEntries ?? [];
        if (this.txFilter() === 'debit') return entries.filter(e => e.direction !== 'Credit');
        return entries;
    };

    constructor() {
        this.topupForm = this.fb.group({
            amount: ['', [Validators.required, Validators.min(100), Validators.max(500000)]]
        });
        this.withdrawForm = this.fb.group({
            amount: ['', [Validators.required, Validators.min(1)]],
            remarks: ['']
        });
    }

    ngOnInit() { this.loadWalletData(); }

    openAddMoneyModal() { this.showAddMoney.set(true); this.showWithdraw.set(false); }
    openWithdrawModal() { this.showWithdraw.set(true); this.showAddMoney.set(false); }
    closeModals() { this.showAddMoney.set(false); this.showWithdraw.set(false); }

    loadWalletData() {
        this.applicationService.getWalletSummary().subscribe({
            next: (s) => this.walletSummary.set(s),
            error: () => this.showError('Failed to load wallet data')
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
        if (!this.topupForm.valid) return;
        this.loading.set(true);
        this.applicationService.createTopUpOrder({ amount: this.topupForm.value.amount, currency: 'INR' }).subscribe({
            next: (r: CreateTopUpOrderResponse) => this.handleRazorpay(r),
            error: (err) => { this.loading.set(false); this.showError(err.error?.message || 'Failed to create order'); }
        });
    }

    private handleRazorpay(order: CreateTopUpOrderResponse) {
        const rzp = new Razorpay({
            key: order.keyId,
            amount: order.amount * 100,
            currency: order.currency,
            name: 'CapFinLoan',
            description: 'Wallet Top-up',
            order_id: order.providerOrderId,
            handler: (r: any) => this.verifyPayment(r),
            theme: { color: '#1a2744' },
            modal: { ondismiss: () => { this.loading.set(false); this.showError('Payment cancelled'); } }
        });
        rzp.open();
    }

    private verifyPayment(p: any) {
        this.applicationService.verifyTopUp({
            providerOrderId: p.razorpay_order_id,
            providerPaymentId: p.razorpay_payment_id,
            providerSignature: p.razorpay_signature
        }).subscribe({
            next: (r) => {
                this.loading.set(false);
                this.walletSummary.set(r.wallet);
                this.topupForm.reset();
                this.closeModals();
                this.showSuccess('Top-up successful! Wallet credited.');
            },
            error: (err) => { this.loading.set(false); this.showError(err.error?.message || 'Verification failed'); }
        });
    }

    submitWithdraw() {
        if (!this.withdrawForm.valid) return;
        const balance = this.walletSummary()?.balance ?? 0;
        const amount = this.withdrawForm.value.amount;
        if (amount > balance) { this.showError(`Insufficient balance. Available: \u20b9${balance.toFixed(2)}`); return; }
        this.withdrawLoading.set(true);
        this.applicationService.withdraw({ amount, remarks: this.withdrawForm.value.remarks || null }).subscribe({
            next: (s) => {
                this.withdrawLoading.set(false);
                this.walletSummary.set(s);
                this.withdrawForm.reset();
                this.closeModals();
                this.showSuccess('Withdrawal successful!');
            },
            error: (err) => { this.withdrawLoading.set(false); this.showError(err.error?.message || 'Withdrawal failed'); }
        });
    }

    private showError(msg: string) {
        this.errorMessage.set(msg);
        setTimeout(() => this.errorMessage.set(''), 5000);
    }
    private showSuccess(msg: string) {
        this.successMessage.set(msg);
        setTimeout(() => this.successMessage.set(''), 5000);
    }
}
