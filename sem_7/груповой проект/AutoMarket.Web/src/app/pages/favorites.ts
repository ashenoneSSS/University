import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService, errorText } from '../core/api';
import { Listing } from '../core/models';
import { ListingCard } from '../shared/listing-card';

/** Обрані оголошення. */
@Component({
  selector: 'app-favorites',
  imports: [ListingCard],
  template: `
    <h1>Обране</h1>
    @if (error()) { <div class="alert">{{ error() }}</div> }
    @if (items().length === 0) {
      <div class="empty">В обраному поки нічого немає.</div>
    }
    <div class="grid">
      @for (l of items(); track l.id) {
        <div class="fav">
          <app-listing-card [listing]="l" />
          <button class="btn w-100" (click)="remove(l.id)">Прибрати</button>
        </div>
      }
    </div>
  `,
})
export class Favorites implements OnInit {
  private api = inject(ApiService);
  items = signal<Listing[]>([]);
  error = signal('');

  ngOnInit(): void {
    this.api.favorites().subscribe({ next: (l) => this.items.set(l), error: (e) => this.error.set(errorText(e)) });
  }

  remove(id: number): void {
    this.api.removeFavorite(id).subscribe({
      next: () => this.items.update((list) => list.filter((l) => l.id !== id)),
      error: (e) => this.error.set(errorText(e)),
    });
  }
}
