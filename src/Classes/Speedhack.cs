using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class SpeedhackException : Exception
    {
        public SpeedhackException(string message) : base(message) { }
        public SpeedhackException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Speedhack
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static void SetSpeed(float speed)
        {
            try
            {
                lua.GetGlobal("speedhack_setSpeed");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new SpeedhackException("speedhack_setSpeed function not available in this CE version");
                }

                lua.PushNumber(speed);

                var result = lua.PCall(1, 0);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new SpeedhackException($"speedhack_setSpeed({speed}) call failed: {error}");
                }
            }
            catch (Exception ex) when (ex is not SpeedhackException)
            {
                throw new SpeedhackException($"Failed to set speedhack speed to {speed}", ex);
            }
        }

        public static float GetSpeed()
        {
            try
            {
                lua.GetGlobal("speedhack_getSpeed");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new SpeedhackException("speedhack_getSpeed function not available in this CE version");
                }

                var result = lua.PCall(0, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new SpeedhackException($"speedhack_getSpeed() call failed: {error}");
                }

                if (!lua.IsNumber(-1))
                {
                    lua.Pop(1);
                    throw new SpeedhackException("speedhack_getSpeed() did not return a number");
                }

                var speed = (float)lua.ToNumber(-1);
                lua.Pop(1);
                return speed;
            }
            catch (Exception ex) when (ex is not SpeedhackException)
            {
                throw new SpeedhackException("Failed to get speedhack speed", ex);
            }
        }
    }
}