import { DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Listing } from '../core/models';

/** Картка оголошення для сітки каталогу та обраного. */
import { PhotoPipe } from './photo.pipe';
@Component({
  selector: 'app-listing-card',
  imports: [PhotoPipe, RouterLink, DecimalPipe],
  template: `
    <a class="card" [routerLink]="['/listing', listing().id]">
      <div class="card-img">
        @if (listing().photos.length) {
          <img [src]="listing().photos[0] | photo" [alt]="listing().title" loading="lazy" />
        } @else {
          <span class="no-photo">Фото відсутнє</span>
        }
      </div>
      <div class="card-body">
        <div class="card-title">{{ listing().title }}</div>
        <div class="card-price">{{ listing().price | number: '1.0-0' }} ₴</div>
        <div class="muted small">{{ listing().category }} · {{ listing().city || 'Місто не вказано' }}</div>
      </div>
    </a>
  `,
})
export class ListingCard {
  listing = input.required<Listing>();
}
