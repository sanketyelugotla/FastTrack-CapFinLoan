import { Component, inject } from '@angular/core';
import { ErrorService, ToastMessage } from '../../../core/services/error.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  templateUrl: './toast.component.html'
})
export class ToastComponent {
  errorService = inject(ErrorService);

  getClass(toast: ToastMessage): string {
    if (toast.type === 'error') return 'bg-red-600 text-white';
    if (toast.type === 'success') return 'bg-emerald-600 text-white';
    return 'bg-slate-700 text-white';
  }

  getIcon(toast: ToastMessage): string {
    if (toast.type === 'error') return 'error';
    if (toast.type === 'success') return 'check_circle';
    return 'info';
  }
}
