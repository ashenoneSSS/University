import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

/** Оболонка адмін-панелі з вкладками. */
@Component({
  selector: 'app-admin',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <h1>Панель адміністратора</h1>
    <nav class="tabs">
      <a routerLink="moderation" routerLinkActive="active">Модерація</a>
      <a routerLink="users" routerLinkActive="active">Користувачі</a>
      <a routerLink="stats" routerLinkActive="active">Статистика</a>
    </nav>
    <router-outlet />
  `,
})
export class Admin {}
