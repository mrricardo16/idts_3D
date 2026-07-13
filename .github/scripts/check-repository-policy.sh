#!/usr/bin/env bash

set -euo pipefail

if [[ $# -ne 2 ]]; then
  echo "Usage: $0 <base-sha-or-empty> <head-sha>" >&2
  exit 2
fi

base_sha="$1"
head_sha="$2"

git cat-file -e "${head_sha}^{commit}"

if [[ -n "$base_sha" ]]; then
  git cat-file -e "${base_sha}^{commit}"
  git diff --check "$base_sha" "$head_sha"
  mapfile -d '' changed_paths < <(git diff --name-only -z --diff-filter=AMR "$base_sha" "$head_sha")
else
  git diff-tree --check --no-commit-id -r "$head_sha"
  mapfile -d '' changed_paths < <(git diff-tree --no-commit-id --name-only -z --diff-filter=AMR -r "$head_sha")
fi

violations=0

for path in "${changed_paths[@]}"; do
  normalized_path="${path,,}"
  filename="${normalized_path##*/}"

  case "$normalized_path" in
    */bin/*|bin/*|*/obj/*|obj/*|*/node_modules/*|node_modules/*|*/dist/*|dist/*|*/coverage/*|coverage/*|*/testresults/*|testresults/*|*/test-results/*|test-results/*|*/playwright-report/*|playwright-report/*|*/uploads/*|uploads/*|*/local-uploads/*|local-uploads/*|*/conversion-output/*|conversion-output/*|*/conversion-temp/*|conversion-temp/*)
      echo "Repository policy violation: prohibited generated/local path: $path" >&2
      violations=1
      ;;
  esac

  case "$filename" in
    *.log|*.tmp|*.temp|*.dump|*.backup|*.bak|*.db|*.sqlite|*.sqlite3)
      echo "Repository policy violation: prohibited generated/local file: $path" >&2
      violations=1
      ;;
    npm-debug.log*)
      echo "Repository policy violation: npm debug log is not allowed: $path" >&2
      violations=1
      ;;
    .env|.env.*)
      if [[ "$filename" != ".env.example" && "$filename" != ".env.sample" ]]; then
        echo "Repository policy violation: real environment file is not allowed: $path" >&2
        violations=1
      fi
      ;;
  esac
done

if [[ "$violations" -ne 0 ]]; then
  exit 1
fi

echo "Repository policy passed for changed paths."
