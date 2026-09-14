import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../core/services/api.service';
import { User, UserRole } from '../core/models/models';

@Injectable({
  providedIn: 'root'
})
export class AuthStore {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  // State Signals
  readonly currentUser = signal<User | null>(this.getStoredUser());
  readonly token = signal<string | null>(localStorage.getItem('gestion_ope_token'));
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  // Computed Signals
  readonly isAuthenticated = computed(() => !!this.token() && !!this.currentUser());
  readonly isAdmin = computed(() => this.currentUser()?.role === UserRole.Admin);
  readonly userRoleLabel = computed(() => {
    const role = this.currentUser()?.role;
    switch (role) {
      case UserRole.Admin: return 'Administrateur';
      case UserRole.LogisticsManager: return 'Responsable Logistique';
      case UserRole.WarehouseOperator: return 'Opérateur Entrepôt';
      case UserRole.Dispatcher: return 'Dispatcheur';
      default: return 'Utilisateur';
    }
  });

  login(email: string, password: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.login(email, password).subscribe({
      next: (res) => {
        this.setSession(res.token, res.user);
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        const detail = err.error?.detail || err.error?.title || 'Échec de connexion. Vérifiez vos identifiants.';
        this.errorMessage.set(detail);
      }
    });
  }

  demoLogin(role: 'admin' | 'manager' | 'operator'): void {
    const credentials = {
      admin: { email: 'admin@gestionope.fr', pass: 'Admin123!' },
      manager: { email: 'manager@gestionope.fr', pass: 'Manager123!' },
      operator: { email: 'operator@gestionope.fr', pass: 'Operator123!' }
    }[role];

    this.login(credentials.email, credentials.pass);
  }

  logout(): void {
    localStorage.removeItem('gestion_ope_token');
    localStorage.removeItem('gestion_ope_user');
    this.token.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  private setSession(token: string, user: User): void {
    localStorage.setItem('gestion_ope_token', token);
    localStorage.setItem('gestion_ope_user', JSON.stringify(user));
    this.token.set(token);
    this.currentUser.set(user);
  }

  private getStoredUser(): User | null {
    try {
      const stored = localStorage.getItem('gestion_ope_user');
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  }
}
