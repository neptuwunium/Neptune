#!/bin/sh

mkdir -p ../out
podman build -t triton-build/libpng -v $(realpath ../out):/app/out .
