# God is a DJ

Unity 6 (6000.3.5f1) 2D WebGL multiplayer game for Global Game Jam 2026, themed "Masks".

## Running WebGL Builds Locally

Unity's WebGL builds require a proper HTTP server (not `file://`) because of WASM and compressed asset loading. A local dev server is included:

```bash
./serve-webgl.sh [build-folder] [port]
```

Defaults: `Builds/WebGL`, port `8080`.

1. Build in Unity: File > Build Profiles > WebGL, set output to `Builds/WebGL`
2. Run: `./serve-webgl.sh`
3. Open http://localhost:8080

The server handles Brotli (`.br`) and gzip (`.gz`) Content-Encoding headers automatically, which Unity's compressed WebGL builds require.
