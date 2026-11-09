---
name: releasing
description: Who holds the repository and the packages (the StartBits organizations on GitHub and nuget.org) and how a maintainer is added, what is set up on GitHub, cutting a release branch, releasing it into main, and making a hotfix to a published version. Use when preparing or publishing a release, for an urgent fix, or when a maintainer joins or leaves. The rules these steps follow are the Git workflow section of CLAUDE.md.
---

# Releasing

The rules -- branch names, merge commits, what merging into `main` means -- are *Git workflow* in
`CLAUDE.md`. What follows is who holds what, then the order to do things in.

## Accounts

The repository belongs to the StartBits organization on GitHub, and the packages to the StartBits
organization on nuget.org. StartBits is the company the library is published under; the
organizations are its, so that no one person's account holds either.

A maintainer is a person with an account of their own on each site, made a member of both
organizations:

- **GitHub.** Write or Maintain on this repository, never Admin or organization owner, which could
  change the rulesets and the `release` environment. Write at least, because a code owner without
  it is ignored. In `.github/CODEOWNERS`, their name goes beside the maintainer's on the first
  line, `*`, never on a line of its own at the end: a later line wins, and `* @them` there would
  make them the only owner of everything, `.claude/` and `.github/` included. On the lines below
  it, which name the paths *Security* in `CLAUDE.md` lists, their name goes only once it has been
  decided to trust them with those paths: any one owner of a line can approve a change to it.
- **nuget.org.** Collaborator, unless there is a reason for administrator, which can change the
  organization's Trusted Publishing policy. No personal API key: packages are published by
  `release.yml` alone.

Then look again at three things in *What is set up on GitHub*: the review ruleset's bypass,
which goes once there is a second code owner, the creation ruleset's bypass, which stays, so that
release branches are still cut by the organization's owner and not by the new maintainer, and the
`release` environment's required reviewers, where *Prevent self-review* goes on once there is a
second one.

Trusted Publishing ties the nuget.org organization's policy to the member who created it, and
`NUGET_USER` names that member's nuget.org user name. If they leave the organization the policy
stops working, so a remaining administrator makes it again, with everything the comment at the top
of `release.yml` asks for -- the `release` environment above all -- and `NUGET_USER` changes with
it.

Anyone else contributes by a pull request from a fork, which the rulesets hold for a code owner.

### What is set up on GitHub

The StartBits organization's own settings: two-factor authentication required of every member,
members' base permission at **No permission** so that each repository grants its own, and only
owners creating repositories. If they are not in place, walk the maintainer through them first. The
maintainer is its one owner; their two-factor recovery codes are what stand between a lost phone
and a lost organization, so check they are kept somewhere safe.

The repository is guarded on GitHub's side too, which nothing in Claude Code can do for commits
made elsewhere (*Security* in `CLAUDE.md`). If one of these is missing, walk the maintainer through
it:

- **Rulesets.** On `main` and `release/**`, two of them. The first, with no one on its bypass
  list: changes only by pull request, the CI check passing, no force-push, no deletion. Its
  check is not required when a branch is created (*Do not require status checks on creation*),
  or no release branch could be pushed (step 1 of *A release*). The second: a code owner's
  review (`.github/CODEOWNERS`), which a push after the approval dismisses. On `release/**`
  alone, a third: *Restrict creations*, with **Organization admin** on its bypass list in
  **always** mode, since a branch is created by a push and not by a pull request. A release
  branch can be deleted by no one (step 5 of *A release*), so only the organization's owner
  cuts one. On tags `v*`, a fourth: no update and no deletion, so a published version's tag
  always points where it did.

  The review is a ruleset of its own because of its bypass. GitHub never counts the author's
  own approval, and Claude opens pull requests from the maintainer's account, so while the
  maintainer is the only code owner the second ruleset needs a bypass, in **pull requests
  only** mode. A bypass lifts every rule of its ruleset; kept apart, it lifts the review and
  leaves CI and the ban on direct pushes standing. The repository belongs to the StartBits
  organization, where a bypass names a role rather than a person: **Organization admin**, held
  by the maintainer alone. Anyone given that role later can skip the review and cut a release
  branch too, so look at both bypasses again whenever an owner is added. Take the review's
  away when a second code owner joins; the creation ruleset's stays.
