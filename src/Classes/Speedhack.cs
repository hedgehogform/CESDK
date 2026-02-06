using System;
using CESDK.Utils;

namespace CESDK.Classes
{
    public class SpeedhackException : CesdkException
    {
        public SpeedhackException(string message) : base(message) { }
        public SpeedhackException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Speedhack
    {
        public static void SetSpeed(float speed) =>
            WrapException(() => LuaUtils.CallVoidLuaFunction("speedhack_setSpeed", $"set speedhack speed to {speed}", speed));

        public static float GetSpeed() =>
            WrapException(() => LuaUtils.CallLuaFunction("speedhack_getSpeed", "get speedhack speed",
                () => (float)PluginContext.Lua.ToNumber(-1)));

        private static T WrapException<T>(Func<T> operation)
        {
            try { return operation(); }
            catch (InvalidOperationException ex) { throw new SpeedhackException(ex.Message, ex); }
        }

        private static void WrapException(Action operation)
        {
            try { operation(); }
            catch (InvalidOperationException ex) { throw new SpeedhackException(ex.Message, ex); }
        }
    }
}