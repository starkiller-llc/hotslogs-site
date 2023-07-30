import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NewsItem } from './models/news-item';

@Injectable({
  providedIn: 'root'
})
export class NewsService {

  constructor(private http: HttpClient) { }

  get(tags: string, maxEntries?: number): Observable<NewsItem[]> {
    let params = new HttpParams()
      .set('tags', tags);

    if (maxEntries) {
      params = params.set('maxEntries', maxEntries.toString());
    }

    return this.http.get<NewsItem[]>('/api/news', { params });
  }
}