- **The `release` environment**, which `release.yml`'s publish job runs in: the maintainer as
  required reviewer, deployments from `main` only, and `NUGET_USER` as its secret rather than the
  repository's. It holds only if the nuget.org policy names the environment too (the comment at
  the top of `release.yml`). Leave *Prevent self-review* off: the merge that starts the run is
  the maintainer's, and while they are its only reviewer, no one else could approve it.
- **CODEOWNERS.** Once pushed, open `.github/CODEOWNERS` on GitHub: a line it cannot use is
  marked there.
- **Actions**, in the organization's settings so that every repository there inherits them:
  only actions by GitHub, verified creators and `NuGet/login`; full-SHA pinning required;
  approval before any outside contributor's pull request runs a workflow, every time and not
  only the first.
- **Security.** Secret scanning with push protection, the dependency graph, Dependabot alerts,
  and private vulnerability reporting. Dependabot *security updates* stay off, though GitHub
  suggests them: their pull requests raise NuGet packages, which is a release.
- **Dependabot version updates** for `github-actions` alone, in `.github/dependabot.yml`: the
  workflows sit outside the package paths, so its pull requests can go to `main`. Not for NuGet:
  the .NET MAUI version is a minimum chosen on purpose, and raising it is a release (*Packages* in
  `CLAUDE.md`). Dependabot alerts still cover NuGet.

## A release

1. Cut `release/x.y` from `main`, and raise `build/Version.props` there in the first commit: the
   patch for fixes alone, the minor for a feature, the major only for a breaking change. Raise
   `SumireCoreVersion` too if Core will change (*Packages* in `CLAUDE.md`, and the comment in
   `build/Version.props`). In the same commit, open a `## [x.y.z]` section at the top of
   `CHANGELOG.md`, with no date yet, and a link line for it at the bottom, above the existing ones,
   comparing with the last tag
   (`[1.1.0]: https://github.com/startbits/SumireCraft.Maui/compare/v1.0.0...v1.1.0`).

   Then push the branch with that commit on it. The ruleset on `release/**` requires CI on
   everything but a branch's creation. If the push is refused with
   `Required status check "build" is expected`, its *Do not require status checks on creation*
   has been turned off, and turning it back on is the maintainer's. In 1.1, passing CI on the
   same commit on a `try/` branch first did not let the push through; only that setting did.

   Nothing has checked the first commit at that point, so run CI by hand on the new branch
   (`gh workflow run ci.yml --ref release/x.y -f base=<main's SHA>`), which checks the versions as
   the final pull request will. No one reviews it either until that pull request into `main`, so
   read it there with the rest.
2. Each change is a `feature/` or `fix/` branch cut from the release branch, reviewed, and merged
   back into it by pull request. Each one that an app author would notice adds its line to that
   section. Repeat.
3. If what goes in outgrows the number raised in step 1 -- a feature arriving in a release of
   fixes -- raise the number again before the pull request into `main`, and the section's heading
   and its link line with it, so that the heading still resolves.
4. Run the UI tests on the release branch in Debug, then Release, then Consumer, every test green
   in all three: CI does not run them, and the merge publishes. Those three run on Windows and
   Android. iOS and Mac Catalyst run on the Mac only when the release holds a change that could
   behave differently there. Then date the section
   (`## [1.1.0] - 2027-04-15`), the day the pull request is opened: the merge day
   is not known yet, and a day or two apart matters less than a section left undated. If Core
   stays as it was, say so in the heading (`## [1.1.0] - 2027-04-15 (Controls only)`). Then open a
   pull request from the release branch into `main`. Its checks fail unless the version has gone up
   and Core is numbered right.
5. The maintainer merges, and the release workflow tags and publishes. The release branch stays,
   but it is no longer open: nothing more goes into it, and the tag is what that release is found
   by from then on. Release branches are kept because the ruleset that forbids deleting them also
   guards `main`, and is never switched off to make a deletion pass. Once more than one major
   version takes fixes, release branches start to matter after their release, and which to keep is
   decided again then.

