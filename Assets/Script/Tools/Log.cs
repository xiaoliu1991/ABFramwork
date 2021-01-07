using UnityEngine;

public enum LogLevel
{
    All = 0,
    Info,
    Warn,
    Error,
    Off
}

public class Log
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public static LogLevel level;

    public static void l(params object[] args)
    {
        if (level == LogLevel.All || level == LogLevel.Info)
        {
            Debug.Log(getFormatStr(args));
        }
    }

    public static void warn(params object[] args)
    {
        if (level == LogLevel.All || level == LogLevel.Warn)
        {
            Debug.LogWarning(getFormatStr(args));
        }
    }

    public static void error(params object[] args)
    {
        if (level == LogLevel.All || level == LogLevel.Error)
        {
            Debug.LogError(getFormatStr(args));
        }
    }

    private static string getFormatStr(object[] args)
    {
        if (args == null)
            return "[NULL]   ";

        string str = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == null)
                str += "[NULL] , ";
            else
                str += args[i] + " , ";
        }
        return "[" + System.DateTime.Now.ToString("hh:mm：ss：ffff") + "] : " + str;
    }

}
