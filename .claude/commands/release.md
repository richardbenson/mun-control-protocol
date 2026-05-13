# Release Mun Control Protocol

Use this skill to cut a new release. It will update CHANGELOG.md, commit, push, and create the tag that triggers the CI release pipeline.

## Steps

### 1. Determine the version

Ask the user what the new version number is if they haven't already specified one (e.g. `v0.2`, `v1.0`, `v0.1.1`).

### 2. Update CHANGELOG.md

Read `CHANGELOG.md`. Add a new section at the very top (below the `# Changelog` heading) in this exact format:

```
## <version> — <today's date as YYYY-MM-DD>

<content>
```

The content should be a clear summary of what changed since the last release. Look at `git log` from the previous tag to the current HEAD to identify what was added, changed, or fixed. Group changes under headings like `### Features`, `### Fixes`, or `### Changes` as appropriate. Keep it user-facing — describe what users gain or notice, not internal implementation details.

### 3. Commit and push

Stage only `CHANGELOG.md` and commit with the message:

```
Release <version>
```

Push to `main`.

### 4. Tag and push the tag

```
git tag <version>
git push origin <version>
```

This triggers the CI release workflow, which will:
- Build and test the solution
- Publish `MunControlProtocol.MCP.exe` as a self-contained win-x64 executable
- Package `MunControlProtocol-<version>.zip`
- Extract the matching `## <version>` section from CHANGELOG.md as the GitHub Release notes
- Create a GitHub Release (marked pre-release if the tag contains a hyphen, e.g. `v0.2.0-rc1`)

### 5. Confirm

Tell the user the tag has been pushed and give them the Actions URL to watch the pipeline:
`https://github.com/richardbenson/mun-control-protocol/actions`
