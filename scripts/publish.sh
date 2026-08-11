#!/usr/bin/env bash
# Builds the self-contained ducktape CLI binary into ./publish
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet publish src/DuckTape/DuckTape.csproj \
	-c Release \
	-r linux-x64 \
	--self-contained true \
	-o publish