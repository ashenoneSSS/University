import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, errorText } from '../core/api';
import { Listing, ListingStatus, STATUS_LABELS } from '../core/models';

/** Особистий кабінет: мої оголошення з вкладками за статусами. */
import { PhotoPipe } from '../shared/photo.pipe';
@Component({
  selector: 'app-my-listings',
  imports: [PhotoPipe, RouterLink, DecimalPipe, DatePipe],
  template: `
    <div class="page-head">
      <h1>Мої оголошення</h1>
      <a class="btn btn-primary" routerLink="/create">+ Створити оголошення</a>
    </div>

    <div class="tabs">
      @for (t of tabs; track t) {
        <button [class.active]="tab() === t" (click)="tab.set(t)">
          {{ labels[t] }} <span class="badge">{{ count(t) }}</span>
        </button>
      }
    </div>

    @if (error()) { <div class="alert">{{ error() }}</div> }

    @if (filtered().length === 0) {
      <div class="empty">Тут поки порожньо.</div>
    }
    <div class="list">
      @for (l of filtered(); track l.id) {
        <div class="list-item">
          <div class="thumb">
            @if (l.photos.length) { <img [src]="l.photos[0] | photo" alt="" /> } @else { <span class="no-photo">—</span> }
          </div>
          <div class="grow">
            <div class="row-between">
              <b>{{ l.title }}</b>
              <span class="status status-{{ l.status }}">{{ labels[l.status] }}</span>
            </div>
            <div>{{ l.price | number: '1.0-0' }} ₴ · {{ l.category }} · {{ l.city || '—' }}</div>
            <div class="muted small">Створено {{ l.createdAt | date: 'dd.MM.yyyy HH:mm' }} · 👁 {{ l.views }}</div>
            @if (l.status === 'Rejected' && l.rejectReason) {
              <div class="alert small">Причина відхилення: {{ l.rejectReason }}</div>
            }
          </div>
          <div class="actions">
            @if (l.status === 'Active') { <a class="btn" [routerLink]="['/listing', l.id]">Переглянути</a> }
            <a class="btn" [routerLink]="['/edit', l.id]">Редагувати</a>
            <button class="btn btn-danger" (click)="remove(l)">Видалити</button>
          </div>
        </div>
      }
    </div>
  `,
})
export class MyListings implements OnInit {
  private api = inject(ApiService);

  labels = STATUS_LABELS;
  tabs: ListingStatus[] = ['Active', 'Pending', 'Rejected'];
  tab = signal<ListingStatus>('Active');
  listings = signal<Listing[]>([]);
  error = signal('');

  filtered = computed(() => this.listings().filter((l) => l.status === this.tab()));

  ngOnInit(): void {
    this.api.myListings().subscribe({
      next: (list) => {
        this.listings.set(list);
        // відкриваємо першу непорожню вкладку
        const first = this.tabs.find((t) => list.some((l) => l.status === t));
        if (first) this.tab.set(first);
      },
      error: (e) => this.error.set(errorText(e)),
    });
  }

  count(status: ListingStatus): number {
    return this.listings().filter((l) => l.status === status).length;
  }

  remove(l: Listing): void {
    if (!confirm(`Видалити "${l.title}"?`)) return;
    this.api.deleteListing(l.id).subscribe({
      next: () => this.listings.update((list) => list.filter((x) => x.id !== l.id)),
      error: (e) => this.error.set(errorText(e)),
    });
  }
}
