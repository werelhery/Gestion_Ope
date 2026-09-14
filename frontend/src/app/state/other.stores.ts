import { Injectable, signal, inject } from '@angular/core';
import { ApiService } from '../core/services/api.service';
import { DashboardMetrics, Warehouse, Incident } from '../core/models/models';

@Injectable({
  providedIn: 'root'
})
export class DashboardStore {
  private readonly api = inject(ApiService);

  readonly metrics = signal<DashboardMetrics | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  loadMetrics(): void {
    this.isLoading.set(true);
    this.api.getDashboardMetrics().subscribe({
      next: (data) => {
        this.metrics.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}

@Injectable({
  providedIn: 'root'
})
export class WarehouseStore {
  private readonly api = inject(ApiService);

  readonly warehouses = signal<Warehouse[]>([]);
  readonly isLoading = signal<boolean>(false);

  loadWarehouses(): void {
    this.isLoading.set(true);
    this.api.getWarehouses().subscribe({
      next: (data) => {
        this.warehouses.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}

@Injectable({
  providedIn: 'root'
})
export class IncidentStore {
  private readonly api = inject(ApiService);

  readonly incidents = signal<Incident[]>([]);
  readonly isLoading = signal<boolean>(false);

  loadIncidents(): void {
    this.isLoading.set(true);
    this.api.getIncidents(undefined, true).subscribe({
      next: (data) => {
        this.incidents.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
