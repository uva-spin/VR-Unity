using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    static class VisualElementExtensions
    {
        public static void AddToClassList(this VisualElement element, bool condition, string className)
        {
            if (condition)
            {
                element.AddToClassList(className);
            }
            else
            {
                element.RemoveFromClassList(className);
            }
        }

        public static void AddToClassList(this VisualElement element, bool condition, string classNameTrue, string classNameFalse)
        {
            if (condition)
            {
                element.RemoveFromClassList(classNameFalse);
                element.AddToClassList(classNameTrue);
            }
            else
            {
                element.RemoveFromClassList(classNameTrue);
                element.AddToClassList(classNameFalse);
            }
        }
    }
}
