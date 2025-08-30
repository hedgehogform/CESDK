# Cheat Engine SDK for C#

A comprehensive C# wrapper library for developing plugins for Cheat Engine. This SDK provides managed .NET interfaces for memory scanning, process manipulation, and reverse engineering tasks.

## Features

- **Process Management**: Enumerate running processes, open processes by PID or name
- **Memory Scanning**: Comprehensive memory scanning with configurable scan types and value searches
- **Memory Operations**: Read and write memory with type safety
- **Disassembly**: Assembly code analysis and disassembly capabilities
- **Thread Management**: Thread enumeration and manipulation
- **Lua Integration**: Direct access to Cheat Engine's Lua environment
- **Array of Bytes**: Pattern scanning for byte sequences

## Quick Start

### 1. Create a Plugin Class

```csharp
using CESDK;

public class MyPlugin : CESDKPluginClass
{
    public override string GetPluginName()
    {
        return "My Cheat Engine Plugin";
    }

    public override bool EnablePlugin()
    {
        // Initialize your plugin here
        return true;
    }

    public override bool DisablePlugin()
    {
        // Clean up your plugin here
        return true;
    }
}
```

### 2. Process Management

```csharp
var process = new Process();
var processList = process.GetProcessList();

// Open process by name
process.OpenByName("notepad.exe");

// Open process by PID
process.OpenByPid(1234);

// Get process status
var (processId, isOpen, processName) = process.GetStatus();
```

### 3. Memory Scanning

```csharp
var memScan = new MemScan();
var scanParams = new ScanParameters
{
    Value = "100",
    ScanOption = ScanOptions.soExactValue,
    VarType = VarTypes.vtDword
};

memScan.Scan(scanParams);
memScan.WaitTillDone();

var foundList = memScan.GetFoundList();
```

### 4. Memory Operations

```csharp
var memoryRead = new MemoryRead();
var memoryWrite = new MemoryWrite();

// Read memory
int value = memoryRead.ReadInteger(0x12345678);
string text = memoryRead.ReadString(0x12345678, 50);

// Write memory
memoryWrite.WriteInteger(0x12345678, 999);
memoryWrite.WriteString(0x12345678, "Hello World");
```

## Architecture

The SDK follows a wrapper pattern where C# classes encapsulate Cheat Engine's Lua API:

- **CESDKPluginClass**: Base class for all plugins
- **CEObjectWrapper**: Base wrapper with automatic cleanup
- **CESDKLua**: Comprehensive Lua API wrapper
- Individual feature classes (Process, MemScan, etc.)

## Requirements

- .NET Framework 4.8.1 or later
- Cheat Engine 7.0 or later
- Windows operating system

## Installation

Install via NuGet Package Manager:

```
Install-Package CheatEngine.SDK
```

Or via .NET CLI:

```
dotnet add package CheatEngine.SDK
```

## Documentation

For detailed documentation and examples, visit the [project repository](https://github.com/hedgehogform/CESDK).

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
