#!/usr/bin/env python3
# Copyright 2026 Küstenlogik
# SPDX-License-Identifier: Apache-2.0
"""
Pack every exercise of the course as two downloads (#34): the shell a learner
fills in, and the solution.

  python scripts/pack_exercises.py --version v1.2.0 [--out artifacts/exercises] [--verify] [--check-index]

What is an exercise is read off the tree, never from a list:

  units/<unit>/<lesson>/start      -> <unit>-<lesson>-start.zip     (+ the lesson's README.md)
  units/<unit>/<lesson>/completed  -> <unit>-<lesson>-solution.zip  (+ the lesson's README.md)
  capstones/<name>/                -> capstone-<name>-start.zip     (everything but solution/)
                                      capstone-<name>-solution.zip  (everything, if solution/ exists)

A lesson or capstone added without touching CI ships. Each archive has one
top-level folder, a minimal .slnx over its projects (so "open in the IDE" is
one step), and a README that names the course release and the Bowire
version it was built against — a zip from an old release teaches an old
API, and says so.

Archive names carry no version (bowire-bootcamp-unit-4-lesson-1-start.zip):
the course links each download as releases/latest/download/<name>, which
only works for a name that stays the same. The release tag and the README
inside say which version it is.

--check-index fails when index.md does not link an archive, so a new
exercise cannot ship without its download showing up next to the lessons.

--verify extracts every archive into a scratch directory and builds it
there, on its own: what a learner downloads must build without the rest of
the repository.
"""

from __future__ import annotations

import argparse
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SKIP_DIRS = {"bin", "obj", ".vs", "node_modules", "__pycache__"}
PREFIX = "bowire-bootcamp"


def exercises() -> list[tuple[str, Path, list[tuple[Path, str]]]]:
    """(archive stem, README to add or None, [(source dir, path inside archive)])."""
    found = []
    for lesson in sorted(ROOT.glob("units/*/lesson-*")):
        unit = lesson.parent.name
        readme = lesson / "README.md"
        for kind, sub in (("start", "start"), ("solution", "completed")):
            if (lesson / sub).is_dir():
                found.append((f"{unit}-{lesson.name}-{kind}", readme if readme.is_file() else None, [(lesson / sub, "")]))
    for capstone in sorted(p for p in (ROOT / "capstones").iterdir() if p.is_dir()):
        found.append((f"capstone-{capstone.name}-start", None, [(capstone, "")]))
        if (capstone / "solution").is_dir():
            found.append((f"capstone-{capstone.name}-solution", None, [(capstone, "")]))
    return found


def copy_tree(src: Path, dest: Path, exclude_solution: bool) -> None:
    for path in sorted(src.rglob("*")):
        rel = path.relative_to(src)
        if any(part in SKIP_DIRS for part in rel.parts):
            continue
        if exclude_solution and rel.parts and rel.parts[0] == "solution":
            continue
        target = dest / rel
        if path.is_dir():
            target.mkdir(parents=True, exist_ok=True)
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(path, target)


def bowire_version(tree: Path) -> str | None:
    for proj in tree.rglob("*.csproj"):
        m = re.search(r'Include="Kuestenlogik\.Bowire"\s+Version="([^"]+)"', proj.read_text(encoding="utf-8"))
        if m:
            return m.group(1)
    return None


def write_slnx(tree: Path, name: str) -> list[Path]:
    projects = sorted(tree.rglob("*.csproj"))
    if projects:
        lines = ["<Solution>"] + [f'  <Project Path="{p.relative_to(tree).as_posix()}" />' for p in projects] + ["</Solution>", ""]
        (tree / f"{name}.slnx").write_text("\n".join(lines), encoding="utf-8")
    return projects


def stamp_readme(tree: Path, readme: Path | None, version: str, stem: str) -> None:
    target = tree / "README.md"
    text = readme.read_text(encoding="utf-8") if readme else (target.read_text(encoding="utf-8") if target.is_file() else f"# {stem}\n")
    bowire = bowire_version(tree)
    kind = "The solution" if stem.endswith("-solution") else "The exercise shell (the TODOs are yours to fill in)"
    footer = (
        "\n\n---\n\n"
        f"*{kind}, from Bowire Bootcamp {version}"
        + (f", built against `Kuestenlogik.Bowire` {bowire}" if bowire else "")
        + ". Links to other lessons point into the course repository: "
        "https://github.com/Kuestenlogik/Bowire.Bootcamp*\n"
    )
    target.write_text(text.rstrip() + footer, encoding="utf-8")


def pack(version: str, out: Path) -> list[Path]:
    out.mkdir(parents=True, exist_ok=True)
    archives = []
    with tempfile.TemporaryDirectory() as scratch:
        for stem, readme, sources in exercises():
            name = f"{PREFIX}-{stem}"
            tree = Path(scratch) / name
            for src, _ in sources:
                copy_tree(src, tree, exclude_solution=stem.startswith("capstone-") and stem.endswith("-start"))
            stamp_readme(tree, readme, version, stem)
            write_slnx(tree, name)
            archive = out / f"{name}.zip"
            with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as z:
                for f in sorted(tree.rglob("*")):
                    if f.is_file():
                        z.write(f, f.relative_to(Path(scratch)).as_posix())
            archives.append(archive)
            print(f"packed {archive.name}")
    return archives


def verify(archives: list[Path]) -> list[str]:
    failed = []
    for archive in archives:
        with tempfile.TemporaryDirectory() as scratch:
            with zipfile.ZipFile(archive) as z:
                z.extractall(scratch)
            slnx = list(Path(scratch).glob("*/*.slnx"))
            if not slnx:
                print(f"verify {archive.name}: no projects, nothing to build")
                continue
            print(f"::group::build {archive.name}", flush=True)
            ok = subprocess.run(["dotnet", "build", str(slnx[0]), "-c", "Release", "-nologo"]).returncode == 0
            print("::endgroup::", flush=True)
            print(f"verify {archive.name}: {'ok' if ok else 'FAILED'}", flush=True)
            if not ok:
                failed.append(archive.name)
    return failed


def check_index(archives: list[Path]) -> list[str]:
    index = (ROOT / "index.md").read_text(encoding="utf-8")
    return [a.name for a in archives if f"releases/latest/download/{a.name}" not in index]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--version", required=True, help="release tag, e.g. v1.2.0 (or 'pr' for a CI dry run)")
    parser.add_argument("--out", default=str(ROOT / "artifacts" / "exercises"))
    parser.add_argument("--verify", action="store_true", help="build every archive on its own after packing")
    parser.add_argument("--check-index", action="store_true", help="fail when index.md does not link an archive")
    args = parser.parse_args()

    archives = pack(args.version, Path(args.out))
    # Zero archives would be a green run that shipped nothing — the silent
    # failure this repository's CI exists to prevent.
    if not archives:
        print("::error::no exercises found", file=sys.stderr)
        return 1
    if args.check_index:
        unlinked = check_index(archives)
        if unlinked:
            print("::error::index.md does not link: " + ", ".join(unlinked), file=sys.stderr)
            return 1
    if args.verify:
        failed = verify(archives)
        if failed:
            print("::error::archives that do not build on their own: " + ", ".join(failed), file=sys.stderr)
            return 1
    print(f"{len(archives)} archives in {args.out}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
