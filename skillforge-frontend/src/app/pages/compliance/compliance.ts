import { Component, inject, OnInit, signal } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ComplianceService } from '../../core/services/compliance.service';
import { AuthService } from '../../core/services/auth.service';
import { ComplianceSummary, AuditLogItem, ComplianceItem } from '../../core/models';

@Component({
  selector: 'app-compliance',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './compliance.html',
})
export class ComplianceComponent implements OnInit {
  private svc   = inject(ComplianceService);
  readonly auth = inject(AuthService);

  summary   = signal<ComplianceSummary | null>(null);
  auditLogs = signal<AuditLogItem[]>([]);

  complianceLoading = signal(false);
  auditLoading      = signal(false);
  refreshLoading    = signal(false);

  toast     = signal('');
  toastType = signal<'success' | 'error'>('success');

  // ── Compliance filters ────────────────────────────────────────────────────
  employeeSearch  = signal('');
  statusFilter    = signal<'All' | 'Compliant' | 'Non-Compliant'>('All');

  // ── Drill-down (expand per employee) ─────────────────────────────────────
  expandedEmployee = signal<number | null>(null);

  get isHR()       { return this.auth.hasRole('HR'); }
  get isAdminOrHR() { return this.auth.hasRole('Admin', 'HR'); }

  // ── Filtered compliance records ───────────────────────────────────────────
  get filteredRecords(): ComplianceItem[] {
    const records = this.summary()?.records ?? [];
    const search  = this.employeeSearch().toLowerCase().trim();
    const status  = this.statusFilter();
    return records.filter(r => {
      const nameMatch   = !search || r.employeeName.toLowerCase().includes(search);
      const statusMatch = status === 'All'
        || (status === 'Compliant'     &&  r.status)
        || (status === 'Non-Compliant' && !r.status);
      return nameMatch && statusMatch;
    });
  }

  // ── Employee groups for drill-down ────────────────────────────────────────
  get employeeGroups(): { employeeId: number; employeeName: string; records: ComplianceItem[]; compliantCount: number }[] {
    const map = new Map<number, { employeeId: number; employeeName: string; records: ComplianceItem[] }>();
    for (const r of this.filteredRecords) {
      if (!map.has(r.employeeId)) {
        map.set(r.employeeId, { employeeId: r.employeeId, employeeName: r.employeeName, records: [] });
      }
      map.get(r.employeeId)!.records.push(r);
    }
    return Array.from(map.values()).map(g => ({
      ...g,
      compliantCount: g.records.filter(r => r.status).length,
    }));
  }

  toggleEmployee(id: number) {
    this.expandedEmployee.set(this.expandedEmployee() === id ? null : id);
  }

  resetFilters() {
    this.employeeSearch.set('');
    this.statusFilter.set('All');
    this.expandedEmployee.set(null);
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────
  ngOnInit() {
    if (this.isHR)       this.loadSummary();
    if (this.isAdminOrHR) this.loadAuditLogs();
  }

  loadSummary() {
    this.complianceLoading.set(true);
    this.svc.getComplianceSummary().subscribe({
      next:  data => { this.summary.set(data); this.complianceLoading.set(false); },
      error: ()   => { this.complianceLoading.set(false); }
    });
  }

  loadAuditLogs() {
    this.auditLoading.set(true);
    this.svc.getAuditLogs().subscribe({
      next:  logs => { this.auditLogs.set(logs); this.auditLoading.set(false); },
      error: ()   => { this.auditLoading.set(false); }
    });
  }

  runComplianceCheck() {
    this.refreshLoading.set(true);
    this.svc.runComplianceCheck().subscribe({
      next: msg => {
        this.showToast(msg || 'Compliance records refreshed.', 'success');
        this.refreshLoading.set(false);
        this.loadSummary();
      },
      error: err => {
        this.showToast(err?.error?.message ?? 'Compliance check failed.', 'error');
        this.refreshLoading.set(false);
      }
    });
  }

  complianceStatus(status: boolean) { return status ? 'Compliant' : 'Non-Compliant'; }

  showToast(msg: string, type: 'success' | 'error') {
    this.toast.set(msg);
    this.toastType.set(type);
    setTimeout(() => this.toast.set(''), 4000);
  }
}
