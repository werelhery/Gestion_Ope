import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShipmentStore } from '../../state/shipment.store';
import { WarehouseStore } from '../../state/other.stores';
import { AuthStore } from '../../state/auth.store';
import {
  ShipmentStatus,
  ShipmentPriority,
  Shipment,
  ShipmentDetail,
  CreateShipmentPayload,
  UpdateShipmentStatusPayload
} from '../../core/models/models';

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './shipments.component.html'
})
export class ShipmentsComponent implements OnInit {
  readonly shipmentStore = inject(ShipmentStore);
  readonly warehouseStore = inject(WarehouseStore);
  readonly authStore = inject(AuthStore);

  readonly ShipmentStatus = ShipmentStatus;
  readonly ShipmentPriority = ShipmentPriority;

  selectedStatus = signal<ShipmentStatus | null>(null);
  searchQuery = '';

  showTrackingModal = signal<boolean>(false);
  trackingDetail = signal<ShipmentDetail | null>(null);

  showCreateModal = signal<boolean>(false);
  showStatusModal = signal<boolean>(false);
  targetShipmentForStatus = signal<Shipment | null>(null);

  newShipment = {
    description: '',
    senderName: '',
    recipientName: '',
    originCity: '',
    destCity: '',
    weightKg: 50,
    priority: ShipmentPriority.Normal,
    originWarehouseId: '',
    isTempControlled: false
  };

  statusUpdate = {
    newStatus: ShipmentStatus.InTransit,
    location: '',
    reason: ''
  };

  ngOnInit(): void {
    this.shipmentStore.loadShipments();
    this.warehouseStore.loadWarehouses();
  }

  setStatusFilter(status: ShipmentStatus | null): void {
    this.selectedStatus.set(status);
    this.shipmentStore.setFilters(status, null, this.searchQuery);
  }

  onSearchChange(): void {
    this.shipmentStore.setFilters(this.selectedStatus(), null, this.searchQuery);
  }

  viewTracking(id: string): void {
    this.shipmentStore.loadShipmentDetails(id);
    this.showTrackingModal.set(true);
    // Bind current details
    setTimeout(() => {
      this.trackingDetail.set(this.shipmentStore.selectedShipment());
    }, 150);
  }

  closeTrackingModal(): void {
    this.showTrackingModal.set(false);
  }

  openCreateModal(): void {
    const defaultWh = this.warehouseStore.warehouses()[0]?.id || '';
    this.newShipment.originWarehouseId = defaultWh;
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  submitCreateShipment(): void {
    const payload: CreateShipmentPayload = {
      description: this.newShipment.description,
      senderName: this.newShipment.senderName,
      recipientName: this.newShipment.recipientName,
      originAddress: {
        street: 'Plateforme Industrielle',
        city: this.newShipment.originCity,
        postalCode: '75000',
        country: 'France',
        latitude: 48.8,
        longitude: 2.3
      },
      destinationAddress: {
        street: 'Avenue du Commerce',
        city: this.newShipment.destCity,
        postalCode: '69000',
        country: 'France',
        latitude: 45.7,
        longitude: 4.8
      },
      originWarehouseId: this.newShipment.originWarehouseId,
      priority: this.newShipment.priority,
      dimensions: {
        lengthCm: 80,
        widthCm: 60,
        heightCm: 50,
        weightKg: this.newShipment.weightKg
      },
      isTemperatureControlled: this.newShipment.isTempControlled,
      requiredTemperatureCelsius: this.newShipment.isTempControlled ? 4.0 : undefined,
      scheduledPickupDateUtc: new Date().toISOString(),
      estimatedDeliveryDateUtc: new Date(Date.now() + 24 * 3600 * 1000).toISOString()
    };

    this.shipmentStore.createShipment(payload, () => {
      this.closeCreateModal();
    });
  }

  openStatusModal(s: Shipment): void {
    this.targetShipmentForStatus.set(s);
    this.statusUpdate.newStatus = s.status;
    this.statusUpdate.location = s.originAddress.city;
    this.statusUpdate.reason = 'Pointage régulier transporteur';
    this.showStatusModal.set(true);
  }

  closeStatusModal(): void {
    this.showStatusModal.set(false);
  }

  submitUpdateStatus(): void {
    const s = this.targetShipmentForStatus();
    if (!s) return;

    const payload: UpdateShipmentStatusPayload = {
      newStatus: this.statusUpdate.newStatus,
      location: this.statusUpdate.location,
      reason: this.statusUpdate.reason
    };

    this.shipmentStore.updateStatus(s.id, payload, () => {
      this.closeStatusModal();
    });
  }

  getStatusLabel(status: ShipmentStatus): string {
    switch (status) {
      case ShipmentStatus.Draft: return 'Brouillon';
      case ShipmentStatus.Scheduled: return 'Planifié';
      case ShipmentStatus.InTransit: return 'En Transit';
      case ShipmentStatus.OutForDelivery: return 'En Livraison';
      case ShipmentStatus.Delivered: return 'Livré';
      case ShipmentStatus.Delayed: return 'Retardé';
      case ShipmentStatus.Cancelled: return 'Annulé';
      default: return 'Inconnu';
    }
  }

  getStatusBadgeClass(status: ShipmentStatus): string {
    switch (status) {
      case ShipmentStatus.InTransit: return 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/30';
      case ShipmentStatus.OutForDelivery: return 'bg-cyan-500/10 text-cyan-400 border border-cyan-500/30';
      case ShipmentStatus.Delivered: return 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/30';
      case ShipmentStatus.Delayed: return 'bg-rose-500/10 text-rose-400 border border-rose-500/30';
      default: return 'bg-slate-800 text-slate-400 border border-slate-700';
    }
  }

  getStatusDotClass(status: ShipmentStatus): string {
    switch (status) {
      case ShipmentStatus.InTransit: return 'bg-indigo-400';
      case ShipmentStatus.OutForDelivery: return 'bg-cyan-400';
      case ShipmentStatus.Delivered: return 'bg-emerald-400';
      case ShipmentStatus.Delayed: return 'bg-rose-400';
      default: return 'bg-slate-400';
    }
  }
}
