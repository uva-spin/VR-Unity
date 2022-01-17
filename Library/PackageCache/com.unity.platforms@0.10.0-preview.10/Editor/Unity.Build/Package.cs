using System;
using System.IO;
using UnityEditor;

namespace Unity.Build
{
    internal class Package
    {
        public const string PackageName = "com.unity.platforms";
        public const string PackagePath = "Packages/" + PackageName;
        public const string EditorDefaultResourcesPath = PackagePath + "/Editor Default Resources";

        public static T LoadResource<T>(string path, bool required) where T : UnityEngine.Object
        {
            var resourcePath = Path.Combine(EditorDefaultResourcesPath, path).Replace('\\', '/');
            if (!File.Exists(resourcePath))
            {
                if (required)
                {
                    throw new FileNotFoundException($"Missing resource at {resourcePath.ToHyperLink()}.");
                }
                else
                {
                    return null;
                }
            }

            var resource = EditorGUIUtility.Load(resourcePath) as T;
            if (resource == null || !resource)
            {
                if (required)
                {
                    throw new InvalidOperationException($"Failed to load resource at {resourcePath.ToHyperLink()}.");
                }
                else
                {
                    return null;
                }
            }

            return resource;
        }
    }
}
