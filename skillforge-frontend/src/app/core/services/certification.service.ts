import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CertificationResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class CertificationService {
  private base = `${environment.apiUrl}/Certification`;
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<CertificationResponse[]>(`${this.base}/certifications`);
  }

  issue(employeeId: number, courseId: number) {
    return this.http.post<CertificationResponse>(`${this.base}/certifications`, { employeeId, courseId });
  }

  getMy() {
    return this.http.get<CertificationResponse[]>(`${this.base}/my`);
  }

  download(id: number) {
    return this.http.get(`${this.base}/${id}/download`, { responseType: 'blob' });
  }
}
