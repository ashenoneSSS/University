import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { errorText } from '../core/api';
import { AuthService } from '../core/auth';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-box">
      <h1>Реєстрація</h1>
      <form #f="ngForm" (ngSubmit)="submit()">
        <label>Ім'я
          <input name="name" required minlength="2" [(ngModel)]="form.name" />
        </label>
        <label>Email
          <input name="email" type="email" required email [(ngModel)]="form.email" />
        </label>
        <label>Телефон (необов'язково)
          <input name="phone" [(ngModel)]="form.phone" placeholder="+380..." />
        </label>
        <label>Пароль (мінімум 6 символів)
          <input name="password" type="password" required minlength="6" [(ngModel)]="form.password" />
        </label>
        @if (error()) { <div class="alert">{{ error() }}</div> }
        <button class="btn btn-primary w-100" [disabled]="f.invalid || busy()">Зареєструватися</button>
      </form>
      <p class="muted small">Вже є акаунт? <a routerLink="/login">Увійти</a></p>
    </div>
  `,
})
export class Register {
  private auth = inject(AuthService);
  private router = inject(Router);

  form = { name: '', email: '', phone: '', password: '' };
  busy = signal(false);
  error = signal('');

  submit(): void {
    this.busy.set(true);
    this.error.set('');
    this.auth.register({ ...this.form, phone: this.form.phone || undefined }).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: (e) => { this.error.set(errorText(e)); this.busy.set(false); },
    });
  }
}
