import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../state/auth.store';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  readonly authStore = inject(AuthStore);

  email = 'admin@gestionope.fr';
  password = 'Admin123!';

  onSubmit(): void {
    if (this.email && this.password) {
      this.authStore.login(this.email, this.password);
    }
  }

  demoLogin(role: 'admin' | 'manager' | 'operator'): void {
    this.authStore.demoLogin(role);
  }
}
