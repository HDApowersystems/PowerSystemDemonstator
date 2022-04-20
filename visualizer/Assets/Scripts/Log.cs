using System;
using UnityEngine;

public static class Log
{
    internal static void LogException(Exception e)
    {
        Debug.LogException(e);
    }

    internal static void Info(string msg)
    {
        Debug.Log(msg);

    }
}
