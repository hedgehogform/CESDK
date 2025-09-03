# CESDK - Cheat Engine SDK for C#

⚠️ **Work in Progress** ⚠️

A C# wrapper library for developing plugins for Cheat Engine. Provides managed .NET interfaces for memory scanning, process manipulation, and reverse engineering tasks.

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK.svg?type=large&issueType=license)](https://app.fossa.com/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK?ref=badge_large&issueType=license)

## Status

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK?ref=badge_shield)

This project is currently under active development. The core SDK infrastructure is implemented but many features are still being developed and tested.

## Build

```bash
dotnet build
```

## Requirements

- .NET Framework 4.8.1
- Cheat Engine 7.0 or later
- Windows

## FAQ

### Why not on Nuget?

I just want to make sure the library works first as I really can't implement unit testing with a DLL since it requires Cheat Engine running at runtime.

## License

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fhedgehogform%2FCESDK?ref=badge_large)
