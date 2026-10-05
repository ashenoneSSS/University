import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { errorText } from '../core/api';
import { AuthService } from '../core/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-box">
      <h1>Вхід</h1>
      <form #f="ngForm" (ngSubmit)="submit()">
        <label>Email
          <input name="email" type="email" required email [(ngModel)]="email" />
        </label>
        <label>Пароль
          <input name="password" type="password" required [(ngModel)]="password" />
        </label>
        @if (error()) { <div class="alert">{{ error() }}</div> }
        <button class="btn btn-primary w-100" [disabled]="f.invalid || busy()">Увійти</button>
      </form>
      <p class="muted small">Немає акаунта? <a routerLink="/register">Зареєструватися</a></p>
      <p class="muted small">Демо: admin&#64;automarket.local / Admin123! · demo&#64;automarket.local / Demo123!</p>
    </div>
  `,
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = '';
  password = '';
  busy = signal(false);
  error = signal('');

  submit(): void {
    this.busy.set(true);
    this.error.set('');
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl(this.route.snapshot.queryParamMap.get('returnUrl') ?? '/'),
      error: (e) => { this.error.set(errorText(e)); this.busy.set(false); },
    });
  }
}
