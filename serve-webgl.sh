#!/bin/bash
python3 "$(dirname "$0")/serve-webgl.py" "${1:-Builds/WebGL}" "${2:-8080}"
