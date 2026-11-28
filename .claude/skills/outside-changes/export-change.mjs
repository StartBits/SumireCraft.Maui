// Writes a change from outside -- a commit fetched but not checked out -- to files a reviewer can
// read, without anything in it running and without the one who runs this reading it first.
//
//   node .claude/skills/outside-changes/export-change.mjs --head <sha> --base <ref> --out <dir>
//
// <sha> is the full SHA of the commit to read (from `git rev-parse FETCH_HEAD` after
// `git fetch origin pull/<n>/head`), <ref> what it is to be merged into (`origin/main`), and <dir>
// a new, empty folder outside the repository, in the scratchpad. It works on the repository this
// script sits in, wherever it is run from.
//
// It writes, into <dir>:
//   about.txt    the commits involved, and the commit and state of the working tree
//   log.txt      the change's commits, messages and authors included, as git stores them
//   diff.txt     the change against where it branched from <ref>
//   index.txt    one line per changed path: its number, status, mode and the path, quoted, with
//                every character outside printable ASCII escaped
//   <n>.txt      each changed file whole, as the change has it, byte for byte, numbered as listed
//   refused.txt  when it stops on paths or entries, which ones and why, escaped as in index.txt
//
// What it prints is counts alone. Nothing the change chose -- a path, a message -- reaches the one
// who runs it, so that what reads it first is a reviewer with no shell. It stops, with exit code 1,
// on anything a person should look at instead: a path that holds anything but letters, digits, `.`,
// `_`, `-` and `/`, or starts with `-`; an entry that is not a plain file or a symbolic link; or a
// file written at a size other than git's.
//
// git is run without a shell, each argument its own, so no path or revision is ever read as a
// command; and with external diff drivers, text conversion, signature checks and the mailmap off,
// so that nothing configured outside git itself runs on the change's content.

