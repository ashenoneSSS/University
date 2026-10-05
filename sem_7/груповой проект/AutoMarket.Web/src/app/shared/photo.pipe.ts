import { Pipe, PipeTransform } from '@angular/core';
import { API_ORIGIN } from '../core/api';

/** Фото демо-оголошень зберігаються на бекенді як "/images/listings/x.jpg" — додаємо адресу API. */
@Pipe({ name: 'photo' })
export class PhotoPipe implements PipeTransform {
  transform(url: string): string {
    return url.startsWith('/') ? API_ORIGIN + url : url;
  }
}
