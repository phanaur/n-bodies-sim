#!/usr/bin/env bash

# ------------------------------------------------------------------
#  Script:   dump_project.sh
#
#  Purpose:  Concatenate the *content* of every file in a C#/Raylib
#            project into one plain‑text file.
#
#  Usage:
#      ./dump_project.sh           # dumps everything under current dir
#      ./dump_project.sh src/      # dumps only files below src/
#
# ------------------------------------------------------------------

set -euo pipefail          # fail fast, treat unset vars as errors

# ----- Configuration -------------------------------------------------
OUTPUT_FILE="project_dump.txt"   # name of the output file
ROOT_DIR="${1:-NBodiesSim}"     # default to NBodiesSim dir if no arg given

# ----- Safety checks --------------------------------------------------
if [[ ! -d "$ROOT_DIR" ]]; then
    echo "Error: '$ROOT_DIR' is not a directory." >&2
    exit 1
fi

# If the output file already exists, ask for confirmation (unless -f is used)
if [[ -e "$OUTPUT_FILE" ]] && [[ ! $FORCE_DUMP == true ]]; then
    read -p "File '$OUTPUT_FILE' already exists. Overwrite? [y/N] " ans
    case "${ans,,}" in
        y|yes) ;;
        *) echo "Aborted."; exit 1;;
    esac
fi

# ----- Main logic ----------------------------------------------------
# We use `find` to locate every file that is not a directory.
# For each found file we:
#   * print a header line with its path (so you know where the content came from)
#   * dump the raw contents of the file
# The whole stream is then redirected into $OUTPUT_FILE

{
    # Header for the entire dump
    echo "=== Dump started: $(date) ==="
    echo "Root directory: ${ROOT_DIR}/Data and ${ROOT_DIR}/Source"
    echo

    find "$ROOT_DIR/Data" "$ROOT_DIR/Source" -type f \( -name "*.md" -o -name "*.json" -o -name "*.cs" \) 2>/dev/null | sort | while IFS= read -r file; do
        # Print a separator that includes the relative path
        printf "\n--- %s ---\n\n" "${file#${PWD}/}"

        # Dump the content.  Use cat so binary files are preserved as-is.
        # If you only want text files, replace `cat "$file"` with a grep or sed filter.
        cat "$file"
    done

    echo
    echo "=== Dump finished: $(date) ==="
} >"$OUTPUT_FILE"

echo "✅ All file contents written to '$OUTPUT_FILE'"
