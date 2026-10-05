import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorText } from '../../core/api';
import { AuthService } from '../../core/auth';
import { PagedResult, User } from '../../core/models';

/** Керування користувачами: пошук, блокування, ролі, видалення. */
@Component({
  selector: 'app-users',
  imports: [FormsModule, DatePipe],
  template: `
    <form class="row" (ngSubmit)="load()">
      <input class="grow" name="search" [(ngModel)]="search" placeholder="Пошук за email або ім'ям" />
      <button class="btn btn-primary">Знайти</button>
    </form>
    @if (error()) { <div class="alert">{{ error() }}</div> }

    @if (page(); as p) {
      <table class="table">
        <thead>
          <tr><th>ID</th><th>Email</th><th>Ім'я</th><th>Роль</th><th>Оголошень</th><th>Зареєстрований</th><th>Статус</th><th></th></tr>
        </thead>
        <tbody>
          @for (u of p.items; track u.id) {
            <tr [class.blocked]="u.isBlocked">
              <td>{{ u.id }}</td>
              <td>{{ u.email }}</td>
              <td>{{ u.name }}</td>
              <td>
                <select [ngModel]="u.role" (ngModelChange)="setRole(u, $event)" [disabled]="u.id === me">
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </td>
              <td>{{ u.listingsCount }}</td>
              <td>{{ u.createdAt | date: 'dd.MM.yyyy' }}</td>
              <td>{{ u.isBlocked ? 'Заблокований' : 'Активний' }}</td>
              <td class="actions-inline">
                @if (u.id !== me) {
                  <button class="btn btn-sm" (click)="toggleBlock(u)">{{ u.isBlocked ? 'Розблокувати' : 'Заблокувати' }}</button>
                  <button class="btn btn-sm btn-danger" (click)="remove(u)">Видалити</button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (p.totalPages > 1) {
        <div class="pager">
          <button class="btn" [disabled]="!p.hasPrevious" (click)="load(p.page - 1)">←</button>
          <span>{{ p.page }} / {{ p.totalPages }}</span>
          <button class="btn" [disabled]="!p.hasNext" (click)="load(p.page + 1)">→</button>
        </div>
      }
    }
  `,
})
export class Users implements OnInit {
  private api = inject(ApiService);
  me = inject(AuthService).user()?.userId;

  page = signal<PagedResult<User> | null>(null);
  error = signal('');
  search = '';

  ngOnInit(): void {
    this.load();
  }

  load(page = 1): void {
    this.error.set('');
    this.api.users(this.search, page).subscribe({ next: (p) => this.page.set(p), error: (e) => this.error.set(errorText(e)) });
  }

  toggleBlock(u: User): void {
    this.api.setBlocked(u.id, !u.isBlocked).subscribe({ next: () => this.reload(), error: (e) => this.error.set(errorText(e)) });
  }

  setRole(u: User, role: 'User' | 'Admin'): void {
    this.api.setRole(u.id, role).subscribe({ next: () => this.reload(), error: (e) => { this.error.set(errorText(e)); this.reload(); } });
  }

  remove(u: User): void {
    if (!confirm(`Видалити ${u.email} разом з усіма оголошеннями?`)) return;
    this.api.deleteUser(u.id).subscribe({ next: () => this.reload(), error: (e) => this.error.set(errorText(e)) });
  }

  private reload(): void {
    this.load(this.page()?.page ?? 1);
  }
}
