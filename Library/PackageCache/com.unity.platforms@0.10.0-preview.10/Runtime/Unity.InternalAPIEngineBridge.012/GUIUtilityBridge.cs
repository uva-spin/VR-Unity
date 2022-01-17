namespace Unity.Build.Bridge
{
#if UNITY_EDITOR
    static class GUIUtilityBridge
    {
        public static double pixelsPerPoint => UnityEngine.GUIUtility.pixelsPerPoint;
    }
#endif
}
