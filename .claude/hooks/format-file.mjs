// PostToolUse hook for Write and Edit. Brings the file Claude just wrote in line with the
// repository's file policy -- whitespace from .editorconfig for C#, then line endings, BOM and
// trailing whitespace for every text file -- so that nobody has to remember to.

import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import {
  hiddenCharacterProblems, normalize, platformNameProblems, readHookInput, readIfExists, repoRelative,
  repoRoot,
} from './policy.mjs';

const input = await readHookInput();
const relative = repoRelative(input.tool_input?.file_path);
const file = relative && path.join(repoRoot, relative);
const before = file && readIfExists(file);
if (!before) {
  process.exit(0);
}

if (relative.endsWith('.cs')) {
  // Whitespace alone: the style rules need the whole solution loaded, which takes far too long
  // here, and the build enforces them anyway.
  spawnSync('dotnet', ['format', 'whitespace', '.', '--folder', '--include', relative, '--verbosity', 'quiet'], {
    cwd: repoRoot,
    timeout: 50_000,
    stdio: 'ignore',
  });
}

const formatted = readIfExists(file);
const normalized = normalize(file, formatted);
if (normalized && !normalized.equals(formatted)) {
  fs.writeFileSync(file, normalized);
}
const changed = !before.equals(readIfExists(file));

const output = {};
const reasons = [];
const problems = platformNameProblems(relative);
if (problems.length > 0) {
  reasons.push(`${relative}: ${problems.join('; ')}. build/Platforms.props routes only exact names, `
    + 'so this file compiles into every target. Rename it.');
}
for (const problem of hiddenCharacterProblems(relative, readIfExists(file))) {
  reasons.push(`${relative}: ${problem}.`);
}
if (reasons.length > 0) {
  output.decision = 'block';
  output.reason = reasons.join('\n');
}
if (changed) {
  output.hookSpecificOutput = {
    hookEventName: 'PostToolUse',
    additionalContext: `${relative} was reformatted after the edit (.editorconfig whitespace, line endings, BOM). `
      + 'Read it again before an edit that depends on exact whitespace.',
  };
}
if (Object.keys(output).length > 0) {
  process.stdout.write(JSON.stringify(output));
}
