import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService, errorText } from '../../core/api';
import { ListingStatus, STATUS_LABELS, Stats as StatsModel } from '../../core/models';

/** Статистика: картки з ключовими числами + розподіл за категоріями. */
@Component({
  selector: 'app-stats',
  imports: [DecimalPipe],
  template: `
    @if (error()) { <div class="alert">{{ error() }}</div> }
    @if (stats(); as s) {
      <div class="stat-grid">
        <div class="stat"><span>Користувачів</span><b>{{ s.totalUsers }}</b><small>заблоковано: {{ s.blockedUsers }}</small></div>
        <div class="stat"><span>Оголошень</span><b>{{ s.totalListings }}</b><small>за 7 днів: {{ s.newListingsLast7Days }}</small></div>
        <div class="stat"><span>Середня ціна</span><b>{{ s.averageActivePrice | number: '1.0-0' }} ₴</b><small>серед активних</small></div>
        @for (st of statuses; track st) {
          <div class="stat"><span>{{ labels[st] }}</span><b>{{ s.listingsByStatus[st] }}</b></div>
        }
      </div>
      <h3>Активні оголошення за категоріями</h3>
      @for (c of s.activeByCategory; track c.id) {
        <div class="bar-row">
          <span class="bar-label">{{ c.name }}</span>
          <div class="bar"><div class="bar-fill" [style.width.%]="percent(c.activeListings, s)"></div></div>
          <b>{{ c.activeListings }}</b>
        </div>
      }
    }
  `,
})
export class Stats implements OnInit {
  private api = inject(ApiService);
  labels = STATUS_LABELS;
  statuses: ListingStatus[] = ['Active', 'Pending', 'Rejected', 'Deleted'];
  stats = signal<StatsModel | null>(null);
  error = signal('');

  ngOnInit(): void {
    this.api.stats().subscribe({ next: (s) => this.stats.set(s), error: (e) => this.error.set(errorText(e)) });
  }

  percent(value: number, s: StatsModel): number {
    const max = Math.max(...s.activeByCategory.map((c) => c.activeListings), 1);
    return (value / max) * 100;
  }
}
