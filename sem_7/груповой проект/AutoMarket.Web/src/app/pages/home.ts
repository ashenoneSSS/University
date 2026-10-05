import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorText } from '../core/api';
import { Category, Listing, ListingQuery, ListingSort, PagedResult } from '../core/models';
import { ListingCard } from '../shared/listing-card';

/** Головна: каталог активних оголошень з пошуком, фільтрами та пагінацією. */
@Component({
  selector: 'app-home',
  imports: [FormsModule, ListingCard],
  template: `
    <section class="hero">
      <h1>Дошка оголошень б/у техніки та авто</h1>
      <form class="search" (ngSubmit)="search()">
        <input name="q" [(ngModel)]="query.q" placeholder="Що шукаєте? Наприклад: iPhone, Toyota..." />
        <button class="btn btn-primary" type="submit">Знайти</button>
      </form>
    </section>

    <div class="layout">
      <aside class="filters">
        <h3>Фільтри</h3>
        <label>Категорія
          <select name="cat" [(ngModel)]="query.categoryId" (ngModelChange)="search()">
            <option [ngValue]="null">Усі категорії</option>
            @for (c of categories(); track c.id) {
              <option [ngValue]="c.id">{{ c.name }} ({{ c.activeListings }})</option>
            }
          </select>
        </label>
        <label>Ціна, ₴
          <div class="row">
            <input type="number" name="min" min="0" [(ngModel)]="query.minPrice" placeholder="від" />
            <input type="number" name="max" min="0" [(ngModel)]="query.maxPrice" placeholder="до" />
          </div>
        </label>
        <label>Місто
          <input name="city" [(ngModel)]="query.city" placeholder="Київ" />
        </label>
        <label>Сортування
          <select name="sort" [(ngModel)]="query.sort" (ngModelChange)="search()">
            @for (s of sorts; track s.value) {
              <option [value]="s.value">{{ s.label }}</option>
            }
          </select>
        </label>
        <button class="btn btn-primary w-100" (click)="search()">Застосувати</button>
        <button class="btn w-100" (click)="reset()">Скинути</button>
      </aside>

      <section class="results">
        @if (error()) { <div class="alert">{{ error() }}</div> }
        @if (loading()) {
          <p class="muted">Завантаження...</p>
        } @else if (result(); as r) {
          <p class="muted">Знайдено: {{ r.totalCount }}</p>
          @if (r.items.length === 0) {
            <div class="empty">Нічого не знайдено. Спробуйте змінити фільтри.</div>
          }
          <div class="grid">
            @for (l of r.items; track l.id) {
              <app-listing-card [listing]="l" />
            }
          </div>
          @if (r.totalPages > 1) {
            <div class="pager">
              <button class="btn" [disabled]="!r.hasPrevious" (click)="goTo(r.page - 1)">← Назад</button>
              <span>Сторінка {{ r.page }} з {{ r.totalPages }}</span>
              <button class="btn" [disabled]="!r.hasNext" (click)="goTo(r.page + 1)">Далі →</button>
            </div>
          }
        }
      </section>
    </div>
  `,
})
export class Home implements OnInit {
  private api = inject(ApiService);

  categories = signal<Category[]>([]);
  result = signal<PagedResult<Listing> | null>(null);
  loading = signal(false);
  error = signal('');

  query: ListingQuery = Home.emptyQuery();

  sorts: { value: ListingSort; label: string }[] = [
    { value: 'Newest', label: 'Спочатку нові' },
    { value: 'PriceAsc', label: 'Спочатку дешеві' },
    { value: 'PriceDesc', label: 'Спочатку дорогі' },
    { value: 'Popular', label: 'Популярні' },
    { value: 'Oldest', label: 'Спочатку старі' },
  ];

  ngOnInit(): void {
    this.api.categories().subscribe({ next: (c) => this.categories.set(c) });
    this.load();
  }

  search(): void {
    this.query.page = 1;
    this.load();
  }

  goTo(page: number): void {
    this.query.page = page;
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  reset(): void {
    this.query = Home.emptyQuery();
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.search(this.query).subscribe({
      next: (r) => { this.result.set(r); this.loading.set(false); },
      error: (e) => { this.error.set(errorText(e)); this.loading.set(false); },
    });
  }

  private static emptyQuery(): ListingQuery {
    return { q: '', categoryId: null, minPrice: null, maxPrice: null, city: '', sort: 'Newest', page: 1, pageSize: 12 };
  }
}
