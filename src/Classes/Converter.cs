using System;
using CESDK.Lua;

namespace CESDK.Classes
{
    public class ConverterException : Exception
    {
        public ConverterException(string message) : base(message) { }
        public ConverterException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Converter
    {
        private static readonly LuaNative lua = PluginContext.Lua;

        public static string StringToMD5(string value)
        {
            try
            {
                lua.GetGlobal("stringToMD5String");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new ConverterException("stringToMD5String function not available in this CE version");
                }

                lua.PushString(value);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new ConverterException($"stringToMD5String() call failed: {error}");
                }

                var md5 = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return md5;
            }
            catch (Exception ex) when (ex is not ConverterException)
            {
                throw new ConverterException("Failed to convert string to MD5", ex);
            }
        }

        public static string AnsiToUtf8(string text)
        {
            try
            {
                lua.GetGlobal("ansiToUtf8");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new ConverterException("ansiToUtf8 function not available in this CE version");
                }

                lua.PushString(text);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new ConverterException($"ansiToUtf8() call failed: {error}");
                }

                var utf8Text = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return utf8Text;
            }
            catch (Exception ex) when (ex is not ConverterException)
            {
                throw new ConverterException("Failed to convert ANSI to UTF-8", ex);
            }
        }

        public static string Utf8ToAnsi(string text)
        {
            try
            {
                lua.GetGlobal("utf8ToAnsi");
                if (!lua.IsFunction(-1))
                {
                    lua.Pop(1);
                    throw new ConverterException("utf8ToAnsi function not available in this CE version");
                }

                lua.PushString(text);

                var result = lua.PCall(1, 1);
                if (result != 0)
                {
                    var error = lua.ToString(-1);
                    lua.Pop(1);
                    throw new ConverterException($"utf8ToAnsi() call failed: {error}");
                }

                var ansiText = lua.ToString(-1) ?? "";
                lua.Pop(1);
                return ansiText;
            }
            catch (Exception ex) when (ex is not ConverterException)
            {
                throw new ConverterException("Failed to convert UTF-8 to ANSI", ex);
            }
        }
    }
}