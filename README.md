# CESDK - Cheat Engine SDK for C#

⚠️ **Work in Progress** ⚠️

A C# wrapper library for developing plugins for Cheat Engine. Provides managed .NET interfaces for memory scanning, process manipulation, and reverse engineering tasks.

## Status

This project is currently under active development. The core SDK infrastructure is implemented but many features are still being developed and tested.

## Build

```bash
dotnet build
```

## Architecture

- **CESDK.cs** - Main plugin infrastructure
- **CESDKLua.cs** - Lua API wrapper
- **CEObjectWrapper.cs** - Base wrapper class with cleanup
- Individual feature classes (Process, MemScan, MemoryRead, etc.)

## Requirements

- .NET Framework 4.8.1 or later
- Cheat Engine 7.0 or later
- Windows
