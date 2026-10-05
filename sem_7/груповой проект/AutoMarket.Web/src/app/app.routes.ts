import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards';

/** Маршрути: публічні, для гостей, для авторизованих та для адміністратора. Сторінки вантажаться ліниво. */
export const routes: Routes = [
  // публічні
  { path: '', title: 'AutoMarket', loadComponent: () => import('./pages/home').then((m) => m.Home) },
  { path: 'listing/:id', title: 'Оголошення', loadComponent: () => import('./pages/listing-details').then((m) => m.ListingDetails) },

  // лише для гостей
  { path: 'login', title: 'Вхід', canActivate: [guestGuard], loadComponent: () => import('./pages/login').then((m) => m.Login) },
  { path: 'register', title: 'Реєстрація', canActivate: [guestGuard], loadComponent: () => import('./pages/register').then((m) => m.Register) },

  // кабінет користувача
  { path: 'my', title: 'Мої оголошення', canActivate: [authGuard], loadComponent: () => import('./pages/my-listings').then((m) => m.MyListings) },
  { path: 'create', title: 'Нове оголошення', canActivate: [authGuard], loadComponent: () => import('./pages/listing-form').then((m) => m.ListingForm) },
  { path: 'edit/:id', title: 'Редагування', canActivate: [authGuard], loadComponent: () => import('./pages/listing-form').then((m) => m.ListingForm) },
  { path: 'favorites', title: 'Обране', canActivate: [authGuard], loadComponent: () => import('./pages/favorites').then((m) => m.Favorites) },

  // адмін-панель
  {
    path: 'admin',
    title: 'Адмін-панель',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin').then((m) => m.Admin),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'moderation' },
      { path: 'moderation', loadComponent: () => import('./pages/admin/moderation').then((m) => m.Moderation) },
      { path: 'users', loadComponent: () => import('./pages/admin/users').then((m) => m.Users) },
      { path: 'stats', loadComponent: () => import('./pages/admin/stats').then((m) => m.Stats) },
    ],
  },

  { path: '**', title: 'Не знайдено', loadComponent: () => import('./pages/not-found').then((m) => m.NotFound) },
];
