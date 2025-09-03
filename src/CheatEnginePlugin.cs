#nullable enable
using CESDK.Lua;

namespace CESDK
{
    /// <summary>
    /// Base class for creating Cheat Engine plugins with a clean, modern API.
    /// </summary>
    public abstract class CheatEnginePlugin
    {
        /// <summary>
        /// Gets the display name of your plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Called when the plugin is enabled.
        /// </summary>
        protected virtual void OnEnable() { }

        /// <summary>
        /// Called when the plugin is disabled.
        /// </summary>
        protected virtual void OnDisable() { }

        #region Internal Plugin Interface

        internal void InternalOnEnable() => OnEnable();
        internal void InternalOnDisable() => OnDisable();

        #endregion

        /// <summary>
        /// Processes pending Windows messages to keep CE responsive.
        /// </summary>
        protected static void ProcessMessages() => CESDK.ProcessMessages();

        /// <summary>
        /// Checks for synchronization events with a timeout.
        /// </summary>
        protected static bool CheckSynchronize(int timeout) => CESDK.CheckSynchronize(timeout);
    }
}
