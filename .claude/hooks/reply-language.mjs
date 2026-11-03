// Stop hook. Holds back a reply written in another script than the maintainer's last message, the
// mechanical side of "Language policy" in CLAUDE.md: after a turn of English prompts to a reviewer,
// the report to the maintainer kept coming out in English, however plainly the rule was written.
//
// It knows no language. It compares how much of each text is written in letters other than Latin:
// a maintainer who writes in Japanese, say, and a reply that is nearly all Latin letters do not
// match. A maintainer who writes in English, or in any other Latin-script language, is never
// stopped, and neither is a reply in a different Latin-script language from theirs -- that slip goes
// unseen here.
//
// It reads Claude Code's transcript, whose format is not a published contract. Anything it does not
// recognise lets the reply through: a broken guard must never hold up the work.

import fs from 'node:fs';
import { readHookInput } from './policy.mjs';

// A maintainer is taken to write in a non-Latin script when at least this share of their letters is.
const MAINTAINER_SHARE = 0.5;
// A reply is taken to be in Latin script when less than this share of its letters is not.
const REPLY_SHARE = 0.2;
// Shorter replies are let through: a line of names or paths says nothing about its language.
const MIN_REPLY_LETTERS = 40;
// A shorter message from the maintainer ("OK", "go") is passed over for the one before it.
const MIN_MAINTAINER_LETTERS = 10;

const input = await readHookInput();

// Already sent back once this turn: let the second attempt through rather than loop.
if (input.stop_hook_active) {
  process.exit(0);
}

const entries = readTranscript(input.transcript_path);
if (!entries) {
  process.exit(0);
}

// What Claude wrote in this turn: back to the first entry that is neither Claude's nor a tool's
// result, whatever it is -- the maintainer, a task notification, a skill, this hook sending a reply
// back. Reading further would judge a reply from an earlier turn again.
const reply = [];
let i = entries.length - 1;
for (; i >= 0; i--) {
  const entry = entries[i];
  if (entry.isSidechain) {
    continue;
  }
  if (entry.type === 'assistant') {
    reply.unshift(...textBlocks(entry.message?.content));
  } else if (entry.type === 'user' && !isToolResult(entry)) {
    break;
  }
}
// Newer versions of Claude Code hand the last message to the hook as well, which covers a transcript
// not yet written to the end.
if (typeof input.last_assistant_message === 'string') {
  reply.push(input.last_assistant_message);
}

// The maintainer's language, from the last message of theirs long enough to show it.
let asked = null;
for (; i >= 0 && !asked; i--) {
  const text = maintainerText(entries[i]);
  const share = text && nonLatinShare(text);
  if (share && share.letters >= MIN_MAINTAINER_LETTERS) {
    asked = share;
  }
}

if (!asked || asked.share < MAINTAINER_SHARE) {
  process.exit(0);
}

// Each message on its own: a turn that starts in the maintainer's language and ends in English would
// pass on average.
const slipped = reply.some((text) => {
  const answered = nonLatinShare(stripCode(text));
  return answered.letters >= MIN_REPLY_LETTERS && answered.share < REPLY_SHARE;
});

if (slipped) {
  process.stdout.write(JSON.stringify({
    decision: 'block',
    reason: 'A message to the maintainer in this turn is written in a different script from their last one. '
      + 'Write it again in the maintainer\'s native language (CLAUDE.md, "Language policy"), '
      + 'keeping code, identifiers and paths as they are. English is for what goes into the '
      + 'repository and to subagents, not for the maintainer. A draft meant for someone else -- a '
      + 'commit message, a reply to a user, README text -- stays in English inside a code block or '
      + 'a quote; only the words to the maintainer around it change.',
  }));
}

function readTranscript(file) {
  try {
    return fs.readFileSync(file, 'utf8')
      .split('\n')
      .filter((line) => line.trim())
      .map((line) => {
        try {
          return JSON.parse(line);
        } catch {
          return null;
        }
      })
      .filter(Boolean);
  } catch {
    return null;
  }
}

function textBlocks(content) {
  if (typeof content === 'string') {
    return [content];
  }
  if (!Array.isArray(content)) {
    return [];
  }
  return content.filter((block) => block?.type === 'text' && typeof block.text === 'string').map((block) => block.text);
}

function isToolResult(entry) {
  const content = entry.message?.content;
  return Array.isArray(content) && content.some((block) => block?.type === 'tool_result');
}

// What the maintainer typed in a user entry, or null when the entry is something else: a tool
// result, a task notification, a subagent's exchange, a summary written when the conversation was
// compacted, a hook's or the harness's own insertion.
function maintainerText(entry) {
  if (entry.type !== 'user' || entry.isMeta || entry.isSidechain || entry.isCompactSummary) {
    return null;
  }
  if (entry.origin?.kind && entry.origin.kind !== 'human') {
    return null;
  }
  if (isToolResult(entry)) {
    return null;
  }
  const text = textBlocks(entry.message?.content).join('\n')
    // Tags the harness wraps around what it adds, and text the maintainer pasted from elsewhere,
    // which is in whatever language its source was.
    .replace(/<(system-reminder|task-notification|ide_[a-z_]+|pasted_content|command-[a-z-]+|local-command-[a-z-]+)\b[^>]*>[\s\S]*?<\/\1[^>]*>/g, '')
    .trim();
  return text || null;
}

// Code, paths and links say nothing about the language around them, and a quoted draft is meant for
// someone else.
function stripCode(text) {
  return text
    .replace(/```[\s\S]*?```/g, '')
    .replace(/`[^`\n]*`/g, '')
    .replace(/^\s*>.*$/gm, '')
    .replace(/\]\([^)]*\)/g, ']')
    .replace(/https?:\/\/\S+/g, '')
    .replace(/\b[A-Za-z]:[\\/]\S+/g, '');
}

function nonLatinShare(text) {
  const letters = text.match(/\p{L}/gu) ?? [];
  const nonLatin = letters.filter((letter) => !/\p{Script=Latin}/u.test(letter)).length;
  return { letters: letters.length, share: letters.length === 0 ? 0 : nonLatin / letters.length };
}
