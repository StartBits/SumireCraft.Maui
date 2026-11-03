// SessionStart hook. Tells Claude when the .NET SDK is not the one the last full code-analysis sweep
// ran on. Files are checked as they are edited, so a file nobody touches stays as clean as it was,
// until a new SDK brings new analyzers that see it differently.
//
// `node .claude/hooks/analysis-sdk.mjs --record` notes the current SDK once a sweep is done (the
// code-analysis skill). The note lives in the git directory, so it is this clone's and never tracked.

import path from 'node:path';
import fs from 'node:fs';
import { spawnSync } from 'node:child_process';
import { readIfExists, repoRoot } from './policy.mjs';

function run(command, args) {
  // Short of the hook's 60 seconds in .claude/settings.json: the first `dotnet` after an SDK is
  // installed can be slow, and the hook should end on its own rather than be cut off.
  return spawnSync(command, args, { cwd: repoRoot, encoding: 'utf8', timeout: 45_000 }).stdout?.trim() ?? '';
}

// `dotnet --version` run here honours global.json, so it names the SDK the build uses.
const version = run('dotnet', ['--version']);
const gitDirectory = run('git', ['rev-parse', '--git-dir']);
if (!version || !gitDirectory) {
  process.exit(0);
}
const note = path.join(path.resolve(repoRoot, gitDirectory), 'sumire-analysis-sdk');

if (process.argv.includes('--record')) {
  fs.writeFileSync(note, `${version}\n`);
  console.log(`Recorded SDK ${version} as swept.`);
  process.exit(0);
}

const swept = readIfExists(note)?.toString('utf8').trim();
if (swept !== version) {
  console.log(`The .NET SDK is now ${version}; the last full code-analysis sweep ran on `
    + `${swept ? swept : 'no SDK on record'}. Tell the maintainer, and offer the full sweep in the `
    + 'code-analysis skill before other code work.');
}
