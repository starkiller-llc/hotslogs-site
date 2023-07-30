import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse
} from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { dateParser } from '@eles/public-api';

@Injectable()
export class JsonHttpInterceptor implements HttpInterceptor {
  constructor() { }

  intercept(
    httpRequest: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return httpRequest.responseType === "json"
      ? this.handleJsonResponses(httpRequest, next)
      : next.handle(httpRequest);
  }

  private handleJsonResponses(
    httpRequest: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next
      .handle(httpRequest.clone({ responseType: "text" }))
      .pipe(map(event => this.parseResponse(event)));
  }

  private parseResponse(event: HttpEvent<any>): HttpEvent<any> {
    if (event instanceof HttpResponse) {
      // console.log(event.body, JSON.parse(event.body));
    }
    return event instanceof HttpResponse
      ? event.clone({ body: this.parse(event) })
      : event;
  }

  private parse(event: HttpResponse<any>): any {
    const rc = JSON.parse(event.body, dateParser);
    return rc;
  }
}
