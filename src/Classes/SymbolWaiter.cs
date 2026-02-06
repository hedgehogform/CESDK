using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CESDK.Utils;

namespace CESDK.Classes
{
    public enum SymbolLevel
    {
        Sections = 1,
        Exports = 2,
        DotNet = 3,
        PDB = 4
    }

    public class SymbolWaitException : CesdkException
    {
        public SymbolLevel Level { get; }

        public SymbolWaitException(SymbolLevel level, string message) : base($"Failed to wait for {level} symbols: {message}")
        {
            Level = level;
        }

        public SymbolWaitException(SymbolLevel level, string message, Exception innerException) : base($"Failed to wait for {level} symbols: {message}", innerException)
        {
            Level = level;
        }
    }

    public static class SymbolWaiter
    {
        private static readonly Dictionary<SymbolLevel, string> LevelFunctionMap = new()
        {
            [SymbolLevel.Sections] = "waitForSections",
            [SymbolLevel.Exports] = "waitForExports",
            [SymbolLevel.DotNet] = "waitForDotNet",
            [SymbolLevel.PDB] = "waitForPDB"
        };

        public static void WaitForSections() => WaitFor(SymbolLevel.Sections);
        public static void WaitForExports() => WaitFor(SymbolLevel.Exports);
        public static void WaitForDotNet() => WaitFor(SymbolLevel.DotNet);
        public static void WaitForPDB() => WaitFor(SymbolLevel.PDB);

        public static Task WaitForSectionsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            WaitForAsync(SymbolLevel.Sections, timeout, cancellationToken);
        public static Task WaitForExportsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            WaitForAsync(SymbolLevel.Exports, timeout, cancellationToken);
        public static Task WaitForDotNetAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            WaitForAsync(SymbolLevel.DotNet, timeout, cancellationToken);
        public static Task WaitForPDBAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            WaitForAsync(SymbolLevel.PDB, timeout, cancellationToken);

        public static void WaitFor(SymbolLevel level)
        {
            if (!LevelFunctionMap.TryGetValue(level, out var functionName))
                throw new ArgumentException($"Invalid symbol level: {level}", nameof(level));

            try
            {
                LuaUtils.CallVoidLuaFunction(functionName, $"wait for {level} symbols");
            }
            catch (InvalidOperationException ex)
            {
                throw new SymbolWaitException(level, ex.Message, ex);
            }
        }

        public static async Task WaitForAsync(SymbolLevel level, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (timeout.HasValue)
                cts.CancelAfter(timeout.Value);

            try
            {
                await Task.Run(() =>
                {
                    cts.Token.ThrowIfCancellationRequested();
                    WaitFor(level);
                }, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeout.HasValue)
            {
                throw new TimeoutException($"Timeout waiting for {level} symbols after {timeout.Value.TotalSeconds:F1} seconds");
            }
        }
    }
}