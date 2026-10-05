import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <div class="empty">
      <h1>404</h1>
      <p>Сторінку не знайдено.</p>
      <a class="btn btn-primary" routerLink="/">На головну</a>
    </div>
  `,
})
export class NotFound {}
