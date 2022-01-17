using System;

namespace Unity.Platforms
{
    public class RunLoop
    {
        /// Timestamp in seconds as a double since some platform-dependent point in time, that will be used to calculate delta and elapsed time.
        /// It is expected to be a timestamp from monotonic high-frequency timer, but on some platforms it is received from a wallclock timer (emscripten, html5)
        public delegate bool RunLoopDelegate(double timestampInSeconds);

        public static RunLoopDelegate CurrentRunLoopDelegate { get; protected set; }

        public static void EnterMainLoop(RunLoopDelegate runLoopDelegate)
        {
            CurrentRunLoopDelegate = runLoopDelegate;
            RunLoopImpl.EnterMainLoop(runLoopDelegate);
        }

        /// <summary>
        /// If DisableTicks is set, then the run loop will not trigger the UnityInstance "tick".
        /// This can be helpful if you need to temporarily take over driving the main tick.
        /// Only supported on the Web platform.
        /// </summary>
        public static bool DisableTicks {
#if UNITY_WEBGL
            get => RunLoopImpl.DisableTicks;
            set => RunLoopImpl.DisableTicks = value;
#else
            get => false;
            set => throw new Exception("DisableTicks is only supported on the Web platform");
#endif
        }
    }
}
