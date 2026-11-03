// The repository's file policy, shared by the hooks in this folder. The source of truth is
// .editorconfig and the "Files" section of CLAUDE.md; keep the lists here in step with them.

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

export const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');

// Files Visual Studio edits as XML keep a UTF-8 BOM; it re-saves a BOM-less one in the ANSI code page.
const bomExtensions = new Set([
  '.csproj', '.props', '.targets', '.slnx', '.sln', '.xaml', '.resx', '.xml',
  '.appxmanifest', '.manifest', '.plist', '.xcprivacy',
]);

// Text files that must carry no BOM. Anything not listed in either set is left alone.
const plainExtensions = new Set([
  '.cs', '.md', '.json', '.yml', '.yaml', '.config', '.mjs', '.js', '.ps1', '.svg', '.txt',
  '.editorconfig', '.gitignore', '.gitattributes', 'license', 'codeowners',
]);

const skippedFolders = new Set(['bin', 'obj', 'artifacts', 'TestResults', '.git', '.vs', 'node_modules']);

const platformNames = ['Android', 'iOS', 'MacCatalyst', 'MaciOS', 'Windows'];

const bom = Buffer.from([0xef, 0xbb, 0xbf]);

export async function readHookInput() {
  let text = '';
  for await (const chunk of process.stdin) {
    text += chunk;
  }
  return text ? JSON.parse(text) : {};
}

// The path relative to the repository with forward slashes, or null for a file outside it or in
// a build output folder.
export function repoRelative(file) {
  if (!file) {
    return null;
  }
  const relative = path.relative(repoRoot, path.resolve(file));
  if (!relative || relative.startsWith('..') || path.isAbsolute(relative)) {
    return null;
  }
  const segments = relative.split(path.sep);
  if (segments.some((s) => skippedFolders.has(s))) {
    return null;
  }
  return segments.join('/');
}

// The extension, or for a name with none -- .gitignore, LICENSE -- the whole name.
function extensionOf(file) {
  const name = path.basename(file);
  const whole = !name.slice(1).includes('.');
  return (whole ? name : path.extname(name)).toLowerCase();
}

// 'bom', 'plain', or null for a file the policy does not cover.
export function kindOf(file) {
  const extension = extensionOf(file);
  if (bomExtensions.has(extension)) {
    return 'bom';
  }
  return plainExtensions.has(extension) ? 'plain' : null;
}

function decode(bytes) {
  try {
    return new TextDecoder('utf-8', { fatal: true }).decode(bytes);
  } catch {
    return null;
  }
}

function hasBom(bytes) {
  return bytes.length >= 3 && bytes.subarray(0, 3).equals(bom);
}

// What .editorconfig asks of the file, applied: LF, the right BOM, no trailing whitespace outside
// Markdown, a final newline. Returns null when the file is not valid UTF-8 and must not be touched.
export function normalize(file, bytes) {
  const kind = kindOf(file);
  const text = decode(hasBom(bytes) ? bytes.subarray(3) : bytes);
  if (kind === null || text === null) {
    return null;
  }
  let result = text.replace(/\r\n/g, '\n');
  if (extensionOf(file) !== '.md') {
    result = result.replace(/[ \t]+$/gm, '');
  }
  if (result.length > 0 && !result.endsWith('\n')) {
    result += '\n';
  }
  const body = Buffer.from(result, 'utf8');
  return kind === 'bom' ? Buffer.concat([bom, body]) : body;
}

// Breaches that Git cannot fix on its own: a BOM where there should be none or none where there
// should be one, CRLF, or bytes that are not UTF-8.
export function encodingProblems(file, bytes) {
  const kind = kindOf(file);
  if (kind === null) {
    return [];
  }
  const problems = [];
  if (decode(hasBom(bytes) ? bytes.subarray(3) : bytes) === null) {
    problems.push('is not valid UTF-8');
  }
  if (kind === 'bom' && !hasBom(bytes)) {
    problems.push('needs a UTF-8 BOM');
  }
  if (kind === 'plain' && hasBom(bytes)) {
    problems.push('carries a BOM it should not');
  }
  if (bytes.includes('\r\n')) {
    problems.push('has CRLF line endings');
  }
  return problems;
}

