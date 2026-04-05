import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorService } from '../services/error.service';

function extractMessage(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'Unable to connect to the server. Please check your connection.';
  }
  if (err.status >= 500) {
    return err.error?.message || 'A server error occurred. Please try again.';
  }
  // 4xx — try to extract meaningful message
  if (err.error?.message) return err.error.message;
  if (err.error?.errors) {
    // ASP.NET validation problem details: { errors: { field: ["msg"] } }
    const messages = Object.values(err.error.errors as Record<string, string[]>)
      .flat()
      .join(' ');
    return messages || err.statusText;
  }
  if (typeof err.error === 'string') return err.error;
  return err.message || err.statusText || 'An unexpected error occurred.';
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorService = inject(ErrorService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const message = extractMessage(err);
      // Only show toast for unhandled 5xx / network errors; 4xx are handled locally by components
      if (err.status === 0 || err.status >= 500) {
        errorService.showError(message);
      }
      // Re-throw with normalised message so component error handlers get it too
      const normalised = new HttpErrorResponse({
        error: { message },
        status: err.status,
        statusText: err.statusText,
        url: err.url ?? undefined
      });
      return throwError(() => normalised);
    })
  );
};
