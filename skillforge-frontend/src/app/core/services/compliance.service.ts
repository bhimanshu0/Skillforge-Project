import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ComplianceSummary, AuditLogItem } from '../models';

@Injectable({ providedIn: 'root' })
export class ComplianceService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getComplianceSummary() {
    return this.http.get<ComplianceSummary>(`${this.base}/ComplianceRecord/Summary`);
  }

  runComplianceCheck() {
    return this.http.get(`${this.base}/ComplianceRecord/Refresh`, { responseType: 'text' });
  }

  getAuditLogs() {
    return this.http
      .get<{ message: string; data: AuditLogItem[] }>(`${this.base}/AuditLog`)
      .pipe(
        map(res => res.data ?? []),
        catchError(err => {
          // 404 means no logs yet — treat as empty list
          if (err.status === 404) return of([] as AuditLogItem[]);
          throw err;
        })
      );
  }
}
