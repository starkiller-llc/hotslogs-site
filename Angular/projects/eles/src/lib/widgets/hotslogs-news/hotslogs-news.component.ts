import { Component, OnInit, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { NewsService } from '../../news.service';
import { Observable, tap } from 'rxjs';
import { NewsItem } from '../../models/news-item';

@Component({
  selector: 'lib-hotslogs-news',
  templateUrl: './hotslogs-news.component.html',
  styleUrls: ['./hotslogs-news.component.scss']
})
export class HotslogsNewsComponent implements OnInit, OnChanges {
  @Input() tags = 'none';
  @Input() max: number = null;
  @Output() ready = new EventEmitter<boolean>();
  newsItems$: Observable<NewsItem[]>;

  constructor(private news: NewsService) { }
  ngOnInit(): void {
    this.setNewsItems();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.tags || changes.max) {
      this.setNewsItems();
    }
  }

  private setNewsItems() {
    this.newsItems$ = this.news.get(this.tags, this.max).pipe(tap(r => this.ready.emit(true)));
  }
}
