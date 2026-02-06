// Polyfill for C# 9.0 init properties in netstandard2.0
// ReSharper disable once CheckNamespace
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    [SuppressMessage("Design", "S2094:Classes should not be empty", Justification = "Marker type for compiler")]
    internal static class IsExternalInit { }
}
