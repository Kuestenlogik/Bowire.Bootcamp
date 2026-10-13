#!/usr/bin/env python3
# Copyright 2026 Küstenlogik
# SPDX-License-Identifier: Apache-2.0
"""
#40 - everything in this repository is English: lessons, capstones, code,
comments, scripts, CI, audits. Another language belongs only in data the code
or a lesson has to read.

Reads every tracked text file and reports each line that reads as German, as
file:line, and exits non-zero while there is any. The same heuristic and the
same markers as the guard in Bowire (scripts/ci/check-english.mjs there): a
line counts as German when it carries at least three distinct German function
words and more of them than English ones, when a word with an umlaut or ß
stands beside a German function word, or when a quotation opening on the line
has two German words and no English ones.

Where German is right, say so:
  - a whole file: an entry in scripts/english-allowlist.json, with the reason;
  - one line: `lang-exempt: <reason>` on that line or on the line above it.

  python scripts/check_english.py            # the whole repository
  python scripts/check_english.py <file>...  # just these files
  python scripts/check_english.py --self-test
"""

from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ALLOWLIST = Path(__file__).resolve().parent / "english-allowlist.json"

# Words that are German and are not English words or common identifiers.
# Short words that English or code also use (an, in, am, im, so, man, die,
# was, also, war, es, um, er) are left out on purpose.
GERMAN = set("""
und oder nicht kein keine keinen keinem keiner nichts ist sind wird werden wurde wurden
worden sein seine seinen ihre ihren hat haben hatte hätte habe kann können konnte könnte
muss müssen musste müsste soll sollen sollte sollten darf dürfen würde würden wäre waere
ich wir sie ihr mich mir dich dir uns euch sich
der das dem den des ein eine einen einem einer eines dieser diese dieses diesen diesem
jeder jede jedes alle alles etwas mehr weniger viel viele sehr ganz gar
auch noch nur schon jetzt immer wieder dann denn weil wenn dass daß damit obwohl sondern
aber doch eigentlich vielleicht einfach gleich bitte warum wieso weshalb wie wo wohin woher
hier dort oben unten links rechts zwischen neben hinter vor nach seit bis ohne gegen durch
für fuer über ueber unter mit bei beim zum zur vom aus auf ins
gibt geht gehen ging macht machen zeigt zeigen sieht sehen steht stehen liegt liegen
fehlt fehlen funktioniert klappt passiert bleibt bleiben kommt kommen gemacht geprüft geprueft
leer leere leeren neu neue neuen neues alte alten gut besser richtig falsch
datei dateien fehler seite fenster leiste einstellungen knopf schaltfläche
""".split())

ENGLISH = set("""
the a and or not is are were be been this that these those with for of to on
at by from it its as but if then when which who what why how can could should would you
""".split())

UMLAUT = re.compile(r"[äöüßÄÖÜ]")
WORD = re.compile(r"[A-Za-zÄÖÜäöüß]+")
QUOTE = re.compile(r"['\"„“‚‘]([^'\"„“‚‘]*)$")
EXEMPT = re.compile(r"lang-exempt:\s*\S")
BINARY = re.compile(
    r"\.(png|jpe?g|gif|ico|webp|avif|bmp|svg|woff2?|ttf|otf|eot|pdf|zip|gz|tgz|nupkg|dll|exe|so|dylib|mp3|mp4|webm|wasm|bin|db|sqlite)$",
    re.IGNORECASE,
)


def german_reason(line: str) -> str | None:
    """Why a line reads as German, or None when it does not."""
    words = WORD.findall(line)
    german: list[str] = []
    english_count = 0
    umlaut_word = None
    for raw in words:
        w = raw.lower()
        if w in GERMAN and w not in german:
            german.append(w)
        if w in ENGLISH:
            english_count += 1
        if umlaut_word is None and UMLAUT.search(raw):
            umlaut_word = raw
    if len(german) >= 3 and len(german) > english_count:
        return "German words: " + ", ".join(german[:6])
    if umlaut_word and len(german) >= 1 and umlaut_word.lower() not in GERMAN:
        return f'"{umlaut_word}" beside German words: ' + ", ".join(german[:6])
    if umlaut_word and len(german) >= 2:
        return "German words: " + ", ".join(german[:6])
    # A quotation that opens at the end of an English line: judge the quoted
    # part on its own, so the English around it does not outvote it.
    m = QUOTE.search(line)
    if m:
        q = [w.lower() for w in WORD.findall(m.group(1))]
        q_german = list(dict.fromkeys(w for w in q if w in GERMAN))
        if len(q_german) >= 2 and not any(w in ENGLISH for w in q):
            return "German quotation: " + ", ".join(q_german[:6])
    return None


