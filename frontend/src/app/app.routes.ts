import { Routes } from '@angular/router';
import { LayoutComponent } from './features/layout/layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ShipmentsComponent } from './features/shipments/shipments.component';
import { WarehousesComponent } from './features/warehouses/warehouses.component';
import { IncidentsComponent } from './features/incidents/incidents.component';
import { LoginComponent } from './features/auth/login.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'shipments', component: ShipmentsComponent },
      { path: 'warehouses', component: WarehousesComponent },
      { path: 'incidents', component: IncidentsComponent }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
