using System.Diagnostics;
using Plugin.BLE.Abstractions;


namespace Plugin.BLE {
    static class DefaultTrace {
        static DefaultTrace() {
            Plugin.BLE.Abstractions.Trace.TraceImplementation = (s, o) => {Debug.WriteLine(s, o);};
        }

    }
}