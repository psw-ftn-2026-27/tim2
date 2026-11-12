import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { UploadedImageDto } from '../../shared/api/uploaded-image';

@Injectable({ providedIn: 'root' })
export class Images {
  private readonly http = inject(HttpClient);

  upload(file: File): Promise<UploadedImageDto> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<UploadedImageDto>('/api/images', body));
  }
}
