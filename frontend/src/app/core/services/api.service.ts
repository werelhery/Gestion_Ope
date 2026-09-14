import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, catchError } from 'rxjs';
import {
  Shipment,
  ShipmentDetail,
  Warehouse,
  Incident,
  DashboardMetrics,
  AuthResponse,
  CreateShipmentPayload,
  UpdateShipmentStatusPayload,
  ShipmentStatus,
  ShipmentPriority
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5228/api';

  // --- Shipments ---
  getShipments(status?: ShipmentStatus, priority?: ShipmentPriority, search?: string): Observable<Shipment[]> {
    let params = new HttpParams();
    if (status !== undefined && status !== null) params = params.set('status', status.toString());
    if (priority !== undefined && priority !== null) params = params.set('priority', priority.toString());
    if (search) params = params.set('search', search);

    return this.http.get<Shipment[]>(`${this.baseUrl}/shipments`, { params }).pipe(
      catchError(() => of([]))
    );
  }

  getShipmentById(id: string): Observable<ShipmentDetail | null> {
    return this.http.get<ShipmentDetail>(`${this.baseUrl}/shipments/${id}`).pipe(
      catchError(() => of(null))
    );
  }

  getShipmentByTrackingNumber(trackingNumber: string): Observable<ShipmentDetail | null> {
    return this.http.get<ShipmentDetail>(`${this.baseUrl}/shipments/tracking/${trackingNumber}`).pipe(
      catchError(() => of(null))
    );
  }

  createShipment(payload: CreateShipmentPayload): Observable<Shipment> {
    return this.http.post<Shipment>(`${this.baseUrl}/shipments`, payload);
  }

  updateShipmentStatus(id: string, payload: UpdateShipmentStatusPayload): Observable<Shipment> {
    return this.http.put<Shipment>(`${this.baseUrl}/shipments/${id}/status`, payload);
  }

  // --- Warehouses ---
  getWarehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(`${this.baseUrl}/warehouses`).pipe(
      catchError(() => of([]))
    );
  }

  // --- Incidents ---
  getIncidents(shipmentId?: string, onlyOpen: boolean = false): Observable<Incident[]> {
    let params = new HttpParams();
    if (shipmentId) params = params.set('shipmentId', shipmentId);
    if (onlyOpen) params = params.set('onlyOpen', 'true');

    return this.http.get<Incident[]>(`${this.baseUrl}/incidents`, { params }).pipe(
      catchError(() => of([]))
    );
  }

  // --- Dashboard Metrics ---
  getDashboardMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(`${this.baseUrl}/dashboard/metrics`);
  }

  // --- Auth ---
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, { email, password });
  }

  register(payload: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, payload);
  }
}
