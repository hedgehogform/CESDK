# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the CESDK (Cheat Engine SDK) - a C# wrapper library for developing plugins for Cheat Engine. It provides managed .NET interfaces to interact with Cheat Engine's Lua-based API for memory scanning, process manipulation, and reverse engineering tasks.

## Build Commands

```bash
# Build the project in Debug configuration
dotnet build

# Build in Release configuration  
dotnet build -c Release

# Clean the build output
dotnet clean
```

## Architecture

The codebase follows a wrapper pattern where C# classes encapsulate Cheat Engine's Lua API functionality:

### Core Components

- **CESDK.cs** - Main plugin infrastructure that bridges between C# and CE's plugin system
- **CESDKLua.cs** - Comprehensive Lua API wrapper providing C# delegates for all Lua functions
- **CEObjectWrapper.cs** - Base class for CE object wrappers with automatic cleanup via destructors

### Key Wrapper Classes

- **Process.cs** - Process enumeration, opening processes by PID/name, process status management
- **MemScan.cs** - Memory scanning with configurable scan types, value searches, and result management
- **FoundList.cs** - Managing scan results and found memory addresses
- **MemoryRead.cs / MemoryWrite.cs** - Memory read/write operations
- **Disassembler.cs** - Assembly code analysis and disassembly
- **ThreadList.cs** - Thread enumeration and management
- **Address.cs** - Memory address utilities and conversions
- **AOB.cs** - Array of Bytes pattern scanning

### Plugin Development Pattern

1. Inherit from `CESDKPluginClass`
2. Implement `GetPluginName()`, `EnablePlugin()`, `DisablePlugin()`
3. Use `CESDK.currentPlugin.sdk` to access the main SDK instance
4. Access Lua functionality via `sdk.lua` for direct CE integration

### Lua Integration

The architecture heavily relies on CE's Lua environment:
- All CE objects are accessed through Lua function calls
- C# delegates are registered with Lua for callback functionality  
- Memory operations, scanning, and process management use CE's native Lua API
- Error handling wraps Lua exceptions with C# exception types

## Key Design Patterns

- **Wrapper Pattern**: C# classes wrap CE's Lua objects with type safety
- **RAII**: CEObjectWrapper ensures proper cleanup of CE objects via destructors
- **Delegate Registration**: C# methods registered as Lua callbacks for events
- **Plugin Architecture**: Single plugin instance pattern with global access via CESDK.currentPlugin