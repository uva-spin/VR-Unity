using UnityEditor;
using UnityEngine;

namespace Unity.Build.Editor
{
    static class GenericMenuExtensions
    {
        public static void AddItem(this GenericMenu menu, string label, bool selected, bool enabled, GenericMenu.MenuFunction function)
        {
            var content = new GUIContent(label);
            if (enabled)
            {
                menu.AddItem(content, selected, function);
            }
            else
            {
                menu.AddDisabledItem(content, selected);
            }
        }

        public static void AddItem(this GenericMenu menu, string label, bool selected, bool enabled, GenericMenu.MenuFunction2 function, object userData)
        {
            var content = new GUIContent(label);
            if (enabled)
            {
                menu.AddItem(content, selected, function, userData);
            }
            else
            {
                menu.AddDisabledItem(content, selected);
            }
        }

        public static void AddDisabledItem(this GenericMenu menu, string label, bool selected = false)
        {
            menu.AddDisabledItem(new GUIContent(label), selected);
        }

        public static void AddSeparator(this GenericMenu menu)
        {
            menu.AddSeparator("");
        }
    }
}
