import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WarehouseStore } from '../../state/other.stores';
import { WarehouseStatus } from '../../core/models/models';

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './warehouses.component.html'
})
export class WarehousesComponent implements OnInit {
  readonly warehouseStore = inject(WarehouseStore);

  ngOnInit(): void {
    this.warehouseStore.loadWarehouses();
  }

  getCapacityTextColor(pct: number): string {
    if (pct > 85) return 'text-rose-400';
    if (pct > 70) return 'text-amber-400';
    return 'text-emerald-400';
  }

  getCapacityBarColor(pct: number): string {
    if (pct > 85) return 'bg-rose-500';
    if (pct > 70) return 'bg-amber-500';
    return 'bg-emerald-500';
  }
}
