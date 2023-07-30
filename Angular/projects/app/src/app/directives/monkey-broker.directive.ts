import { DOCUMENT } from '@angular/common';
import { Directive, Inject, OnInit, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appMonkeyBroker]'
})
export class MonkeyBrokerDirective implements OnInit {
  constructor(
    private _renderer2: Renderer2,
    @Inject(DOCUMENT) private _document: Document
  ) { }

  ngOnInit(): void {
    let script1 = this._renderer2.createElement('script');
    script1.src='https://zam-com.videoplayerhub.com/gallery.js';
    let script2 = this._renderer2.createElement('script');
    script2.text = `
    (function (w, d, s) {
      var script = d.createElement(s), i = (((w.location.search || '').match(/zaf=([^&]*)/)) || [])[1];
      script.setAttribute("async", "async");
      script.src = 'https://zaf.services.zam.com/stable/js/hotslogs.js' + (i ? ('?id=' + i) : '');
      d.getElementsByTagName('head')[0].appendChild(script);
    })(window, document, 'script');

    (function (w, d, s, l, i) {
        w[l] = w[l] || []; w[l].push({
            'gtm.start':
                new Date().getTime(), event: 'gtm.js'
        }); var f = d.getElementsByTagName(s)[0],
            j = d.createElement(s), dl = l != 'dataLayer' ? '&l=' + l : ''; j.async = true; j.src =
                'https://www.googletagmanager.com/gtm.js?id=' + i + dl; f.parentNode.insertBefore(j, f);
    })(window, document, 'script', 'dataLayer', 'GTM-WRH79P9');
    `;

    this._renderer2.appendChild(this._document.body, script1);
    this._renderer2.appendChild(this._document.body, script2);
  }
}
