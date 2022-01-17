using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Build.Editor
{
    sealed class PlatformInspector : Inspector<Platform>
    {
        TextElement m_TextElement;

        public override VisualElement Build()
        {
            var root = Resources.TypeInspector.Clone();

            var label = root.Q<Label>("label");
            label.text = DisplayName;

            var input = root.Q<VisualElement>("input");
            input.RegisterCallback<MouseDownEvent>(e =>
            {
                var items = new List<SearchView.Item>();
                foreach (var platform in Platform.AvailablePlatforms)
                {
                    var type = platform.GetType();
                    if (type.HasAttribute<HideInInspector>() ||
                        type.HasAttribute<ObsoleteAttribute>())
                    {
                        continue;
                    }

                    items.Add(new SearchView.Item
                    {
                        Path = platform.DisplayName,
                        Icon = Resources.GetPlatformIcon(platform.IconName),
                        Data = platform
                    });
                }

                items = items.OrderBy(item => item.Path).ToList();

                SearchWindow searchWindow = SearchWindow.Create();
                searchWindow.Title = "Platform";
                searchWindow.Items = items;
                searchWindow.OnSelection += item =>
                {
                    Target = (Platform)item.Data;
                    m_TextElement.text = Target.DisplayName;
                };

                var rect = EditorWindow.focusedWindow.position;
                var button = input.worldBound;
                searchWindow.position = new Rect(rect.x + button.x, rect.y + button.y + button.height, 230, 315);
                searchWindow.ShowPopup();

                e.StopPropagation();
            });

            m_TextElement = input.Q<TextElement>("text");
            m_TextElement.text = Target?.DisplayName;

            return root;
        }

        public override void Update()
        {
            var name = Target?.DisplayName;
            if (m_TextElement.text != name)
            {
                m_TextElement.text = name;
            }
        }
    }
}
