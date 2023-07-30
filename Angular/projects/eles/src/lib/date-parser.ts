const reISO = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)(?:Z|(\+|-)([\d|:]*))?$/;
const reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;
const reTimespan = /^(\d{2}):(\d{2}):(\d{2})(\.\d{7})?$/;

export const dateParser = (key, value) => {
  if (typeof value === 'string') {
    let a = reISO.exec(value);
    if (a) {
      const rc = new Date(value);
      return rc;
    }
    a = reMsAjax.exec(value);
    if (a) {
      const b = a[1].split(/[-+,.]/);
      const rc = new Date(b[0] ? +b[0] : 0 - +b[1]);
      return rc;
    }
    a = reTimespan.exec(value);
    if (a) {
      const rc = new Date();
      rc.setHours(+a[1]);
      rc.setMinutes(+a[2]);
      rc.setSeconds(+a[3]);
      rc.setMilliseconds(+(a[4] || 0));
      return rc;
    }
  }
  return value;
};
