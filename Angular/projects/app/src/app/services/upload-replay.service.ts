import { HttpClient, HttpParams, HttpEventType } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UploadReplayService {

  private url(api: string): string {
    return `/api/UploadReplay/${api}`;
  }

  constructor(private http: HttpClient) { }

  uploadReplay(formData: FormData) {
    const ret = this.http.post(this.url('UploadReplay'), formData, {reportProgress: true, observe: 'events'})
    return ret;
  }

}