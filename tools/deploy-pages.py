"""Deploy the WebGL build in game/Builds/Web to GitHub Pages.

Publishes the build on an orphan `gh-pages` branch (via a temp clone so
main's history stays clean), enables the Pages site, and prints the live
URL. Re-runnable: skips repo creation when origin already exists and
re-stages the latest build output on every run.

Usage (from repo root, WSL or Windows):
    python tools/deploy-pages.py

Requires: GitHub CLI (`gh` / `gh.exe` on WSL) authenticated with repo+workflow
scopes, and a completed build in game/Builds/Web.
"""

from __future__ import annotations

import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

REPO = "spb87/game-reebles-2d"
PAGES_BRANCH = "gh-pages"
SITE_URL = "https://spb87.github.io/game-reebles-2d/"
BUILD_DIR = Path("game/Builds/Web")


def gh() -> str:
    """Return the GitHub CLI command name — gh.exe under WSL interop, gh elsewhere."""
    return "gh.exe" if shutil.which("gh.exe") else "gh"


def run(args: list[str], cwd: Path | None = None, check: bool = True) -> subprocess.CompletedProcess:
    """Run a command, echoing it; raise on failure unless check=False."""
    print("+", " ".join(str(a) for a in args), flush=True)
    result = subprocess.run(args, cwd=cwd, text=True, capture_output=True)
    if result.stdout:
        print(result.stdout, end="")
    if result.stderr:
        print(result.stderr, end="", file=sys.stderr)
    if check and result.returncode != 0:
        raise RuntimeError(f"command failed ({result.returncode}): {' '.join(args)}")
    return result


def git_with_gh_auth(args: list[str], cwd: Path | None = None) -> subprocess.CompletedProcess:
    """Run git with a credential helper that feeds `gh auth token`.

    WSL git has no stored credentials; gh holds the token, so we inject a
    one-shot credential helper instead of embedding the token in the URL.
    """
    helper = f"!f(){{ echo username=x-access-token; echo password=$({gh()} auth token); }};f"
    return run(["git", "-c", "credential.helper=", "-c", f"credential.helper={helper}", *args], cwd=cwd)


def ensure_remote_repo(repo_root: Path) -> None:
    """Create the GitHub repo and push main if no origin remote exists yet."""
    remotes = run(["git", "remote"], cwd=repo_root).stdout.split()
    if "origin" in remotes:
        print("origin remote already configured — skipping repo create")
    else:
        run([gh(), "repo", "create", REPO, "--public", "--source", ".", "--remote", "origin"], cwd=repo_root)
    git_with_gh_auth(["push", "-u", "origin", "main"], cwd=repo_root)


def stage_pages_branch(repo_root: Path, remote_url: str) -> str:
    """Clone or init a temp gh-pages checkout, copy the build in, commit, push.

    Returns the commit hash pushed to gh-pages.
    """
    build_src = repo_root / BUILD_DIR
    if not (build_src / "index.html").exists():
        raise RuntimeError(f"no WebGL build at {build_src} — run the WebGL build first")

    with tempfile.TemporaryDirectory(prefix="gh-pages-") as tmp:
        work = Path(tmp) / "site"
        clone = run(
            ["git", "clone", "--depth", "1", "--branch", PAGES_BRANCH, remote_url, str(work)],
            check=False,
        )
        if clone.returncode != 0:
            work.mkdir()
            run(["git", "init", "-b", PAGES_BRANCH], cwd=work)
            run(["git", "remote", "add", "origin", remote_url], cwd=work)

        for entry in work.iterdir():
            if entry.name != ".git":
                shutil.rmtree(entry) if entry.is_dir() else entry.unlink()
        shutil.copytree(build_src, work, dirs_exist_ok=True)
        (work / ".nojekyll").touch()  # Unity build has files Jekyll would drop

        run(["git", "add", "-A"], cwd=work)
        run(["git", "-c", "user.name=deploy-pages", "-c", "user.email=deploy@local",
             "commit", "--allow-empty", "-m", "deploy: publish WebGL build"], cwd=work)
        git_with_gh_auth(["push", "origin", PAGES_BRANCH], cwd=work)
        return run(["git", "rev-parse", "HEAD"], cwd=work).stdout.strip()


def enable_pages() -> None:
    """Enable Pages on the gh-pages branch; a 409 means it is already on."""
    args = [gh(), "api", f"repos/{REPO}/pages", "-X", "POST",
            "-f", "source[branch]=" + PAGES_BRANCH, "-f", "source[path]=/"]
    result = run(args, check=False)
    # 409 = site already enabled — confirm it points at gh-pages via GET.
    if result.returncode != 0 and "already enabled" not in result.stderr + result.stdout:
        raise RuntimeError("failed to enable GitHub Pages")
    # jq only accepts double-quoted strings — the docs' single-quote style is
    # shell quoting, not jq syntax, and fails when passed via subprocess argv.
    run([gh(), "api", f"repos/{REPO}/pages", "-q", '.html_url + " <- " + .source.branch'])


def main() -> int:
    repo_root = Path(run(["git", "rev-parse", "--show-toplevel"]).stdout.strip())
    remote_url = f"https://github.com/{REPO}.git"

    ensure_remote_repo(repo_root)
    commit = stage_pages_branch(repo_root, remote_url)
    enable_pages()

    print(f"\ngh-pages commit: {commit}")
    print(f"Site URL: {SITE_URL}")
    print("First deploy can take a few minutes to go live. Recheck with:")
    print(f"  curl -sI {SITE_URL} | head -1")
    return 0


if __name__ == "__main__":
    sys.exit(main())