def find_german(text: str) -> list[tuple[int, str, str]]:
    """Every German line of one file's text, minus the ones marked exempt."""
    lines = text.splitlines()
    hits = []
    for i, line in enumerate(lines):
        if len(line) > 4000:
            continue  # minified or generated data
        if EXEMPT.search(line) or (i > 0 and EXEMPT.search(lines[i - 1])):
            continue
        reason = german_reason(line)
        if reason:
            hits.append((i + 1, line.strip(), reason))
    return hits


def load_allowlist() -> list[dict]:
    entries = json.loads(ALLOWLIST.read_text(encoding="utf-8"))
    for e in entries:
        if not e.get("path") or not str(e.get("reason", "")).strip():
            raise SystemExit(f"english-allowlist.json: every entry needs a path and a reason ({e})")
    return entries


def is_allowed(file: str, allowlist: list[dict]) -> bool:
    return any(file.startswith(e["path"]) if e["path"].endswith("/") else file == e["path"] for e in allowlist)


def tracked_files() -> list[str]:
    out = subprocess.run(["git", "ls-files", "-z"], cwd=ROOT, capture_output=True, check=True).stdout
    return [f for f in out.decode("utf-8").split("\0") if f]


def scan(files: list[str]) -> list[tuple[str, int, str, str]]:
    allowlist = load_allowlist()
    findings = []
    for file in files:
        if BINARY.search(file) or is_allowed(file, allowlist):
            continue
        try:
            data = (ROOT / file).read_bytes()
        except OSError:
            continue
        if b"\0" in data:
            continue
        text = data.decode("utf-8", errors="replace")
        for line_no, line, reason in find_german(text):
            findings.append((file, line_no, line, reason))
    return findings


def self_test() -> None:
    assert german_reason("// Deckt auch die Extension ab, deren Logik wird von dort geprueft.")
    assert german_reason("without a deliverable. Operator note: 'ein capstone macht aktuell auch nur sinn,")
    assert german_reason("   feedback: 'typisch würde man einen nicht klickbaren")
    assert german_reason("The lesson builds a plugin and tests it against the sample.") is None
    assert german_reason("alt: 'Bowire — the multi-protocol API workbench. By Küstenlogik.'") is None
    assert german_reason('new Location("munich-hbf", "München Hauptbahnhof", "station")') is None
    assert [h[0] for h in find_german("first\n// das ist nicht richtig und fehlt\nlast")] == [2]
    assert find_german("// lang-exempt: a quoted source\n// das ist nicht richtig und fehlt") == []
    assert len(find_german("// das ist nicht richtig und fehlt // lang-exempt:")) == 1
    for e in load_allowlist():
        assert len(e["reason"].strip()) > 10, e
    print("check_english self-test passed.")


def main(argv: list[str]) -> int:
    if argv[:1] == ["--self-test"]:
        self_test()
        return 0
    files = [Path(a).resolve().relative_to(ROOT).as_posix() for a in argv] if argv else tracked_files()
    findings = scan(files)
    for file, line_no, line, reason in findings:
        print(f"{file}:{line_no}: {reason}\n    {line[:160]}")
    if findings:
        print(
            f"\n{len(findings)} line(s) read as German. Translate them, or - when German is right there "
            "(data the code or a lesson reads) - add `lang-exempt: <reason>` to the line or an entry "
            "with its reason to scripts/english-allowlist.json.",
            file=sys.stderr,
        )
        return 1
    print(f"English only: {len(files)} file(s) checked, nothing reads as German.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
