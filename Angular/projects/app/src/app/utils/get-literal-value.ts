import { SortDirection } from '@angular/material/sort';

export const getLiteralValue = (s: string | number, dir: SortDirection) => {
  if (!s) {
    return dir === 'desc' ? -100000000 : 100000000;
  }

  if (typeof (s) === 'string') {
    const m = s.match(/\$val:(?<num>.*?)\$/);
    if (m) {
      const val = +m.groups.num;
      return val;
    }
  }

  return s;
};
