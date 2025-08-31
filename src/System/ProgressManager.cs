using System;
using CESDK.Lua;

namespace CESDK.System
{
    /// <summary>
    /// Represents different progress bar states in Cheat Engine.
    /// </summary>
    public enum ProgressState
    {
        /// <summary>
        /// Progress bar is hidden/inactive.
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// Progress bar is visible and showing normal progress.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Progress bar is in indeterminate/marquee mode (continuous animation).
        /// </summary>
        Indeterminate = 2,

        /// <summary>
        /// Progress bar is showing an error state (typically red).
        /// </summary>
        Error = 3,

        /// <summary>
        /// Progress bar is showing a paused state (typically yellow).
        /// </summary>
        Paused = 4
    }

    /// <summary>
    /// Exception thrown when progress operations fail.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ProgressException class.
    /// </remarks>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public class ProgressException(string message, Exception? innerException = null) : Exception($"Progress operation failed: {message}", innerException)
    {
    }

    /// <summary>
    /// Provides control over Cheat Engine's progress bar for long-running operations.
    /// Wraps CE's progress functions with high-level, type-safe C# methods.
    /// </summary>
    /// <remarks>
    /// <para>This class allows plugins to show progress feedback to users during lengthy operations.</para>
    /// <para>The progress bar appears in CE's status area and can show percentage, indeterminate, or error states.</para>
    /// <para>Always hide the progress bar when operations complete to maintain clean UI.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Show progress for a long operation
    /// try
    /// {
    ///     ProgressManager.SetState(ProgressState.Normal);
    ///     ProgressManager.SetValue(0);
    ///     
    ///     for (int i = 0; i &lt; totalItems; i++)
    ///     {
    ///         ProcessItem(i);
    ///         
    ///         // Update progress
    ///         var percentage = (int)((double)i / totalItems * 100);
    ///         ProgressManager.SetValue(percentage);
    ///     }
    /// }
    /// finally
    /// {
    ///     // Always hide progress when done
    ///     ProgressManager.Hide();
    /// }
    /// 
    /// // Show indeterminate progress
    /// ProgressManager.ShowIndeterminate();
    /// PerformLongRunningTask();
    /// ProgressManager.Hide();
    /// 
    /// // Show error state
    /// ProgressManager.ShowError();
    /// Thread.Sleep(2000); // Let user see error
    /// ProgressManager.Hide();
    /// </code>
    /// </example>
    public static class ProgressManager
    {
        /// <summary>
        /// Sets the state of the progress bar.
        /// </summary>
        /// <param name="state">The desired progress state.</param>
        /// <exception cref="ProgressException">Thrown when setting the progress state fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>setProgressState()</c> Lua function.</para>
        /// <para>Hidden state makes the progress bar invisible.</para>
        /// <para>Normal state shows a standard progress bar that can be updated with SetValue().</para>
        /// <para>Indeterminate state shows a continuous animation without specific progress.</para>
        /// <para>Error and Paused states provide visual feedback about operation status.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Show normal progress bar
        /// ProgressManager.SetState(ProgressState.Normal);
        /// 
        /// // Show indeterminate progress
        /// ProgressManager.SetState(ProgressState.Indeterminate);
        /// 
        /// // Show error state
        /// ProgressManager.SetState(ProgressState.Error);
        /// 
        /// // Hide progress bar
        /// ProgressManager.SetState(ProgressState.Hidden);
        /// </code>
        /// </example>
        public static void SetState(ProgressState state)
        {
            try
            {
                var lua = PluginContext.Lua;
                var state_lua = lua.State;
                var native = lua.Native;

                // Get the setProgressState function
                native.GetGlobal(state_lua, "setProgressState");
                if (!native.IsFunction(state_lua, -1))
                {
                    native.Pop(state_lua, 1);
                    throw new ProgressException("setProgressState function not available in this CE version");
                }

                // Push the state parameter
                native.PushInteger(state_lua, (int)state);

                // Call the function (1 parameter, no return values)
                var result = native.PCall(state_lua, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state_lua, -1);
                    native.Pop(state_lua, 1);
                    throw new ProgressException($"setProgressState({state}) call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not ProgressException)
            {
                var lua = PluginContext.Lua;
                lua.Native.SetTop(lua.State, 0);
                throw new ProgressException($"Error setting progress state to {state}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets the progress value as a percentage (0-100).
        /// </summary>
        /// <param name="percentage">The progress percentage (0-100).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage is not between 0 and 100.</exception>
        /// <exception cref="ProgressException">Thrown when setting the progress value fails.</exception>
        /// <remarks>
        /// <para>This method wraps CE's <c>setProgressValue()</c> Lua function.</para>
        /// <para>The progress bar must be in Normal state to display percentage values.</para>
        /// <para>Values outside 0-100 range are clamped to valid bounds.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Set progress to 50%
        /// ProgressManager.SetValue(50);
        /// 
        /// // Update progress in a loop
        /// for (int i = 0; i &lt;= 100; i += 10)
        /// {
        ///     ProgressManager.SetValue(i);
        ///     DoSomeWork();
        /// }
        /// </code>
        /// </example>
        public static void SetValue(int percentage)
        {
            // Clamp percentage to valid range
            percentage = Math.Max(0, Math.Min(100, percentage));

            try
            {
                var lua = PluginContext.Lua;
                var state = lua.State;
                var native = lua.Native;

                // Get the setProgressValue function
                native.GetGlobal(state, "setProgressValue");
                if (!native.IsFunction(state, -1))
                {
                    native.Pop(state, 1);
                    throw new ProgressException("setProgressValue function not available in this CE version");
                }

                // Push the percentage parameter
                native.PushInteger(state, percentage);

                // Call the function (1 parameter, no return values)
                var result = native.PCall(state, 1, 0);
                if (result != 0)
                {
                    var error = native.ToString(state, -1);
                    native.Pop(state, 1);
                    throw new ProgressException($"setProgressValue({percentage}) call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not ProgressException)
            {
                var lua = PluginContext.Lua;
                lua.Native.SetTop(lua.State, 0);
                throw new ProgressException($"Error setting progress value to {percentage}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Shows the progress bar in normal state with the specified initial percentage.
        /// </summary>
        /// <param name="initialPercentage">The initial progress percentage (0-100). Default is 0.</param>
        /// <exception cref="ProgressException">Thrown when showing progress fails.</exception>
        /// <example>
        /// <code>
        /// // Show progress starting at 0%
        /// ProgressManager.Show();
        /// 
        /// // Show progress starting at 25%
        /// ProgressManager.Show(25);
        /// </code>
        /// </example>
        public static void Show(int initialPercentage = 0)
        {
            SetState(ProgressState.Normal);
            SetValue(initialPercentage);
        }

        /// <summary>
        /// Shows the progress bar in indeterminate state (continuous animation).
        /// </summary>
        /// <exception cref="ProgressException">Thrown when showing indeterminate progress fails.</exception>
        /// <remarks>
        /// <para>Use this for operations where progress cannot be measured precisely.</para>
        /// <para>The progress bar will show a continuous animation until hidden or state changed.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// ProgressManager.ShowIndeterminate();
        /// 
        /// // Perform operation with unknown duration
        /// await SomeAsyncOperation();
        /// 
        /// ProgressManager.Hide();
        /// </code>
        /// </example>
        public static void ShowIndeterminate()
        {
            SetState(ProgressState.Indeterminate);
        }

        /// <summary>
        /// Shows the progress bar in error state.
        /// </summary>
        /// <exception cref="ProgressException">Thrown when showing error state fails.</exception>
        /// <remarks>
        /// <para>Use this to indicate that an operation has failed.</para>
        /// <para>The progress bar will typically appear red or with error styling.</para>
        /// <para>Consider showing this state briefly before hiding to give user feedback.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     PerformOperation();
        /// }
        /// catch (Exception ex)
        /// {
        ///     ProgressManager.ShowError();
        ///     Thread.Sleep(2000); // Show error for 2 seconds
        ///     ProgressManager.Hide();
        ///     throw;
        /// }
        /// </code>
        /// </example>
        public static void ShowError()
        {
            SetState(ProgressState.Error);
        }

        /// <summary>
        /// Shows the progress bar in paused state.
        /// </summary>
        /// <exception cref="ProgressException">Thrown when showing paused state fails.</exception>
        /// <remarks>
        /// <para>Use this to indicate that an operation is temporarily paused.</para>
        /// <para>The progress bar will typically appear yellow or with paused styling.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// ProgressManager.Show(50); // Start at 50%
        /// 
        /// if (needsToPause)
        /// {
        ///     ProgressManager.ShowPaused();
        ///     WaitForUserInput();
        ///     ProgressManager.SetState(ProgressState.Normal); // Resume
        /// }
        /// </code>
        /// </example>
        public static void ShowPaused()
        {
            SetState(ProgressState.Paused);
        }

        /// <summary>
        /// Hides the progress bar.
        /// </summary>
        /// <exception cref="ProgressException">Thrown when hiding progress fails.</exception>
        /// <remarks>
        /// <para>Always call this when operations complete to maintain clean UI.</para>
        /// <para>This is equivalent to SetState(ProgressState.Hidden).</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     ProgressManager.Show();
        ///     PerformOperation();
        /// }
        /// finally
        /// {
        ///     ProgressManager.Hide(); // Always hide when done
        /// }
        /// </code>
        /// </example>
        public static void Hide()
        {
            SetState(ProgressState.Hidden);
        }

        /// <summary>
        /// Updates progress with both percentage and ensures normal state.
        /// </summary>
        /// <param name="percentage">The progress percentage (0-100).</param>
        /// <exception cref="ProgressException">Thrown when updating progress fails.</exception>
        /// <remarks>
        /// <para>This is a convenience method that ensures the progress bar is visible and in normal state.</para>
        /// <para>Equivalent to calling SetState(ProgressState.Normal) followed by SetValue(percentage).</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// for (int i = 0; i &lt; items.Count; i++)
        /// {
        ///     ProcessItem(items[i]);
        ///     
        ///     var progress = (int)((double)(i + 1) / items.Count * 100);
        ///     ProgressManager.Update(progress);
        /// }
        /// 
        /// ProgressManager.Hide();
        /// </code>
        /// </example>
        public static void Update(int percentage)
        {
            SetState(ProgressState.Normal);
            SetValue(percentage);
        }

        /// <summary>
        /// Executes an action with automatic progress management.
        /// </summary>
        /// <param name="action">The action to execute with progress updates.</param>
        /// <param name="initialState">The initial progress state. Default is Normal.</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        /// <remarks>
        /// <para>The progress bar is automatically shown before the action and hidden after completion.</para>
        /// <para>The action receives a progress updater delegate to report progress.</para>
        /// <para>Progress is automatically hidden even if the action throws an exception.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// ProgressManager.WithProgress(progress =>
        /// {
        ///     for (int i = 0; i &lt; totalItems; i++)
        ///     {
        ///         ProcessItem(i);
        ///         progress((int)((double)(i + 1) / totalItems * 100));
        ///     }
        /// });
        /// 
        /// // With indeterminate progress
        /// ProgressManager.WithProgress(_ => 
        /// {
        ///     PerformLongOperation();
        /// }, ProgressState.Indeterminate);
        /// </code>
        /// </example>
        public static void WithProgress(Action<Action<int>> action, ProgressState initialState = ProgressState.Normal)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                SetState(initialState);
                if (initialState == ProgressState.Normal)
                    SetValue(0);

                action(SetValue);
            }
            finally
            {
                Hide();
            }
        }
    }
}