import { Injectable } from '@angular/core';
import { BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OverlayService {
  private overlays: Record<string, boolean> = {};
  private pOverlay = new BehaviorSubject<boolean>(false);
  private overlayVisible = false;
  overlay$ = this.pOverlay.asObservable();

  constructor() {
    this.overlay$ = this.pOverlay.pipe(debounceTime(300), distinctUntilChanged());
  }

  setOverlay(overlay: string, flag: boolean) {
    this.overlays[overlay] = flag;
    this.update();
  }

  private update() {
    const o = Object.entries(this.overlays).some(r => r[1]);
    if (this.overlayVisible === o) {
      return;
    }

    this.overlayVisible = o;
    this.pOverlay.next(o);
  }
}
