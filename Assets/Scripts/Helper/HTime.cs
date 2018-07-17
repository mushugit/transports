using System;

public static class HTime
{
    public static void DisplayTimeSpan(string label, TimeSpan ts, int divisor)
    {
        var t = (long)ts.TotalMilliseconds * 10 * 1000 / divisor;
        UnityEngine.Debug.Log($"{label}:{new TimeSpan(t)} ({ts}/{divisor})");
    }
}