import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync, writeFileSync } from "node:fs";
import { isAbsolute, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

const fail = (message) => {
  process.stderr.write(`export-change: ${message}\n`);
  process.exit(1);
};

const args = {};
for (let i = 2; i < process.argv.length; i += 2) {
  const key = process.argv[i];
  const value = process.argv[i + 1];
  if (!["--head", "--base", "--out"].includes(key) || value === undefined) {
    fail("usage: --head <sha> --base <ref> --out <dir>");
  }

  args[key.slice(2)] = value;
}

for (const key of ["head", "base", "out"]) {
  if (!args[key]) {
    fail(`--${key} is missing`);
  }
}

// The repository this script belongs to, three folders up: git runs there, whatever the current
// folder is, so that another repository's configuration is never the one in force.
const root = resolve(fileURLToPath(new URL("../../..", import.meta.url)));

// What git prints is bytes; what it is given is one argument each, never a shell. If it fails, its
// own words are not passed on: they could quote the change.
const git = (...gitArgs) => {
  try {
    return execFileSync("git", ["-c", "core.quotePath=false", ...gitArgs], {
      cwd: root,
      maxBuffer: 1024 * 1024 * 1024,
      stdio: ["ignore", "pipe", "pipe"],
    });
  } catch (error) {
    return fail(`git ${gitArgs[0]} failed with exit code ${error.status ?? "unknown"}`);
  }
};
const gitText = (...gitArgs) => git(...gitArgs).toString("utf8").trim();

// A path as text a reader can trust: quoted, and every character outside printable ASCII spelled
// as an escape, so that one that reorders or hides text shows as what it is.
const show = (path) =>
  JSON.stringify(path).replace(/[^\x20-\x7e]/g, (c) => `\\u${c.charCodeAt(0).toString(16).padStart(4, "0")}`);

if (!/^[0-9a-f]{40}$/.test(args.head)) {
  fail("--head must be a full 40-character SHA");
}

// A ref that starts with a dash would be read as an option, and one with anything but the
// characters a branch name needs is not a ref this script is meant to take.
if (!/^[A-Za-z0-9][A-Za-z0-9._/-]*$/.test(args.base) || args.base.includes("..")) {
  fail("--base must be a plain ref, such as origin/main");
}

if (!isAbsolute(args.out)) {
  fail("--out must be an absolute path");
}

const out = resolve(args.out);
const inside = relative(root, out);
const outside = inside === ".." || inside.startsWith(`..${sep}`) || isAbsolute(inside);
if (!outside) {
  fail("--out must be outside the repository");
}

if (existsSync(out) && readdirSync(out).length > 0) {
  fail("--out must be a new or empty folder");
}

const head = gitText("rev-parse", "--verify", "--end-of-options", `${args.head}^{commit}`);
const base = gitText("rev-parse", "--verify", "--end-of-options", `${args.base}^{commit}`);
const mergeBase = gitText("merge-base", "--end-of-options", base, head);

// Every entry the change touches, with its modes and status, NUL-separated: a path may hold any
// character, a newline included, and only NUL cannot appear in one. Submodules are listed whatever
// the local configuration says, so that one is refused below rather than left out.
const raw = git("diff", "--raw", "-z", "--no-renames", "--no-abbrev", "--ignore-submodules=none", mergeBase, head)
  .toString("utf8");
const fields = raw.split("\0");
if (fields.at(-1) === "") {
  fields.pop();
}

const entries = [];
for (let i = 0; i < fields.length; i += 2) {
  // ":<old mode> <new mode> <old sha> <new sha> <status>", then the path.
  const [, newMode, , newSha, status] = fields[i].slice(1).split(" ");
  entries.push({ path: fields[i + 1], newMode, newSha, status });
}

// A path the change chose may yet reach a command line -- a reviewer may ask for a file at a
// revision -- and what a shell makes of a character differs between bash and PowerShell, so the
// characters a path may hold are listed, not the ones it may not: letters, digits, `.`, `_`, `-`
// and `/`, which every path in this repository keeps to. Nor may it start with `-`, or hold a
// part that is empty, `.` or `..`.
const safe = (path) =>
  /^[A-Za-z0-9._/-]+$/.test(path) &&
  !path.startsWith("-") &&
  !path.split("/").some((part) => part === "" || part === "." || part === "..");

// 100644 and 100755 are files, 120000 a symbolic link (whose content is where it points), 160000
// a submodule and 040000 a tree; a deleted entry has 000000 and nothing to write.
const writable = new Set(["100644", "100755", "120000"]);

const refused = [
  ...entries.filter((entry) => !safe(entry.path)).map((entry) => `path\t${show(entry.path)}`),
  ...entries
    .filter((entry) => entry.status !== "D" && !writable.has(entry.newMode))
    .map((entry) => `mode ${entry.newMode}\t${show(entry.path)}`),
];

mkdirSync(out, { recursive: true });

// Which paths and entries stopped it go to a file, not to the screen: a path is text the change
// chose, and the one running this is not to read it first.
if (refused.length > 0) {
  writeFileSync(join(out, "refused.txt"), `${refused.join("\n")}\n`);
  fail(`stopped on ${refused.length} of ${entries.length} entries; refused.txt names them`);
}

git(
  "log",
  "--no-show-signature",
  "--no-mailmap",
  "--no-color",
  "--format=raw",
  `--output=${join(out, "log.txt")}`,
  `${mergeBase}..${head}`,
);

git(
  "diff",
  "--no-ext-diff",
  "--no-textconv",
  "--no-color",
  "--no-renames",
  "--ignore-submodules=none",
  `--output=${join(out, "diff.txt")}`,
  mergeBase,
  head,
);

const index = [];
let written = 0;
entries.forEach((entry, i) => {
  const number = i + 1;
  if (entry.status === "D") {
    index.push(`${number}\tD\t-\t${show(entry.path)}`);
    return;
  }

  // The blob by its own id, not by path: no filter, conversion or attribute touches it.
  const content = git("cat-file", "blob", entry.newSha);
  const size = Number(gitText("cat-file", "-s", entry.newSha));
  if (content.length !== size) {
    fail(`entry ${number} came out at ${content.length} bytes, git says ${size}`);
  }

  writeFileSync(join(out, `${number}.txt`), content);
  written++;
  const kind = entry.newMode === "120000" ? "link" : entry.newMode;
  index.push(`${number}\t${entry.status}\t${kind}\t${show(entry.path)}`);
});

const expected = entries.filter((entry) => entry.status !== "D").length;
const found = readdirSync(out).filter((name) => /^\d+\.txt$/.test(name)).length;
if (written !== expected || found !== expected) {
  fail(`wrote ${found} files for ${expected} changed entries`);
}

writeFileSync(join(out, "index.txt"), `${index.join("\n")}\n`);

const worktree = gitText("rev-parse", "HEAD");
const clean = gitText("status", "--porcelain") === "";
writeFileSync(
  join(out, "about.txt"),
  [
    `head       ${head}`,
    `base       ${args.base} at ${base}`,
    `branched   ${mergeBase}`,
    `worktree   ${worktree}${clean ? " (clean)" : " (with uncommitted changes)"}`,
    "",
  ].join("\n"),
);

process.stdout.write(`${entries.length} entries, ${written} files written\n`);
