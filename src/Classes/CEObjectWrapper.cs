using System;
using CESDK.Lua;

namespace CESDK.Classes
{
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

        ~CEObjectWrapper()
        {
            if (CEObject != IntPtr.Zero)
            {
                try
                {
                    // Push the CE object and call its destroy method
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
}