## A change outside the packages

For the README, `CLAUDE.md`, `.claude/`, the workflows, the sample or the tests, when nothing on
the release workflow's list of package paths changes.

1. Cut `feature/<what>` or `fix/<what>` from `main`.
2. Open a pull request into `main`. Its checks skip the version, since no package changes; if they
   ask for one, something on the list was touched after all, and the change belongs in a release
   branch.
3. Once it is merged, merge `main` into the open release branch, if there is one.

The README describing something not yet published is not such a change: it goes with that change
through the release branch.

Dependabot's weekly pull request is such a change too, one that arrives by itself on a
`dependabot/` branch it names (`.github/dependabot.yml`). It is known by its author,
`dependabot[bot]`, not by that name, which anyone who can push could give a branch. It did not
come from the conversation, so it is read like any outside change: `security-review` joins, and
each new SHA is checked against its action's own repository (*Security* in `CLAUDE.md`). Its CI
proves less than it seems: `download-artifact` and `NuGet/login` run only in `release.yml`'s
publish job, which no pull request starts, so for those two read the release notes between the
versions, and let a major version wait for a release that can afford to find out. The maintainer
approves it as a code owner -- it is not theirs, so the approval counts -- rather than merging
past the review. Then step 3 above.

A major version waits by being pointed at the open release branch
(`gh pr edit <n> --base release/x.y`), so that a hotfix from `main` still publishes with the
version that has published before. Point `upload-artifact` and `download-artifact` together, as
`.github/dependabot.yml` merges them together. Changing the base starts no CI (`ci.yml` does not
run on `edited`), so its check stays the run against `main`; in 1.1, `@dependabot rebase` brought
no new one. Before it is merged into the release branch, try the upload and the download together
(*Trying the flow*), and approve it then: the pull request may stay open for weeks, and a push to
it in that time dismisses an earlier approval anyway. Once it is merged, step 3 does not apply;
the next pull request into the release branch builds it with the rest.

## A hotfix

Only when the next release cannot wait; a fix that can goes through the release branch.

1. Cut `hotfix/x.y.z` from `main`, named after the version it publishes.
2. Fix it on that branch itself -- no branches under it -- and raise `build/Version.props` there.
   Add its dated `## [x.y.z]` section and link line to `CHANGELOG.md` as a release does.
3. Open a pull request into `main`. The same checks apply, and merging publishes it. Delete the
   hotfix branch, which no ruleset keeps; the tag stays.
4. If a release branch is open, merge `main` into it so the fix is not lost. `build/Version.props`
   conflicts, because both raised it:
   - `SumireVersion`: keep the release branch's, unless it is the number the hotfix just took; then
     move it to the next one.
   - `SumireCoreVersion`: if the release branch has changed Core, the release's number. If it
     has not, the hotfix's Core number, since the merged Core is the hotfix's.

   `CHANGELOG.md` conflicts too, because both added a section. Keep both sections, the release
   branch's above the hotfix's, and both link lines in the same order; the release's link now
   compares with the hotfix's tag. If `SumireVersion` moved to the next number, move the release's
   heading and link line with it.

## Trying the flow

Workflows are tried on a `try/<what>` branch (`try/release-flow`), which is never merged and is
deleted afterwards, and run there by hand (`workflow_dispatch`). Check before each run that
nothing in it can publish from that branch, and push no tag: publishing to nuget.org cannot be
undone.

`download-artifact` runs only in `release.yml`'s publish job, which no run from a `try/` branch
reaches. To try a new version of it with `upload-artifact`, put the pull requests' changes on the
`try/` branch and add a job of its own there, after the build job, whose `permissions` hold
`contents: read` and nothing more: no `environment`, no `id-token`, no tag, no push. It downloads
`nupkg` with the step the publish job uses and lists what arrived. Never edit the publish job
itself, not even on a `try/` branch: it can write tags, and the tag ruleset lets no one delete one.
