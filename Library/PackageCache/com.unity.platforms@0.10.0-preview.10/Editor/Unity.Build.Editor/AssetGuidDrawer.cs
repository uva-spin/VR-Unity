using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    abstract class AssetGuidInspectorBase<T> : PropertyDrawer<T, AssetGuidAttribute>
    {
        protected ObjectField m_ObjectField;

        public override VisualElement Build()
        {
            m_ObjectField = new ObjectField(DisplayName)
            {
                allowSceneObjects = false
            };

            var asset = GetAttribute<AssetGuidAttribute>();
            Assert.IsTrue(typeof(UnityEngine.Object).IsAssignableFrom(asset.Type));
            m_ObjectField.objectType = asset.Type;
            m_ObjectField.RegisterValueChangedCallback(OnChanged);

            OnBuild();

            return m_ObjectField;
        }

        protected abstract void OnBuild();
        protected abstract void OnChanged(ChangeEvent<UnityEngine.Object> evt);
    }

    sealed class GuidAssetInspector : AssetGuidInspectorBase<GUID>
    {
        protected override void OnBuild()
        {
            m_ObjectField.value = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(Target.ToString()));
        }

        protected override void OnChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (null != evt.newValue && evt.newValue)
            {
                Target = new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue)));
            }
            else
            {
                Target = default;
            }
        }
    }

    sealed class GlobalObjectIdAssetInspector : AssetGuidInspectorBase<GlobalObjectId>
    {
        static GlobalObjectId k_DefaultObjectId = new GlobalObjectId();

        protected override void OnBuild()
        {
            m_ObjectField.value = Target.assetGUID != k_DefaultObjectId.assetGUID ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(Target) : null;
        }

        protected override void OnChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (null != evt.newValue && evt.newValue)
            {
                Target = GlobalObjectId.GetGlobalObjectIdSlow(evt.newValue);
            }
            else
            {
                Target = default;
            }
        }
    }
}
