using Unity.Build.Bridge;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Editor
{
    static class UIIcon
    {
        public static Texture2D LoadIcon(string path, string name)
        {
            var prefix = EditorGUIUtility.isProSkin ? "d_" : string.Empty;
            var suffix = GUIUtilityBridge.pixelsPerPoint > 1.0 ? "@2x" : string.Empty;
            return EditorGUIUtility.Load($"{path}/{prefix}{name}{suffix}.png") as Texture2D;
        }

        public static Texture2D LoadPackageIcon(string name)
        {
            var path = $"icons/{(EditorGUIUtility.isProSkin ? "dark" : "light")}";
            var suffix = GUIUtilityBridge.pixelsPerPoint > 1.0 ? "@2x" : "";
            return Package.LoadResource<Texture2D>($"{path}/{name}{suffix}.png", true);
        }
    }
}
