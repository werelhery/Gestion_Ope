import { Injectable, signal, computed, inject } from '@angular/core';
import { ApiService } from '../core/services/api.service';
import {
  Shipment,
  ShipmentDetail,
  ShipmentStatus,
  ShipmentPriority,
  CreateShipmentPayload,
  UpdateShipmentStatusPayload
} from '../core/models/models';

@Injectable({
  providedIn: 'root'
})
export class ShipmentStore {
  private readonly api = inject(ApiService);

  // State Signals
  readonly shipments = signal<Shipment[]>([]);
  readonly selectedShipment = signal<ShipmentDetail | null>(null);
  readonly statusFilter = signal<ShipmentStatus | null>(null);
  readonly priorityFilter = signal<ShipmentPriority | null>(null);
  readonly searchQuery = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly isActionLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  // Computed Signals
  readonly filteredShipments = computed(() => {
    const list = this.shipments();
    const status = this.statusFilter();
    const priority = this.priorityFilter();
    const search = this.searchQuery().toLowerCase().trim();

    return list.filter(item => {
      const matchStatus = status === null || item.status === status;
      const matchPriority = priority === null || item.priority === priority;
      const matchSearch = !search ||
        item.trackingNumber.toLowerCase().includes(search) ||
        item.senderName.toLowerCase().includes(search) ||
        item.recipientName.toLowerCase().includes(search) ||
        item.description.toLowerCase().includes(search) ||
        item.originAddress.city.toLowerCase().includes(search) ||
        item.destinationAddress.city.toLowerCase().includes(search);

      return matchStatus && matchPriority && matchSearch;
    });
  });

  readonly totalCount = computed(() => this.shipments().length);
  readonly inTransitCount = computed(() => this.shipments().filter(s => s.status === ShipmentStatus.InTransit).length);
  readonly delayedCount = computed(() => this.shipments().filter(s => s.status === ShipmentStatus.Delayed || s.isDelayed).length);
  readonly deliveredCount = computed(() => this.shipments().filter(s => s.status === ShipmentStatus.Delivered).length);

  loadShipments(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.api.getShipments(
      this.statusFilter() ?? undefined,
      this.priorityFilter() ?? undefined,
      this.searchQuery() || undefined
    ).subscribe({
      next: (data) => {
        this.shipments.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Impossible de charger les expéditions.');
        this.isLoading.set(false);
      }
    });
  }

  loadShipmentDetails(id: string): void {
    this.isLoading.set(true);
    this.api.getShipmentById(id).subscribe({
      next: (detail) => {
        this.selectedShipment.set(detail);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  createShipment(payload: CreateShipmentPayload, onSuccess?: () => void): void {
    this.isActionLoading.set(true);
    this.api.createShipment(payload).subscribe({
      next: (newShipment) => {
        this.shipments.update(list => [newShipment, ...list]);
        this.isActionLoading.set(false);
        if (onSuccess) onSuccess();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'Erreur lors de la création de l’expédition.');
        this.isActionLoading.set(false);
      }
    });
  }

  updateStatus(id: string, payload: UpdateShipmentStatusPayload, onSuccess?: () => void): void {
    this.isActionLoading.set(true);
    this.api.updateShipmentStatus(id, payload).subscribe({
      next: (updated) => {
        this.shipments.update(list => list.map(s => s.id === id ? updated : s));
        if (this.selectedShipment()?.id === id) {
          this.loadShipmentDetails(id);
        }
        this.isActionLoading.set(false);
        if (onSuccess) onSuccess();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'Erreur lors de la mise à jour du statut.');
        this.isActionLoading.set(false);
      }
    });
  }

  setFilters(status: ShipmentStatus | null, priority: ShipmentPriority | null, search: string): void {
    this.statusFilter.set(status);
    this.priorityFilter.set(priority);
    this.searchQuery.set(search);
  }
}
