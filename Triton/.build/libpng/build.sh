#!/bin/sh
# SPDX-FileCopyrightText: 2026 Neptuwunium
#
# SPDX-License-Identifier: CC0-1.0

mkdir -p ../out
podman build -t triton-build/libpng -v $(realpath ../out):/app/out .
