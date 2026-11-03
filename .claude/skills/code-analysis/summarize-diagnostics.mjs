// Summarizes what VS Code's getDiagnostics returned for the whole workspace: one line per
// diagnostic in src/, tests/ and samples/, then a count per rule. Generated files under obj/ and
// bin/ are left out. The input is the file the tool result was saved to.
//
//   node .claude/skills/code-analysis/summarize-diagnostics.mjs <saved getDiagnostics output>

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

// The repository is found from where this script sits, so a clone under any name works.
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..', '..');

// The path of a file: URI relative to the repository, with forward slashes, or null for anything
// outside it and for the source-generated documents VS Code reports under URIs of their own.
function repoRelative(uri) {
  if (!uri.startsWith('file:')) {
    return null;
  }
  const relative = path.relative(repoRoot, fileURLToPath(uri));
  if (relative.startsWith('..') || path.isAbsolute(relative)) {
    return null;
  }
  return relative.split(path.sep).join('/');
}

const file = process.argv[2];
if (!file) {
  console.error('Pass the file the getDiagnostics result was saved to.');
  process.exit(1);
}

const entries = JSON.parse(fs.readFileSync(file, 'utf8'));
const counts = new Map();
let total = 0;

for (const entry of entries) {
  const relative = repoRelative(entry.uri);
  if (!relative || !/^(src|tests|samples)\//i.test(relative) || /\/(obj|bin)\//i.test(relative)) {
    continue;
  }
  for (const d of entry.diagnostics ?? []) {
    total++;
    const line = d.range.start.line + 1;
    console.log(`${relative}:${line} ${d.severity} ${d.code ?? '-'} ${d.message}`);
    const key = `${d.severity} ${d.code ?? '-'}`;
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }
}

console.log(total === 0 ? 'No diagnostics.' : '');
for (const [key, count] of [...counts].sort()) {
  console.log(`${String(count).padStart(4)}  ${key}`);
}
