using System.Diagnostics;
using System.Linq;


public static class LogManager
{
    internal static string GetCallingModName()
    {
        var stack = new StackTrace();
        foreach (var frame in stack.GetFrames())
        {
            var type = frame.GetMethod()?.DeclaringType;
            if (type == null)
                continue;
            if (typeof(HAMod).IsAssignableFrom(type))
            {
                var instance = ModRegistry.LoadedMods.FirstOrDefault(m => m.GetType() == type);
                if (instance != null)
                    return instance.ModName;
            }
        }
        return "HAMod";
    }
}