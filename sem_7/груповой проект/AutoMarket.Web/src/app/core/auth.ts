import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api';
import { AuthResponse } from './models';

const STORAGE_KEY = 'automarket.auth';

/** Стан авторизації. Токен зберігається в localStorage, щоб переживати перезавантаження сторінки. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  readonly user = signal<AuthResponse | null>(load());
  readonly isLoggedIn = computed(() => this.user() !== null);
  readonly isAdmin = computed(() => this.user()?.role === 'Admin');

  get token(): string | null {
    return this.user()?.token ?? null;
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.api.login(email, password).pipe(tap((res) => this.save(res)));
  }

  register(data: { email: string; password: string; name: string; phone?: string }): Observable<AuthResponse> {
    return this.api.register(data).pipe(tap((res) => this.save(res)));
  }

  logout(redirect = true): void {
    localStorage.removeItem(STORAGE_KEY);
    this.user.set(null);
    if (redirect) this.router.navigateByUrl('/login');
  }

  private save(res: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(res));
    this.user.set(res);
  }
}

function load(): AuthResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const data = JSON.parse(raw) as AuthResponse;
    // протермінований токен одразу відкидаємо
    return new Date(data.expiresAt) > new Date() ? data : null;
  } catch {
    return null;
  }
}
