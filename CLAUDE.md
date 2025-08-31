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

The codebase follows a modern, clean architecture where C# classes provide type-safe access to Cheat Engine's Lua API:

### Core Components

- **CESDK.cs** - Main plugin infrastructure and entry point for CE plugin system
- **CheatEnginePlugin.cs** - Base plugin class for plugin development  
- **PluginContext.cs** - Global context providing access to plugin services

### Lua Integration

- **LuaEngine.cs** - High-level Lua scripting interface with C# method registration
- **LuaNative.cs** - Low-level Lua C API wrapper for direct stack manipulation
- **CEInterop.cs** - Core CE function imports and interop structures

### System Utilities

- **SystemInfo.cs** - System information utilities (dark mode detection, OS detection)

### Memory Management

- **MemoryScanner.cs** - Memory scanning operations
- **ScanConfiguration.cs** - Scan parameter configuration
- **ScanResults.cs** - Scan result management
- **ScanTypes.cs** - Memory scan type definitions

### Plugin Development Pattern

1. Inherit from `CheatEnginePlugin`
2. Implement `GetName()`, `OnEnable()`, `OnDisable()` methods
3. Access Lua functionality via `PluginContext.Lua`
4. Use raw Lua state manipulation for advanced scenarios

### Modern Lua Integration

The architecture provides both high-level and low-level Lua access:
- **High-level**: `LuaEngine` for script execution, function registration, and variable management
- **Low-level**: `LuaNative` for direct Lua stack manipulation and advanced operations
- **Type Safety**: All Lua interactions wrapped with proper error handling and stack management
- **Plugin Context**: Global access to Lua engine through `PluginContext.Lua`

### Key Design Patterns

- **Clean Architecture**: Separation of concerns with dedicated namespaces (Core, Lua, System, Memory)
- **Static Context**: Global plugin context accessible throughout the application
- **Exception Safety**: Proper Lua stack cleanup and C# exception handling
- **Modern C# Features**: Nullable reference types, pattern matching, and modern syntax