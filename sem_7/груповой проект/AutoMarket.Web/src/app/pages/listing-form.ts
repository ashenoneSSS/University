import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService, errorText } from '../core/api';
import { Category, ListingForm as ListingFormModel } from '../core/models';

/** Створення (/create) і редагування (/edit/:id) оголошення. */
@Component({
  selector: 'app-listing-form',
  imports: [FormsModule],
  template: `
    <div class="form-box">
      <h1>{{ id() ? 'Редагування оголошення' : 'Нове оголошення' }}</h1>
      @if (id()) {
        <p class="muted small">Після збереження оголошення знову піде на модерацію.</p>
      }
      <form #f="ngForm" (ngSubmit)="submit()">
        <label>Назва *
          <input name="title" required minlength="3" maxlength="150" [(ngModel)]="form.title" placeholder="Toyota Camry 2012" />
        </label>
        <label>Категорія *
          <select name="categoryId" required [(ngModel)]="form.categoryId">
            <option [ngValue]="0" disabled>Оберіть категорію</option>
            @for (c of categories(); track c.id) { <option [ngValue]="c.id">{{ c.name }}</option> }
          </select>
        </label>
        <div class="row">
          <label class="grow">Ціна, ₴ *
            <input name="price" type="number" required min="0" [(ngModel)]="form.price" />
          </label>
          <label class="grow">Місто
            <input name="city" maxlength="100" [(ngModel)]="form.city" placeholder="Київ" />
          </label>
        </div>
        <label>Опис * (мінімум 10 символів)
          <textarea name="description" rows="6" required minlength="10" maxlength="5000" [(ngModel)]="form.description"></textarea>
        </label>
        <label>Посилання на фото (кожне з нового рядка, до 10)
          <textarea name="photos" rows="3" [(ngModel)]="photosText" placeholder="https://..."></textarea>
        </label>
        @if (error()) { <div class="alert">{{ error() }}</div> }
        <button class="btn btn-primary" [disabled]="f.invalid || !form.categoryId || busy()">
          {{ id() ? 'Зберегти' : 'Опублікувати' }}
        </button>
      </form>
    </div>
  `,
})
export class ListingForm implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  /** id з маршруту /edit/:id; для /create — порожній */
  id = input<string>();

  categories = signal<Category[]>([]);
  busy = signal(false);
  error = signal('');

  form: ListingFormModel = { title: '', description: '', price: 0, city: '', categoryId: 0, photoUrls: [] };
  photosText = '';

  ngOnInit(): void {
    this.api.categories().subscribe((c) => this.categories.set(c));
    const id = this.id();
    if (id) {
      this.api.myListing(+id).subscribe({
        next: (l) => {
          this.form = { title: l.title, description: l.description, price: l.price, city: l.city ?? '', categoryId: l.categoryId, photoUrls: l.photos };
          this.photosText = l.photos.join('\n');
          this.categories.update((c) => [...c]); // оновити шаблон після заповнення форми
        },
        error: (e) => this.error.set(errorText(e)),
      });
    }
  }

  submit(): void {
    this.form.photoUrls = this.photosText.split('\n').map((s) => s.trim()).filter(Boolean);
    this.busy.set(true);
    this.error.set('');
    const id = this.id();
    const request = id ? this.api.updateListing(+id, this.form) : this.api.createListing(this.form);
    request.subscribe({
      next: () => this.router.navigateByUrl('/my'),
      error: (e) => { this.error.set(errorText(e)); this.busy.set(false); },
    });
  }
}
