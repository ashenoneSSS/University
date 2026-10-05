import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, errorText } from '../core/api';
import { AuthService } from '../core/auth';
import { Listing } from '../core/models';

/** Сторінка оголошення: фото, ціна, характеристики, контакти, обране. */
import { PhotoPipe } from '../shared/photo.pipe';
@Component({
  selector: 'app-listing-details',
  imports: [PhotoPipe, DecimalPipe, DatePipe, RouterLink],
  template: `
    <a routerLink="/" class="back">← До каталогу</a>
    @if (error()) {
      <div class="alert">{{ error() }}</div>
    } @else if (listing(); as l) {
      <div class="details">
        <div class="gallery">
          <div class="main-photo">
            @if (l.photos.length) {
              <img [src]="l.photos[activePhoto()] | photo" [alt]="l.title" />
            } @else {
              <span class="no-photo">Зображення відсутнє</span>
            }
          </div>
          @if (l.photos.length > 1) {
            <div class="thumbs">
              @for (p of l.photos; track p; let i = $index) {
                <img [src]="p | photo" [class.active]="i === activePhoto()" (click)="activePhoto.set(i)" alt="" />
              }
            </div>
          }
        </div>

        <div class="info">
          <div class="muted small">👁 {{ l.views }} переглядів · {{ l.createdAt | date: 'dd.MM.yyyy' }}</div>
          <h1>{{ l.title }}</h1>
          <div class="price-row">
            <span class="price">{{ l.price | number: '1.0-0' }} <small>₴</small></span>
            <button class="btn btn-primary" (click)="showContacts.set(true)">Контакти</button>
            @if (auth.isLoggedIn() && auth.user()?.userId !== l.authorId) {
              <button class="btn" (click)="addFavorite(l.id)" [disabled]="favorited()">
                {{ favorited() ? '★ В обраному' : '☆ Додати в обране' }}
              </button>
            }
          </div>
          @if (showContacts()) {
            <div class="contacts">
              <b>{{ l.author }}</b><br />
              {{ l.authorPhone || 'Продавець не вказав телефон' }}
            </div>
          }
          @if (message()) { <div class="ok">{{ message() }}</div> }

          <div class="props">
            <div><span class="muted">Категорія</span><b>{{ l.category }}</b></div>
            <div><span class="muted">Місто</span><b>{{ l.city || '—' }}</b></div>
            <div><span class="muted">Продавець</span><b>{{ l.author }}</b></div>
          </div>

          <h3>Опис</h3>
          <p class="description">{{ l.description }}</p>
        </div>
      </div>
    } @else {
      <p class="muted">Завантаження...</p>
    }
  `,
})
export class ListingDetails implements OnInit {
  private api = inject(ApiService);
  auth = inject(AuthService);

  /** id з маршруту /listing/:id (withComponentInputBinding) */
  id = input.required<string>();

  listing = signal<Listing | null>(null);
  error = signal('');
  message = signal('');
  activePhoto = signal(0);
  showContacts = signal(false);
  favorited = signal(false);

  ngOnInit(): void {
    this.api.getListing(+this.id()).subscribe({
      next: (l) => this.listing.set(l),
      error: (e) => this.error.set(errorText(e)),
    });
  }

  addFavorite(id: number): void {
    this.api.addFavorite(id).subscribe({
      next: () => { this.favorited.set(true); this.message.set('Додано в обране'); },
      error: (e) => this.error.set(errorText(e)),
    });
  }
}
