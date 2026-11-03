---
name: releasing
description: Who holds the repository and the packages (the StartBits organizations on GitHub and nuget.org) and how a maintainer is added, cutting a release branch, releasing it into main, making a hotfix to a published version, and the switch from the squash workflow to branches. Use when preparing or publishing a release, for an urgent fix, or when a maintainer joins or leaves. The rules these steps follow are the Git workflow section of CLAUDE.md.
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

Then look again at what step 5 of *The switch from squashing* set up: the review ruleset's bypass,
which goes once there is a second code owner, and the `release` environment's required reviewers.

Trusted Publishing ties the nuget.org organization's policy to the member who created it, and
`NUGET_USER` names that member's nuget.org user name. If they leave the organization the policy
stops working, so a remaining administrator makes it again, with everything the comment at the top
of `release.yml` asks for -- the `release` environment above all -- and `NUGET_USER` changes with
it.

Anyone else, once the repository is public, contributes by a pull request from a fork, which the
rulesets hold for a code owner.

## The switch from squashing

The `squash` skill keeps the history to one commit, and a squash force-pushes `main`. Once a
branch has been cut from `main`, a force-push would leave that branch on a history that no longer
exists, so the squash ends where the first release branch begins:

1. Run the UI tests in Debug, then Release, then Consumer (the `ui-tests` and `consumer-build`
   skills), every test green in all three. Then the last squash, on the maintainer's word as
   always.
2. Cut `release/1.0` from `main`. From here on, nothing is squashed or force-pushed.
3. What 1.0.0 still needs goes in by `feature/` and `fix/` branches, as in any release.
4. Try the whole flow on a `try/` branch, which is never merged, before the first pull request
   into `main`, because that merge publishes (see *Trying the flow*).
5. Turn the workflows' automatic triggers back on, by a `feature/` branch into `release/1.0`.
   Without them the pull request into `main` is not checked and its merge publishes nothing.
   Before the pull request into `main`, the maintainer makes the repository public -- the README's
   and the PACKAGE-READMEs' links into GitHub lead nowhere until then -- and only after that sets
   up Trusted Publishing as the comment at the top of `release.yml` says: the policy on nuget.org,
   with `release` as its environment, and the `NUGET_USER` secret. Walk them through both then. A
   policy made while the repository is still private lapses after seven days without a publish, so
   making it public first avoids that.

   The StartBits organization's own settings do not wait for any of this, and if they are not in
   place yet, walk the maintainer through them first: two-factor authentication required of every
   member, members' base permission at **No permission** so that each repository grants its own,
   and only owners creating repositories. The maintainer is its one owner; their two-factor
   recovery codes are what stand between a lost phone and a lost organization, so check they are
   kept somewhere safe.

   Public, the repository can also be guarded on GitHub's side, which nothing in Claude Code can do
   for commits made elsewhere (*Security* in `CLAUDE.md`). Walk the maintainer through these in the
   repository's settings, after the last squash, since a squash force-pushes `main`:

   - **Rulesets.** On `main` and `release/**`, two of them. The first, with no one on its bypass
     list: changes only by pull request, the CI check passing, no force-push, no deletion. The
     second: a code owner's review (`.github/CODEOWNERS`). On tags `v*`, a third: no update and no
     deletion, so a published version's tag always points where it did.

     The review is a ruleset of its own because of its bypass. GitHub never counts the author's
     own approval, and Claude opens pull requests from the maintainer's account, so while the
     maintainer is the only code owner the second ruleset needs a bypass, in **pull requests
     only** mode. A bypass lifts every rule of its ruleset; kept apart, it lifts the review and
     leaves CI and the ban on direct pushes standing. The repository belongs to the StartBits
     organization, where a bypass names a role rather than a person: **Organization admin**, held
     by the maintainer alone. Anyone given that role later can skip the review too, so look at the
     bypass again whenever an owner is added, and take it away when a second code owner joins.
   - **The `release` environment**, which `release.yml`'s publish job runs in: the maintainer as
     required reviewer, deployments from `main` only, and `NUGET_USER` as its secret rather than the
     repository's. It holds only if the nuget.org policy names the environment too (the comment at
     the top of `release.yml`).
   - **CODEOWNERS.** Once pushed, open `.github/CODEOWNERS` on GitHub: a line it cannot use is
     marked there.
   - **Actions**, in the organization's settings so that every repository there inherits them:
     only actions by GitHub, verified creators and `NuGet/login`; full-SHA pinning required;
     approval before a first-time contributor's pull request runs any workflow.
   - **Security.** Secret scanning with push protection, Dependabot alerts, and private
     vulnerability reporting.
   - **Dependabot version updates** for `github-actions` alone, in a `.github/dependabot.yml`
     written then: the workflows sit outside the package paths, so its pull requests can go to
     `main`. Not for NuGet: the .NET MAUI version is a minimum chosen on purpose, and raising it is
     a release (*Packages* in `CLAUDE.md`). Dependabot alerts still cover NuGet.
