import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
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
  selector: 'app-admin-wallet',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-wallet.html',
  styleUrl: './admin-wallet.css'
})
export class AdminWalletComponent implements OnInit {
  private applicationService = inject(ApplicationService);
  private fb = inject(FormBuilder);

  walletSummary = signal<WalletSummaryResponse | null>(null);
  walletConfig = signal<WalletConfigResponse>({ applicationFee: 500, minTopUpAmount: 100, maxTopUpAmount: 500000, currency: 'INR' });

  topupForm: FormGroup;
  withdrawForm: FormGroup;
  loading = signal(false);
  withdrawLoading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');
  showAddMoney = signal(false);
  showWithdraw = signal(false);

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

  submitWithdraw() {
    if (!this.withdrawForm.valid) return;
    const balance = this.walletSummary()?.balance ?? 0;
    const amount = this.withdrawForm.value.amount;
    if (amount > balance) { this.showError(`Insufficient balance. Available: \u20b9${balance.toFixed(2)}`); return; }
    this.withdrawLoading.set(true);
    this.applicationService.withdrawAdmin({ amount, remarks: this.withdrawForm.value.remarks || null }).subscribe({
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
}
