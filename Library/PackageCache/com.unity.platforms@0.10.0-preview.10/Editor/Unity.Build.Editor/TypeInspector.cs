using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Build.Editor
{
    /// <summary>
    /// Base inspector class for <see cref="Type"/> searcher field.
    /// </summary>
    /// <typeparam name="T">Type to populate the searcher with.</typeparam>
    public abstract class TypeInspector<T> : Inspector<T>
    {
        TextElement m_TextElement;

        /// <summary>
        /// The title displayed on the searcher window.
        /// </summary>
        public virtual string Title => $"Select {typeof(T).Name}";

        /// <summary>
        /// A function that returns whether or not the type should be filtered.
        /// </summary>
        public virtual Func<Type, bool> TypeFilter { get; }

        /// <summary>
        /// A function that returns the display name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeName { get; }

        /// <summary>
        /// A function that returns the display category name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeCategory { get; }

        /// <summary>
        /// A function that returns the display icon of the type.
        /// </summary>
        public virtual Func<Type, Texture2D> TypeIcon { get; }

        public override VisualElement Build()
        {
            var root = Resources.TypeInspector.Clone();

            var label = root.Q<Label>("label");
            label.text = DisplayName;

            var input = root.Q<VisualElement>("input");
            input.RegisterCallback<MouseDownEvent>(e =>
            {
                var items = new List<SearchView.Item>();
                var types = TypeCache.GetTypesDerivedFrom<T>();
                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface ||
                        type.HasAttribute<HideInInspector>() ||
                        type.HasAttribute<ObsoleteAttribute>())
                    {
                        continue;
                    }

                    if (!TypeConstruction.CanBeConstructed(type))
                    {
                        continue;
                    }

                    if (TypeFilter != null && !TypeFilter(type))
                    {
                        continue;
                    }

                    var name = GetName(type);
                    var category = GetCategory(type);
                    items.Add(new SearchView.Item
                    {
                        Path = !string.IsNullOrEmpty(category) ? $"{category}/{name}" : name,
                        Icon = GetIcon(type),
                        Data = type
                    });
                }
                items = items.OrderBy(item => item.Path).ToList();

                SearchWindow searchWindow = SearchWindow.Create();
                searchWindow.Title = Title;
                searchWindow.Items = items;
                searchWindow.OnSelection += item =>
                {
                    var type = (Type)item.Data;
                    if (TypeConstruction.TryConstruct<T>(type, out var instance))
                    {
                        Target = instance;
                        m_TextElement.text = GetName(type);
                    }
                };

                var rect = EditorWindow.focusedWindow.position;
                var button = input.worldBound;
                searchWindow.position = new Rect(rect.x + button.x, rect.y + button.y + button.height, 230, 315);
                searchWindow.ShowPopup();

                e.StopPropagation();
            });

            m_TextElement = input.Q<TextElement>("text");
            m_TextElement.text = GetName(Target?.GetType());

            return root;
        }

        public override void Update()
        {
            var name = GetName(Target?.GetType());
            if (m_TextElement.text != name)
            {
                m_TextElement.text = name;
            }
        }

        string GetName(Type type) => type != null ? TypeName?.Invoke(type) ?? type.Name : string.Empty;
        string GetCategory(Type type) => type != null ? TypeCategory?.Invoke(type) ?? type.Namespace ?? "Global" : string.Empty;
        Texture2D GetIcon(Type type) => type != null ? TypeIcon?.Invoke(type) : null;
    }
}
