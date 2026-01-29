# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 (version 6000.3.5f1) game project for Global Game Jam 2026, themed "Masks". Currently in early development with template scaffolding only.

## Build & Run

This is a Unity project — it must be opened and run through the Unity Editor (version 6000.3.5f1). There is no CLI build pipeline configured. The solution/csproj files are auto-generated and gitignored.

- **Open project:** Launch Unity Hub, open this directory
- **Play:** Press Play in Unity Editor
- **Tests:** Window > General > Test Runner (uses com.unity.test-framework 1.6.0)

## Architecture

- **Rendering:** Universal Render Pipeline (URP 17.3.0) with separate PC and Mobile renderer/quality assets in `Assets/Settings/`
- **Input:** New Input System (1.17.0) configured via `Assets/InputSystem_Actions.inputactions`
- **Async:** UniTask (com.cysharp.unitask 2.5.10) for async/await patterns — prefer UniTask over coroutines for new async code
- **Scenes:** `Assets/Scenes/` — currently only `SampleScene.unity`

## Key Conventions

- C# scripts go under `Assets/` in appropriate subdirectories
- Editor-only scripts must be placed in `Editor/` folders to avoid build errors
- Package dependencies are managed in `Packages/manifest.json` (OpenUPM scoped registry configured for UniTask)
- Color space is Linear (not Gamma) — relevant for shader and texture work
