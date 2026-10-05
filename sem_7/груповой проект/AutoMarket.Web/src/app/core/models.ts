/** Типи, що відповідають DTO бекенду (AutoMarket.Api/Dtos). */

export type ListingStatus = 'Pending' | 'Active' | 'Rejected' | 'Deleted';
export type ListingSort = 'Newest' | 'Oldest' | 'PriceAsc' | 'PriceDesc' | 'Popular';
export type UserRole = 'User' | 'Admin';

export interface Listing {
  id: number;
  title: string;
  description: string;
  price: number;
  city: string | null;
  status: ListingStatus;
  rejectReason: string | null;
  categoryId: number;
  category: string;
  authorId: number;
  author: string;
  authorPhone: string | null;
  views: number;
  createdAt: string;
  updatedAt: string | null;
  photos: string[];
}

export interface ListingForm {
  title: string;
  description: string;
  price: number;
  city: string;
  categoryId: number;
  photoUrls: string[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface ListingQuery {
  q?: string;
  categoryId?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  city?: string;
  sort?: ListingSort;
  page?: number;
  pageSize?: number;
}

export interface Category {
  id: number;
  name: string;
  activeListings: number;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: number;
  email: string;
  name: string;
  role: UserRole;
}

export interface User {
  id: number;
  email: string;
  name: string;
  phone: string | null;
  role: UserRole;
  isBlocked: boolean;
  createdAt: string;
  listingsCount: number;
}

export interface Stats {
  totalUsers: number;
  blockedUsers: number;
  totalListings: number;
  listingsByStatus: Record<ListingStatus, number>;
  activeByCategory: Category[];
  newListingsLast7Days: number;
  averageActivePrice: number;
}

export const STATUS_LABELS: Record<ListingStatus, string> = {
  Pending: 'На модерації',
  Active: 'Активне',
  Rejected: 'Відхилене',
  Deleted: 'Видалене',
};
