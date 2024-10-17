using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace LiteRP
{
    
    [DisplayInfo(name = "LiteRP Global Settings Asset", order = CoreUtils.Sections.section4 + 2)]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [DisplayName("LiteRP")]
    public class LiteRPGlobalSettings : RenderPipelineGlobalSettings<LiteRPGlobalSettings, LiteRenderPipeline>
    {
        public const string defaultAssetName = "LiteRPGlobalSettings";
        
        [SerializeField] RenderPipelineGraphicsSettingsContainer m_Settings = new();
        protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;
        
#if UNITY_EDITOR
        internal static string defaultPath => $"Assets/LiteRP/{defaultAssetName}.asset";

        //Making sure there is at least one UniversalRenderPipelineGlobalSettings instance in the project
        internal static LiteRPGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            LiteRPGlobalSettings currentInstance = GraphicsSettings.
                GetSettingsForRenderPipeline<LiteRenderPipeline>() as LiteRPGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<LiteRPGlobalSettings, LiteRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                if (currentInstance != null && !currentInstance.IsAtLastVersion())
                {
                    UpgradeAsset(currentInstance.GetInstanceID());
                    AssetDatabase.SaveAssetIfDirty(currentInstance);
                }

                return currentInstance;
            }

            return null;
        }
#endif 
        
        //版本信息，为了以后做升级
        #region Version system
        internal bool IsAtLastVersion() => k_LastVersion == m_AssetVersion;
        internal const int k_LastVersion = 0;

#pragma warning disable CS0414
        [SerializeField][FormerlySerializedAs("k_AssetVersion")]
        internal int m_AssetVersion = k_LastVersion;
#pragma warning restore CS0414

#if UNITY_EDITOR
        public static void UpgradeAsset(int assetInstanceID)
        {
            if (EditorUtility.InstanceIDToObject(assetInstanceID) is not LiteRPGlobalSettings asset)
                return;

            int assetVersionBeforeUpgrade = asset.m_AssetVersion;
            
            //未来写升级迁移设置的地方
            /*if (asset.m_AssetVersion < 0)
            {
                asset.m_AssetVersion = 0;
            })*/

            // If the asset version has changed, means that a migration step has been executed
            if (assetVersionBeforeUpgrade != asset.m_AssetVersion)
                EditorUtility.SetDirty(asset);
        }
#endif
        #endregion
    }
}
