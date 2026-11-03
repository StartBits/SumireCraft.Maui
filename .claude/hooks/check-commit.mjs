// PreToolUse hook for Bash and PowerShell. Before a `git commit`, checks every changed file
// against the repository's file policy, checks C# whitespace, runs the unit tests when code
// changed, and builds the UI tests and the sample when they or what they use changed. A build that
// prints a warning fails like one that does not build. A failure blocks the commit and tells Claude
// why.
//
// Run by hand, `node .claude/hooks/check-commit.mjs --all` checks the file policy across every
// tracked file instead, and runs nothing else.

import path from 'node:path';
import { spawnSync } from 'node:child_process';
import {
  encodingProblems, hiddenCharacterProblems, packageVersionProblems, platformNameProblems, readHookInput,
  readIfExists, repoRelative, repoRoot,
} from './policy.mjs';

const all = process.argv.includes('--all');

if (!all) {
  const input = await readHookInput();
  const command = input.tool_input?.command ?? '';
  const commit = /\bgit\b([^;&|\n]*?)\bcommit\b/.exec(command);
  if (!commit || command.includes('SUMIRE_SKIP_COMMIT_CHECK') || !targetsThisRepository(input.cwd, commit[1])) {
    process.exit(0);
  }
}

// Whether the commit lands in this repository. `git -C <path>` decides when it is given. Otherwise
// the working directory does: inside the repository, or above it -- a session opened on the parent
// folder reaches in with `cd` -- but not beside it.
function targetsThisRepository(cwd, gitOptions) {
  const base = cwd ?? process.cwd();
  const option = /(?:^|\s)-C\s+(?:"([^"]+)"|'([^']+)'|(\S+))/.exec(gitOptions);
  const directory = option ? path.resolve(base, option[1] ?? option[2] ?? option[3]) : path.resolve(base);
  const relative = path.relative(directory, repoRoot);
  const inside = repoRelative(path.join(directory, 'x')) !== null;
  const above = !option && !relative.startsWith('..') && !path.isAbsolute(relative);
  return inside || above;
}

function run(command, args, timeout) {
  return spawnSync(command, args, { cwd: repoRoot, encoding: 'utf8', timeout, maxBuffer: 64 * 1024 * 1024 });
}

// Changed and untracked files, as the commit will see them once `git add -A` has run. The check runs
// before the command, so the index may not hold them yet.
function changedFiles() {
  const entries = run('git', ['status', '--porcelain=v1', '-z', '--untracked-files=all']).stdout.split('\0');
  const files = [];
  for (let i = 0; i < entries.length; i++) {
    const entry = entries[i];
    if (entry.length < 4) {
      continue;
    }
    if (entry[0] === 'R' || entry[0] === 'C') {
      i++;
    }
    files.push(entry.slice(3));
  }
  return files;
}

const files = (all ? run('git', ['ls-files', '-z']).stdout.split('\0') : changedFiles())
  .map((f) => repoRelative(path.join(repoRoot, f)))
  .filter((f) => f !== null);

const problems = [];
for (const relative of files) {
  const bytes = readIfExists(path.join(repoRoot, relative));
  if (!bytes) {
    continue;
  }
  for (const problem of [
    ...encodingProblems(relative, bytes),
    ...hiddenCharacterProblems(relative, bytes),
    ...platformNameProblems(relative),
    ...packageVersionProblems(relative, bytes),
  ]) {
    problems.push(`${relative}: ${problem}`);
  }
}

if (all) {
  console.log(problems.length > 0 ? problems.join('\n') : `${files.length} files checked, no problems.`);
  process.exit(problems.length > 0 ? 1 : 0);
}

const sources = files.filter((f) => f.endsWith('.cs') && readIfExists(path.join(repoRoot, f)));
if (sources.length > 0) {
  const format = run('dotnet', ['format', 'whitespace', '.', '--folder', '--verify-no-changes', '--include', ...sources], 120_000);
  if (format.status !== 0) {
    problems.push('C# whitespace differs from .editorconfig. Fix it with: '
      + `dotnet format whitespace . --folder --include ${sources.join(' ')}`);
  }
}

function tailOf(result) {
  return `${result.stdout ?? ''}${result.stderr ?? ''}`.trim().split('\n').slice(-40).join('\n');
}

// The warnings a build printed. A project that is up to date skips compiling and prints none, so
// every build here is --no-incremental: an earlier build of the same code, by hand or by Visual
// Studio, would otherwise hide what it warned about. IDs are not all capitals (xUnit1031, CsWinRT1030).
function warningsOf(result) {
  const lines = `${result.stdout ?? ''}${result.stderr ?? ''}`.split('\n');
  return [...new Set(lines.filter((l) => /: warning [A-Za-z]+\d+:/.test(l)).map((l) => l.trim()))];
}

// Builds a project from scratch; true when it built without a warning.
function buildClean(what, project, extra, timeout) {
  const build = run('dotnet', ['build', project, '--no-incremental', ...extra], timeout);
  if (build.status !== 0) {
    problems.push(`Building ${what} failed:\n${tailOf(build)}`);
    return false;
  }
  const warnings = warningsOf(build);
  if (warnings.length > 0) {
    problems.push(`Building ${what} printed warnings, and the build stays at 0:\n${warnings.join('\n')}`);
    return false;
  }
  return true;
}

const shared = (f) => /^build\/|^(Directory\.[^/]+|global\.json|\.editorconfig)$/.test(f);
const unitTests = 'tests/SumireCraft.Maui.UnitTests/SumireCraft.Maui.UnitTests.csproj';

// The timeouts below add up to less than the hook's own in .claude/settings.json, so that a slow
// step fails here, blocking the commit, rather than the whole hook being cut off.
if (problems.length === 0 && files.some((f) => shared(f) || /^(src|tests\/SumireCraft\.Maui\.UnitTests)\//.test(f))
  && buildClean('the libraries and the unit tests', unitTests, [], 300_000)) {
  const test = run('dotnet', ['test', '--project', unitTests, '--no-build'], 300_000);
  if (test.status !== 0) {
    problems.push(`The unit tests failed:\n${tailOf(test)}`);
  }
}

if (problems.length === 0 && files.some((f) => shared(f) || /^tests\/SumireCraft\.Maui\.UITests\//.test(f))) {
  buildClean('the UI tests', 'tests/SumireCraft.Maui.UITests/SumireCraft.Maui.UITests.csproj', [], 180_000);
}

// The sample is built for Windows alone, and only on Windows: the other platforms are slower to
// build, and the full build covers them. It is built when the libraries change too, because it uses
// them: a renamed member or a new [Obsolete] shows there first. Directory.Build.props is where its
// target framework is written down.
const windows = /<SumireWindowsTargetFramework>([^<]+)</.exec(readIfExists(path.join(repoRoot, 'Directory.Build.props'))?.toString('utf8') ?? '');
if (problems.length === 0 && process.platform === 'win32' && windows
  && files.some((f) => shared(f) || /^(src|samples)\//.test(f))) {
  buildClean(`the sample for ${windows[1]}`, 'samples/SumireCraft.Maui.Sample/SumireCraft.Maui.Sample.csproj',
    ['-f', windows[1]], 480_000);
}

if (problems.length > 0) {
  console.error([
    'Commit blocked by .claude/hooks/check-commit.mjs:',
    ...problems.map((p) => `- ${p}`),
    'Fix these and commit again. Skip the check only when the maintainer asks for it, by putting '
      + 'SUMIRE_SKIP_COMMIT_CHECK in the command.',
  ].join('\n'));
  process.exit(2);
}
