---
name: squash
description: Fold the repository's history back into its single "First Commit" and force-push it. Only when the maintainer asks for it -- "squash", "the usual squash", /squash, or the same in the maintainer's native language -- never on your own initiative.
---

# Squash into First Commit

Until version tags start, the history on GitHub is kept to a single commit, `First Commit`. The
repository is used first as an off-machine backup, so what matters is that the latest snapshot is
there, not the history behind it. Claude does the squash; the maintainer does not.

## Steps

1. Look before folding. Run `git status --short` and `git diff --stat HEAD`, and check that nothing
   untracked or half-finished is about to be committed. Ask if anything looks out of place.
2. If local commits have piled up since `First Commit`, go back to the root first, keeping the work:

   ```powershell
   git reset --soft (git rev-list --max-parents=0 HEAD)
   ```

3. Fold and push. `--reset-author` makes the commit the current maintainer's, as the local git
   config names them, and dates it now; amending alone would keep whoever wrote the commit before.
   The `Co-Authored-By` line is a second `-m`, with the model named as the current attribution
   instructions say.

   ```powershell
   git add -A
   git commit --amend --reset-author -m "First Commit" -m "Co-Authored-By: Claude <model> <noreply@anthropic.com>"
   git push --force-with-lease
   ```

   The commit runs `.claude/hooks/check-commit.mjs` first, which can take a minute when code changed.
   If it blocks the commit, fix what it lists and run the step again; skip it only when the
   maintainer says so. The hook runs before the whole command does, so it sees the tree as it is
   then: anything the same command writes before `git commit` goes unchecked. Make files in one
   command and commit in the next.

   A blocked commit stops the `git add -A` in the same command too. Run again from `git add -A`,
   not from `git commit`: the commit alone amends nothing but the date, and that is easy to miss
   until after the push. Check that `git status --short` is empty before pushing.

4. Confirm afterwards: `git fetch origin`, then check that `git log --oneline origin/main` shows the
   one commit and `git status` is clean.

The author is the maintainer as a person, set in the repository's local git config (`git config
user.name`, `git config user.email`), never the company the packages are published under. Use the
account's no-reply address (`<id>+<login>@users.noreply.github.com`), which GitHub links to the
account whatever name the commit gives. Like the maintainer's language, it is not written here:
it belongs to whoever is maintaining. Before folding, look at the two values: an address that is
not a no-reply one, or a name that is not the maintainer's, is asked about, never changed by
Claude.

## Do not fetch before pushing

`--force-with-lease` compares against the last remote state this clone saw. Without a fetch, a
commit pushed from elsewhere makes the push fail instead of being overwritten. Fetch only after
the push, to check it.

## Commits made on another machine

The maintainer sometimes commits on GitHub from another machine, and always says so beforehand. Only
then: `git fetch origin`, `git rebase origin/main`, then fold as above. Do not check the remote for
such commits unprompted.

That other machine must not `pull` after a squash -- it would merge the discarded history back.
It takes the new history with `git fetch origin` and `git reset --hard origin/main`.

## Periods with a different rhythm

The maintainer sometimes asks for something else for a while, and it holds until they lift it:

- **Do not commit.** Changes stay in the working tree so the maintainer can read the diff. The
  squash is then the only commit.
- **Commit and push each step.** Pushes are fast-forward, so no force is needed; the steps are
  folded with a squash when the work is done.

Otherwise, commit locally after each change that builds and passes its tests, with a meaningful
message; the squash folds them.

## When this ends

A squash force-pushes `main`, which strands any branch cut from it and, later, any tag. This
workflow ends with the last squash before the first release branch is cut. The `releasing` skill
says how the switch goes, and that this skill is deleted after the first release.
