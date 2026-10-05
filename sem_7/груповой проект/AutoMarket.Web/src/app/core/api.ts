import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AuthResponse, Category, Listing, ListingForm, ListingQuery, ListingStatus,
  PagedResult, Stats, User, UserRole,
} from './models';

/** Адреса бекенду (AutoMarket.Api, профіль http). */
export const API_ORIGIN = 'http://localhost:5299';
export const API_URL = `${API_ORIGIN}/api`;

/** Обгортка над REST API бекенду. Один метод = один ендпоінт. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  // ---------- auth ----------
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_URL}/auth/login`, { email, password });
  }

  register(data: { email: string; password: string; name: string; phone?: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_URL}/auth/register`, data);
  }

  // ---------- каталог ----------
  search(query: ListingQuery): Observable<PagedResult<Listing>> {
    return this.http.get<PagedResult<Listing>>(`${API_URL}/listings`, { params: toParams(query) });
  }

  getListing(id: number): Observable<Listing> {
    return this.http.get<Listing>(`${API_URL}/listings/${id}`);
  }

  categories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${API_URL}/categories`);
  }

  // ---------- мої оголошення ----------
  myListings(status?: ListingStatus): Observable<Listing[]> {
    return this.http.get<Listing[]>(`${API_URL}/listings/my`, { params: toParams({ status }) });
  }

  myListing(id: number): Observable<Listing> {
    return this.http.get<Listing>(`${API_URL}/listings/my/${id}`);
  }

  createListing(data: ListingForm): Observable<Listing> {
    return this.http.post<Listing>(`${API_URL}/listings`, data);
  }

  updateListing(id: number, data: ListingForm): Observable<Listing> {
    return this.http.put<Listing>(`${API_URL}/listings/${id}`, data);
  }

  deleteListing(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/listings/${id}`);
  }

  // ---------- обране ----------
  favorites(): Observable<Listing[]> {
    return this.http.get<Listing[]>(`${API_URL}/favorites`);
  }

  addFavorite(id: number): Observable<void> {
    return this.http.post<void>(`${API_URL}/favorites/${id}`, null);
  }

  removeFavorite(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/favorites/${id}`);
  }

  // ---------- адмін ----------
  moderation(status: ListingStatus, page = 1): Observable<PagedResult<Listing>> {
    return this.http.get<PagedResult<Listing>>(`${API_URL}/admin/listings`, { params: toParams({ status, page }) });
  }

  approve(id: number): Observable<Listing> {
    return this.http.post<Listing>(`${API_URL}/admin/listings/${id}/approve`, null);
  }

  reject(id: number, reason: string): Observable<Listing> {
    return this.http.post<Listing>(`${API_URL}/admin/listings/${id}/reject`, { reason });
  }

  users(search = '', page = 1): Observable<PagedResult<User>> {
    return this.http.get<PagedResult<User>>(`${API_URL}/admin/users`, { params: toParams({ search, page }) });
  }

  setBlocked(id: number, blocked: boolean): Observable<User> {
    return this.http.post<User>(`${API_URL}/admin/users/${id}/${blocked ? 'block' : 'unblock'}`, null);
  }

  setRole(id: number, role: UserRole): Observable<User> {
    return this.http.put<User>(`${API_URL}/admin/users/${id}/role`, { role });
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/admin/users/${id}`);
  }

  stats(): Observable<Stats> {
    return this.http.get<Stats>(`${API_URL}/admin/stats`);
  }
}

/** Прибирає порожні значення, щоб не слати ?city=&minPrice= */
function toParams(obj: object): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(obj)) {
    if (value !== undefined && value !== null && value !== '') params = params.set(key, String(value));
  }
  return params;
}

/** Текст помилки з ProblemDetails / ValidationProblemDetails бекенду. */
export function errorText(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) return 'Невідома помилка';
  if (err.status === 0) return 'Сервер недоступний. Запустіть AutoMarket.Api (http://localhost:5299)';
  if (err.status === 429) return 'Забагато спроб, зачекайте хвилину';
  const body = err.error;
  if (body?.errors) return Object.values(body.errors as Record<string, string[]>).flat().join(' ');
  return body?.detail ?? body?.title ?? `Помилка ${err.status}`;
}
