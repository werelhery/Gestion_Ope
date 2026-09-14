import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardStore } from '../../state/other.stores';
import { ShipmentStatus, ShipmentPriority } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  readonly dashboardStore = inject(DashboardStore);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.dashboardStore.loadMetrics();
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

  getPriorityLabel(priority: ShipmentPriority): string {
    switch (priority) {
      case ShipmentPriority.Critical: return 'Critique';
      case ShipmentPriority.High: return 'Haute';
      case ShipmentPriority.Normal: return 'Normale';
      default: return 'Basse';
    }
  }

  getPriorityBadgeClass(priority: ShipmentPriority): string {
    switch (priority) {
      case ShipmentPriority.Critical: return 'bg-rose-500/20 text-rose-300 border border-rose-500/30';
      case ShipmentPriority.High: return 'bg-amber-500/20 text-amber-300 border border-amber-500/30';
      default: return 'bg-slate-800 text-slate-400';
    }
  }
}
