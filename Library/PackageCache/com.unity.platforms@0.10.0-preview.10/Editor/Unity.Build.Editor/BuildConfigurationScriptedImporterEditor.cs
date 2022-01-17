using System;
using System.IO;
using Unity.Properties.UI;
using UnityEditor;
using UnityEngine.UIElements;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Unity.Build.Editor
{
    [CustomEditor(typeof(BuildConfigurationScriptedImporter))]
    class BuildConfigurationScriptedImporterEditor : ScriptedImporterEditor
    {
        VisualElement m_Root;

        public override bool showImportedObject { get; } = false;
        protected override Type extraDataType { get; } = typeof(BuildConfiguration);
        protected override bool needsApplyRevert { get; } = true;
        protected override bool useAssetDrawPreview { get; } = false;

        BuildConfiguration Source => assetTarget as BuildConfiguration;
        BuildConfiguration Target => extraDataTarget as BuildConfiguration;

        public override VisualElement CreateInspectorGUI() => Build();

        protected override void OnHeaderGUI() { }

        protected override void InitializeExtraDataInstance(UnityEngine.Object extraData, int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= targets.Length)
            {
                return;
            }

            var target = targets[targetIndex];
            if (target == null || !target)
            {
                return;
            }

            var assetImporter = target as AssetImporter;
            if (assetImporter == null || !assetImporter)
            {
                return;
            }

            var asset = extraData as BuildConfiguration;
            if (asset == null || !asset)
            {
                return;
            }

            if (BuildConfiguration.DeserializeFromPath(asset, assetImporter.assetPath))
            {
                asset.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
            }

            if (m_Root != null)
            {
                Rebuild();
            }
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var asset = extraDataTargets[i] as BuildConfiguration;
                if (asset == null || !asset)
                {
                    continue;
                }

                asset.SerializeToPath(assetImporter.assetPath);
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var asset = extraDataTargets[i] as BuildConfiguration;
                if (asset == null || !asset)
                {
                    continue;
                }

                if (BuildConfiguration.DeserializeFromPath(asset, assetImporter.assetPath))
                {
                    asset.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
                }
            }
            Rebuild();
        }

        public override bool HasModified()
        {
            if (Source == null || !Source || Target == null || !Target)
                return false;

            return Source.SerializeToJson() != Target.SerializeToJson();
        }

        VisualElement Build()
        {
            if (m_Root == null)
            {
                m_Root = new VisualElement();
            }

            if (Source != null && Source && Target != null && Target)
            {
                var configRoot = new PropertyElement();
                configRoot.SetTarget(new BuildConfigurationInspectorData(Source, Target));
                configRoot.AddContext(new BuildConfigurationContext(this));
                m_Root.contentContainer.Add(configRoot);
            }

            m_Root.contentContainer.Add(new IMGUIContainer(ApplyRevertGUI));
            return m_Root;
        }

        void Rebuild()
        {
            m_Root.Clear();
            Build();
        }

        internal BuildConfiguration HandleUnappliedImportSettings()
        {
            if (HasModified())
            {
                var path = AssetDatabase.GetAssetPath(assetTarget);
                int option = EditorUtility.DisplayDialogComplex("Unapplied import settings",
                    $"Unapplied import settings for '{path}'", "Apply", "Revert", "Cancel");
                switch (option)
                {
                    case 0: // Apply
                        Apply();
                        AssetDatabase.Refresh();
                        break;

                    case 1: // Revert
                        ResetValues();
                        AssetDatabase.Refresh();
                        break;

                    case 2: // Cancel
                        return null;
                }
            }

            if (Source == null || !Source)
            {
                throw new NullReferenceException(nameof(Source));
            }
            return Source;
        }
    }
}