6. The pull request from `release/1.0` into `main` publishes 1.0.0. `build/Version.props` already
   says 1.0.0 and there is no earlier tag to compare with, so the version check has nothing to
   hold it against and passes. `CHANGELOG.md` already has its `## [1.0.0]` section; it takes its
   date as in step 4 of *A release*.

After the first release, delete the `squash` skill and its line in `CLAUDE.md`, and this section
with them: what it checks before the switch, step 4 of *A release* checks before every release.

## A release

1. Cut `release/x.y` from `main`, and raise `build/Version.props` there in the first commit: the
   patch for fixes alone, the minor for a feature, the major only for a breaking change. Raise
   `SumireCoreVersion` too if Core will change (*Packages* in `CLAUDE.md`, and the comment in
   `build/Version.props`). In the same commit, open a `## [x.y.z]` section at the top of
   `CHANGELOG.md`, with no date yet, and a link line for it at the bottom, above the existing ones,
   comparing with the last tag
   (`[1.1.0]: https://github.com/startbits/SumireCraft.Maui/compare/v1.0.0...v1.1.0`).
2. Each change is a `feature/` or `fix/` branch cut from the release branch, reviewed, and merged
   back into it by pull request. Each one that an app author would notice adds its line to that
   section. Repeat.
3. If what goes in outgrows the number raised in step 1 -- a feature arriving in a release of
   fixes -- raise the number again before the pull request into `main`, and the section's heading
   and its link line with it, so that the heading still resolves.
4. Run the UI tests on the release branch in Debug, then Release, then Consumer, every test green
   in all three: CI does not run them, and the merge publishes. Then date the section
   (`## [1.1.0] - 2027-04-15`), the day the pull request is opened: the merge day
   is not known yet, and a day or two apart matters less than a section left undated. If Core
   stays as it was, say so in the heading (`## [1.1.0] - 2027-04-15 (Controls only)`). Then open a
   pull request from the release branch into `main`. Its checks fail unless the version has gone up
   and Core is numbered right.
5. The maintainer merges, and the release workflow tags and publishes. Delete the release branch;
   the tag stays, and is what that release is found by from then on.

## A change outside the packages

For the README, `CLAUDE.md`, `.claude/`, the workflows, the sample or the tests, when nothing on
the release workflow's list of package paths changes. Until 1.0.0 is published, nothing checks a
pull request into `main`, so these go through `release/1.0` like everything else.

1. Cut `feature/<what>` or `fix/<what>` from `main`.
2. Open a pull request into `main`. Its checks skip the version, since no package changes; if they
   ask for one, something on the list was touched after all, and the change belongs in a release
   branch.
3. Once it is merged, merge `main` into the open release branch, if there is one.

The README describing something not yet published is not such a change: it goes with that change
through the release branch.

## A hotfix

Only when the next release cannot wait; a fix that can goes through the release branch.

1. Cut `hotfix/x.y.z` from `main`, named after the version it publishes.
2. Fix it on that branch itself -- no branches under it -- and raise `build/Version.props` there.
   Add its dated `## [x.y.z]` section and link line to `CHANGELOG.md` as a release does.
3. Open a pull request into `main`. The same checks apply, and merging publishes it. Delete the
   hotfix branch, as a release branch is; the tag stays.
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
