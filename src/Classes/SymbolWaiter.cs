#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using CESDK.Lua;

namespace CESDK.Classes
{
    public enum SymbolLevel
    {
        Sections = 1,
        Exports = 2,
        DotNet = 3,
        PDB = 4
    }

    public class SymbolWaitException : Exception
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
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void WaitForSections()
        {
            CallWaitFunction("waitForSections", SymbolLevel.Sections);
        }

        public static void WaitForExports()
        {
            CallWaitFunction("waitForExports", SymbolLevel.Exports);
        }

        public static void WaitForDotNet()
        {
            CallWaitFunction("waitForDotNet", SymbolLevel.DotNet);
        }

        public static void WaitForPDB()
        {
            CallWaitFunction("waitForPDB", SymbolLevel.PDB);
        }

        public static async Task WaitForSectionsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForSections(), SymbolLevel.Sections, timeout, cancellationToken);
        }

        public static async Task WaitForExportsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForExports(), SymbolLevel.Exports, timeout, cancellationToken);
        }

        public static async Task WaitForDotNetAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForDotNet(), SymbolLevel.DotNet, timeout, cancellationToken);
        }

        public static async Task WaitForPDBAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await WaitAsync(() => WaitForPDB(), SymbolLevel.PDB, timeout, cancellationToken);
        }

        public static void WaitFor(SymbolLevel level)
        {
            switch (level)
            {
                case SymbolLevel.Sections:
                    WaitForSections();
                    break;
                case SymbolLevel.Exports:
                    WaitForExports();
                    break;
                case SymbolLevel.DotNet:
                    WaitForDotNet();
                    break;
                case SymbolLevel.PDB:
                    WaitForPDB();
                    break;
                default:
                    throw new ArgumentException($"Invalid symbol level: {level}", nameof(level));
            }
        }

        public static async Task WaitForAsync(SymbolLevel level, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            switch (level)
            {
                case SymbolLevel.Sections:
                    await WaitForSectionsAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.Exports:
                    await WaitForExportsAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.DotNet:
                    await WaitForDotNetAsync(timeout, cancellationToken);
                    break;
                case SymbolLevel.PDB:
                    await WaitForPDBAsync(timeout, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"Invalid symbol level: {level}", nameof(level));
            }
        }

        private static void CallWaitFunction(string functionName, SymbolLevel level)
        {
            try
            {
                lua.GetGlobal(functionName);
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new SymbolWaitException(level, $"{functionName} function not available in this CE version");
                }

                var result = lua.PCall(0, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new SymbolWaitException(level, $"{functionName}() call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not SymbolWaitException)
            {
                throw new SymbolWaitException(level, ex.Message, ex);
            }
        }

        private static async Task WaitAsync(Action waitAction, SymbolLevel level, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (timeout.HasValue)
            {
                cts.CancelAfter(timeout.Value);
            }

            try
            {
                await Task.Run(() =>
                {
                    cts.Token.ThrowIfCancellationRequested();
                    waitAction();
                }, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeout.HasValue)
            {
                throw new TimeoutException($"Timeout waiting for {level} symbols after {timeout.Value.TotalSeconds:F1} seconds");
            }
        }
    }
}