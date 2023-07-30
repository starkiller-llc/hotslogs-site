import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { FileUploader } from 'ng2-file-upload';
import { Subscription } from 'rxjs';
import { HlLocalService } from '../../modules/shared/services/hl-local.service';

const URL = '/api/mig/Default/upload';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent implements AfterViewInit, OnDestroy, OnInit {
  @ViewChildren('items') items: QueryList<ElementRef>;

  uploader: FileUploader;
  hasBaseDropZoneOver: boolean;
  hasAnotherDropZoneOver: boolean;
  response: string;
  subs: Subscription[] = [];

  constructor(public title: Title, loc: HlLocalService, private route: ActivatedRoute) {
    title.setTitle('Upload Replays');
    this.subs.push(loc.change$.subscribe(() => {
      title.setTitle(loc.get('UploadTitle'));
    }));
    this.uploader = new FileUploader({
      url: URL,
      additionalParameter: { foo: 'bar' },
      autoUpload: true,
    });

    this.hasBaseDropZoneOver = false;
    this.hasAnotherDropZoneOver = false;

    this.response = '';

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      item['myResult'] = JSON.parse(response);
      const idx = this.uploader.queue.indexOf(item);
      const itm = this.items.find(r => {
        const rc = r.nativeElement.classList.contains(`item-${idx}`);
        return rc;
      })
      const block = idx % 5 === 0 ? 'center' : 'nearest';
      itm?.nativeElement.scrollIntoView({ behavior: 'smooth', block } as ScrollIntoViewOptions);
    };

    this.uploader.response.subscribe((res, ...rest) => {
      this.response = res;
    });
  }

  ngOnInit() {
    this.subs.push(this.route.queryParamMap.subscribe(r => {
      const key = r.keys.find(r => r.toLowerCase() === 'eventid');
      if (key) {
        this.uploader.options.additionalParameter.eventId = r.get(key);
      }
    }));
  }

  ngAfterViewInit(): void {
    this.subs.push(this.items.changes.subscribe(r => {
      this.items.first.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'center' } as ScrollIntoViewOptions);
    }));
  }

  ngOnDestroy() {
    this.subs.forEach(r => r.unsubscribe());
  }

  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  public fileOverAnother(e: any): void {
    this.hasAnotherDropZoneOver = e;
  }
}
