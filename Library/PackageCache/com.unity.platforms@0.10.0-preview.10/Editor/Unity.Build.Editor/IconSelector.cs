using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    /// <summary>
    /// Visual element to select icon png file with specific resolution
    /// </summary>
    public sealed class IconSelector : VisualElement
    {
        string m_Path;
        Image m_Image;
        string m_Name;
        int m_Width;
        int m_Height;
        Action<string> m_OnValueChanged;

        /// <summary>
        /// Construct icon selector visual element.
        /// </summary>
        /// <param name="name">User friendly icon name.</param>
        /// <param name="width">Required icon image width.</param>
        /// <param name="height">Required icon image height.</param>
        /// <param name="path">Path to icon file or null if icon has not been set.</param>
        /// <param name="onValueChanged">Action to be called when path to icon is changed.</param>
        public IconSelector(string name, int width, int height, string path, Action<string> onValueChanged) : base()
        {
            m_Path = path;
            m_Name = name;
            m_Width = width;
            m_Height = height;
            m_OnValueChanged = onValueChanged;

            var iconBrowse = new Button(OnBrowse)
            {
                text = $"Browse {m_Name} icon ({m_Width}px x {m_Height}px)"
            };
            Add(iconBrowse);
            m_Image = new Image();
            m_Image.style.width = m_Width > 512 ? m_Width / 4 : m_Width / 2;
            m_Image.style.height = m_Width > 512 ? m_Height / 4 : m_Height / 2;
            m_Image.style.alignSelf = Align.Center;
            LoadImage(m_Path);
            Add(m_Image);
        }

        private void OnBrowse()
        {
            var iconPath = Browse("");
            if (iconPath == null)
            {
                return;
            }
            if (!LoadImage(iconPath))
            {
                return;
            }
            m_Path = iconPath;
            if (m_OnValueChanged != null)
            {
                m_OnValueChanged(m_Path);
            }
        }

        private string Browse(string path)
        {
            var msg = "Select icon file...";
            var defaultFolder = path;

            path = EditorUtility.OpenFilePanel(msg, defaultFolder, "png");
            // user pressed cancel?
            if (path.Length == 0)
            {
                return null;
            }
            return (new FileInfo(path)).GetRelativePath(new DirectoryInfo("."));
        }

        private bool LoadImage(string path)
        {
            m_Image.image = null;
            m_Image.style.display = DisplayStyle.None;
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }
            var texture = new Texture2D(1, 1);
            Byte[] data = null;
            try
            {
                data = File.ReadAllBytes(path);
            }
            catch
            {
            }
            if (data == null || !ImageConversion.LoadImage(texture, data))
            {
                UnityEngine.Debug.LogError($"Cannot load image {path}");
                return false;
            }
            else if (texture.width != m_Width || texture.height != m_Height)
            {
                UnityEngine.Debug.LogError($"Wrong image size {texture.width} x {texture.height} for {m_Name} icon");
                return false;
            }
            m_Image.image = texture;
            m_Image.style.display = DisplayStyle.Flex;
            return true;
        }
    }
}
