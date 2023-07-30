import { DefaultUrlSerializer, UrlTree } from '@angular/router';
import * as _ from 'lodash-es';

export class CaseInsensitiveUrlSerializer extends DefaultUrlSerializer {

  constructor() {
    super();
  }

  parse(url: string): UrlTree {
    const rc = super.parse(url);

    if (rc.root.children.primary) {
      rc.root.children.primary.segments.forEach(x => x.path = x.path.toLowerCase());
    }

    return rc;
  }

  serialize(tree: UrlTree): string {
    if (tree.root.children.primary) {
      tree.root.children.primary.segments.forEach(x => x.path = x.path.toLowerCase());
    }

    const rc = super.serialize(tree);

    return rc;
  }
}
