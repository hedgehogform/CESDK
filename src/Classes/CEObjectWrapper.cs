using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    /// <summary>
    /// Base exception for all CESDK operations
    /// </summary>
    public class CesdkException : Exception
    {
        public CesdkException(string message) : base(message) { }
        public CesdkException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Base class for wrapping Cheat Engine objects in C#
    /// </summary>
    public abstract class CEObjectWrapper
    {
        protected readonly LuaNative lua;
        protected IntPtr CEObject;

        protected CEObjectWrapper()
        {
            lua = PluginContext.Lua;
            CEObject = IntPtr.Zero;
        }

        /// <summary>
        /// Pushes the CE object onto the Lua stack
        /// </summary>
        internal void PushCEObject()
        {
            if (CEObject == IntPtr.Zero)
                throw new InvalidOperationException("CE object is not initialized");

            lua.PushCEObject(CEObject);
        }

        /// <summary>
        /// Sets the CE object from the current top of the Lua stack
        /// </summary>
        protected void SetCEObjectFromStack()
        {
            if (!lua.IsCEObject(-1))
                throw new InvalidOperationException("Top of stack is not a CE object");

            CEObject = lua.ToCEObject(-1);
        }

        #region Property Helpers

        protected int GetIntProperty(string name)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.GetField(-1, name);
                var result = lua.ToInteger(-1);
                lua.Pop(2);
                return result;
            }
            catch { return 0; }
        }

        protected long GetLongProperty(string name)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.GetField(-1, name);
                var result = lua.ToInteger(-1);
                lua.Pop(2);
                return result;
            }
            catch { return 0; }
        }

        protected string GetStringProperty(string name)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.GetField(-1, name);
                var result = lua.ToString(-1) ?? "";
                lua.Pop(2);
                return result;
            }
            catch { return ""; }
        }

        protected bool GetBoolProperty(string name)
        {
            try
            {
                lua.PushCEObject(CEObject);
                lua.GetField(-1, name);
                var result = lua.ToBoolean(-1);
                lua.Pop(2);
                return result;
            }
            catch { return false; }
        }

        protected void SetIntProperty(string name, int value)
        {
            lua.PushCEObject(CEObject);
            lua.PushInteger(value);
            lua.SetField(-2, name);
            lua.Pop(1);
        }

        protected void SetStringProperty(string name, string value)
        {
            lua.PushCEObject(CEObject);
            lua.PushString(value);
            lua.SetField(-2, name);
            lua.Pop(1);
        }

        protected void SetBoolProperty(string name, bool value)
        {
            lua.PushCEObject(CEObject);
            lua.PushBoolean(value);
            lua.SetField(-2, name);
            lua.Pop(1);
        }

        #endregion

        #region Method Helpers

        /// <summary>
        /// Calls a parameterless method on this CE object
        /// </summary>
        protected void CallMethod(string methodName)
        {
            lua.PushCEObject(CEObject);
            lua.GetField(-1, methodName);
            if (!lua.IsFunction(-1))
            {
                lua.Pop(2);
                throw new InvalidOperationException($"{methodName} method not available");
            }
            lua.PushValue(-2); // self
            lua.PCall(1, 0);
            lua.Pop(1);
        }

        #endregion

        /// <summary>
        /// Whether this wrapper should skip destruction (e.g. for CE-owned objects like the main memscan,
        /// or FoundList objects that are owned by their parent MemScan).
        /// </summary>
        protected internal bool SuppressDestroy { get; set; }

        ~CEObjectWrapper()
        {
            if (SuppressDestroy || CEObject == IntPtr.Zero)
                return;

            try
            {
                PushCEObject();
                lua.PushString("destroy");
                lua.GetTable(-2);

                if (lua.IsFunction(-1))
                {
                    lua.PushValue(-2); // Push self
                    lua.PCall(1, 0);
                }
                else
                {
                    lua.Pop(1); // Pop non-function
                }

                lua.Pop(1); // Pop CE object
            }
            catch
            {
                // Ignore errors during destruction
            }
        }
    }
}