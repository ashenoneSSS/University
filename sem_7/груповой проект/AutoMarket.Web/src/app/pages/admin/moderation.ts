import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorText } from '../../core/api';
import { Listing, ListingStatus, PagedResult, STATUS_LABELS } from '../../core/models';

/** Модерація: схвалення / відхилення оголошень. */
import { PhotoPipe } from '../../shared/photo.pipe';
@Component({
  selector: 'app-moderation',
  imports: [PhotoPipe, DecimalPipe, DatePipe, FormsModule],
  template: `
    <div class="tabs">
      @for (s of statuses; track s) {
        <button [class.active]="status() === s" (click)="setStatus(s)">{{ labels[s] }}</button>
      }
    </div>
    @if (error()) { <div class="alert">{{ error() }}</div> }

    @if (page(); as p) {
      @if (p.items.length === 0) { <div class="empty">Немає оголошень зі статусом «{{ labels[status()] }}».</div> }
      <div class="list">
        @for (l of p.items; track l.id) {
          <div class="list-item">
            <div class="thumb">
              @if (l.photos.length) { <img [src]="l.photos[0] | photo" alt="" /> } @else { <span class="no-photo">—</span> }
            </div>
            <div class="grow">
              <div class="row-between">
                <b>#{{ l.id }} {{ l.title }}</b>
                <span class="status status-{{ l.status }}">{{ labels[l.status] }}</span>
              </div>
              <div>{{ l.price | number: '1.0-0' }} ₴ · {{ l.category }} · {{ l.city || '—' }} · автор: {{ l.author }}</div>
              <p class="small">{{ l.description }}</p>
              <div class="muted small">{{ l.createdAt | date: 'dd.MM.yyyy HH:mm' }}</div>
              @if (l.rejectReason) { <div class="muted small">Причина: {{ l.rejectReason }}</div> }
              @if (rejectingId() === l.id) {
                <div class="row">
                  <input class="grow" [(ngModel)]="reason" placeholder="Причина відхилення" />
                  <button class="btn btn-danger" [disabled]="reason.trim().length < 3" (click)="reject(l)">Відхилити</button>
                  <button class="btn" (click)="rejectingId.set(null)">Скасувати</button>
                </div>
              }
            </div>
            <div class="actions">
              @if (l.status !== 'Active' && l.status !== 'Deleted') {
                <button class="btn btn-success" (click)="approve(l)">Схвалити</button>
              }
              @if (l.status !== 'Rejected' && l.status !== 'Deleted') {
                <button class="btn btn-danger" (click)="startReject(l.id)">Відхилити</button>
              }
            </div>
          </div>
        }
      </div>
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
export class Moderation implements OnInit {
  private api = inject(ApiService);

  labels = STATUS_LABELS;
  statuses: ListingStatus[] = ['Pending', 'Active', 'Rejected', 'Deleted'];
  status = signal<ListingStatus>('Pending');
  page = signal<PagedResult<Listing> | null>(null);
  error = signal('');
  rejectingId = signal<number | null>(null);
  reason = '';

  ngOnInit(): void {
    this.load();
  }

  setStatus(s: ListingStatus): void {
    this.status.set(s);
    this.load();
  }

  load(page = 1): void {
    this.error.set('');
    this.api.moderation(this.status(), page).subscribe({
      next: (p) => this.page.set(p),
      error: (e) => this.error.set(errorText(e)),
    });
  }

  approve(l: Listing): void {
    this.api.approve(l.id).subscribe({ next: () => this.load(this.page()?.page), error: (e) => this.error.set(errorText(e)) });
  }

  startReject(id: number): void {
    this.reason = '';
    this.rejectingId.set(id);
  }

  reject(l: Listing): void {
    this.api.reject(l.id, this.reason.trim()).subscribe({
      next: () => { this.rejectingId.set(null); this.load(this.page()?.page); },
      error: (e) => this.error.set(errorText(e)),
    });
  }
}