// Characters that change what a reader sees without being seen: bidirectional controls, which can
// make code read differently from how it compiles ("Trojan Source"), zero-width characters, a byte
// order mark past the start, the soft hyphen, line and paragraph separators, and the tag
// characters, which spell out text that an editor hides and a language model still reads. Nothing
// in this repository needs any of them.
// Written as numbers rather than as \u escapes, so that this file holds none of them itself: an
// editor or a tool that turns an escape into its character would otherwise hide the list.
// The variation selectors and the Hangul fillers are on it too: the first can carry any bytes behind
// a visible letter, the second can make a name that looks like nothing.
const hiddenRanges = [
  [0x00ad], [0x034f], [0x061c], [0x115f, 0x1160], [0x17b4, 0x17b5], [0x180b, 0x180f],
  [0x200b, 0x200f], [0x2028, 0x202e], [0x2060, 0x2064], [0x2066, 0x2069], [0x3164],
  [0xfe00, 0xfe0f], [0xfeff], [0xffa0], [0xfff9, 0xfffb], [0xe0000, 0xe007f], [0xe0100, 0xe01ef],
];
const backslash = String.fromCharCode(0x5c);
const hex = (code) => `${backslash}u{${code.toString(16)}}`;
const hiddenCharacters = new RegExp(
  `[${hiddenRanges.map(([from, to]) => (to ? `${hex(from)}-${hex(to)}` : hex(from))).join("")}]`, "gu");

export function hiddenCharacterProblems(file, bytes) {
  if (kindOf(file) === null) {
    return [];
  }
  const text = decode(hasBom(bytes) ? bytes.subarray(3) : bytes);
  if (text === null) {
    return [];
  }
  const found = [];
  text.split('\n').forEach((line, index) => {
    for (const match of line.matchAll(hiddenCharacters)) {
      const code = match[0].codePointAt(0).toString(16).toUpperCase().padStart(4, '0');
      found.push(`line ${index + 1}: U+${code}`);
    }
  });
  if (found.length === 0) {
    return [];
  }
  const shown = found.slice(0, 5).join(', ') + (found.length > 5 ? `, and ${found.length - 5} more` : '');
  return [`holds characters a reader cannot see (${shown}); remove them, or ask the maintainer why they are there`];
}

// build/Platforms.props routes platform files by exact, case-sensitive names. A misspelled one is
// routed nowhere and compiles into every target, and Windows never says so.
export function platformNameProblems(relative) {
  const problems = [];
  const segments = relative.split('/');
  const exactName = (name) => platformNames.find((n) => n.toLowerCase() === name.toLowerCase());

  const suffix = /\.([A-Za-z]+)\.cs$/.exec(segments.at(-1));
  if (suffix) {
    const exact = exactName(suffix[1]);
    if (exact && exact !== suffix[1]) {
      problems.push(`the suffix is spelled .${exact}.cs, not .${suffix[1]}.cs`);
    }
    if (suffix[1].toLowerCase() === 'apple') {
      problems.push('code shared by iOS and Mac Catalyst is .MaciOS.cs, not .Apple.cs');
    }
  }

  segments.slice(0, -1).forEach((segment, i) => {
    if (segment.toLowerCase() === 'platforms' && segment !== 'Platforms') {
      problems.push(`the folder is spelled Platforms, not ${segment}`);
    }
    if (i > 0 && segments[i - 1] === 'Platforms') {
      const exact = exactName(segment);
      if (exact && exact !== segment) {
        problems.push(`the folder is spelled Platforms/${exact}, not Platforms/${segment}`);
      }
      if (segment.toLowerCase() === 'apple') {
        problems.push('the folder shared by iOS and Mac Catalyst is Platforms/MaciOS, not Platforms/Apple');
      }
    }
  });
  return problems;
}

// Package versions live in Directory.Packages.props alone.
export function packageVersionProblems(relative, bytes) {
  if (!/\.(csproj|props|targets)$/i.test(relative) || /(^|\/)Directory\.Packages\.props$/i.test(relative)) {
    return [];
  }
  return /<PackageReference\b[^>]*\sVersion\s*=/.test(bytes.toString('utf8'))
    ? ['puts a Version on a PackageReference; versions belong in Directory.Packages.props']
    : [];
}

export function readIfExists(file) {
  try {
    return fs.readFileSync(file);
  } catch {
    return null;
  }
}
