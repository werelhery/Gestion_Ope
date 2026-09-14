import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IncidentStore } from '../../state/other.stores';
import { IncidentSeverity, IncidentStatus } from '../../core/models/models';

@Component({
  selector: 'app-incidents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './incidents.component.html'
})
export class IncidentsComponent implements OnInit {
  readonly incidentStore = inject(IncidentStore);
  readonly IncidentSeverity = IncidentSeverity;

  ngOnInit(): void {
    this.incidentStore.loadIncidents();
  }

  getSeverityLabel(severity: IncidentSeverity): string {
    switch (severity) {
      case IncidentSeverity.Critical: return 'CRITIQUE';
      case IncidentSeverity.Major: return 'MAJEUR';
      case IncidentSeverity.Moderate: return 'MODÉRÉ';
      default: return 'MINEUR';
    }
  }

  getSeverityBadgeClass(severity: IncidentSeverity): string {
    switch (severity) {
      case IncidentSeverity.Critical: return 'bg-rose-500/20 text-rose-400 border border-rose-500/30';
      case IncidentSeverity.Major: return 'bg-amber-500/20 text-amber-400 border border-amber-500/30';
      case IncidentSeverity.Moderate: return 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/30';
      default: return 'bg-slate-800 text-slate-400';
    }
  }

  getStatusLabel(status: IncidentStatus): string {
    switch (status) {
      case IncidentStatus.Open: return 'En cours de traitement';
      case IncidentStatus.InInvestigation: return 'Enquête transporteur';
      case IncidentStatus.Mitigated: return 'Mesure palliative active';
      case IncidentStatus.Resolved: return 'Résolu & Archivé';
      default: return 'Ouvert';
    }
  }
}
