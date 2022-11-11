using System;

namespace Plugin.BLE.Abstractions
{
    public static class Trace
    {
        public static Action<string, object[]> TraceImplementation { get; set; }
        
        public static void Message(string format, params object[] args)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine(format, args);
                TraceImplementation?.Invoke(format, args);
            }
            catch { /* ignore */ }
        }
    }
}