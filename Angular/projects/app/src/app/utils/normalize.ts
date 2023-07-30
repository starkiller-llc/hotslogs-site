export function normalize(s: string, keepCase = true) {
  const rc = s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim();
  return keepCase ? rc : rc.toLowerCase();
}
