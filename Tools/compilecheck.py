"""
written by Claude Opus 5

Compiles every game script against Unity's assemblies, without opening the editor.

There is no test suite in this project, so this is the only automated check we have. Run it
from the repo root:

    python Tools/compilecheck.py

Why it isn't just `dotnet build Assembly-CSharp.csproj`: Unity generates that project with an
*explicit* <Compile Include> list, and it only regenerates when the editor has focus. So the
moment you add a script it isn't compiled (phantom "type not found" errors on your own new
code), and the moment you delete one you get CS2001 for a file that isn't there any more.

This builds a throwaway copy of the project with that list corrected, then deletes it.
"""

import glob
import io
import os
import re
import subprocess
import sys

SOURCE_PROJECT = "Assembly-CSharp.csproj"
TEMP_PROJECT = "_compilecheck.csproj"

COMPILE_LINE = re.compile(r'<Compile Include="([^"]+)"')

# Plugins/ compiles into Assembly-CSharp-firstpass and is referenced back as a dll.
# Including its sources here too gives duplicate-definition errors on all of DOTween.
SKIP_DIRS = {"Editor", "Plugins"}


def build_project_file(repo_root):
    """Writes TEMP_PROJECT: the Unity project with its file list brought up to date."""
    source_path = os.path.join(repo_root, SOURCE_PROJECT)

    if not os.path.exists(source_path):
        sys.exit(
            "[compilecheck] >> no %s found. open the project in Unity once so it generates "
            "the .csproj, then try again." % SOURCE_PROJECT
        )

    kept = []
    dropped = 0

    for line in io.open(source_path, encoding="utf-8-sig"):
        match = COMPILE_LINE.search(line)

        # drop entries for scripts deleted since Unity last regenerated
        if match and not os.path.exists(os.path.join(repo_root, match.group(1))):
            dropped += 1
            continue

        kept.append(line)

    src = "".join(kept)
    listed = set(m.lower() for m in COMPILE_LINE.findall(src))

    added = []
    for found in glob.glob("Assets/**/*.cs", recursive=True):
        path = found.replace("/", os.sep)

        if path.lower() in listed:
            continue
        if SKIP_DIRS.intersection(path.split(os.sep)):
            continue

        added.append(path)

    block = "".join('    <Compile Include="%s" />\n' % path for path in added)
    src = src.replace("</Project>", "  <ItemGroup>\n" + block + "  </ItemGroup>\n</Project>")

    io.open(os.path.join(repo_root, TEMP_PROJECT), "w", encoding="utf-8").write(src)

    return len(added), dropped


def main():
    repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    os.chdir(repo_root)

    added, dropped = build_project_file(repo_root)
    print("[compilecheck] >> %d script(s) added, %d stale entry(s) dropped" % (added, dropped))

    try:
        result = subprocess.run(
            ["dotnet", "build", TEMP_PROJECT, "-v", "q", "--nologo", "-t:Rebuild"],
            capture_output=True,
            text=True,
        )
    except FileNotFoundError:
        sys.exit("[compilecheck] >> dotnet not found on PATH.")
    finally:
        # always clean up, even if the build blew up. this is a scratch file and must never
        # be committed
        if os.path.exists(TEMP_PROJECT):
            os.remove(TEMP_PROJECT)

    interesting = [
        line
        for line in (result.stdout + result.stderr).splitlines()
        if "error" in line.lower() or "warning" in line.lower() or "Build succeeded" in line
    ]

    print("\n".join(interesting) if interesting else result.stdout.strip())

    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
