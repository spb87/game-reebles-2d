// One-shot project bootstrap: creates the URP 2D renderer + pipeline assets
// and assigns them to Graphics/Quality settings. Run via:
//   Unity.exe -batchmode -projectPath game -executeMethod Reebles2D.Editor.ProjectBootstrap.Setup -quit

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Reebles2D.Editor
{
    /// <summary>
    /// Batch-mode bootstrap that wires the URP 2D pipeline into project settings.
    /// Idempotent: reuses existing assets if they already exist.
    /// </summary>
    public static class ProjectBootstrap
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string RendererPath = SettingsFolder + "/Reebles2DRenderer.asset";
        private const string PipelinePath = SettingsFolder + "/Reebles2DPipeline.asset";

        /// <summary>
        /// Creates the Renderer2DData and UniversalRenderPipelineAsset under
        /// Assets/Settings and assigns them to GraphicsSettings and QualitySettings.
        /// </summary>
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            Renderer2DData rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            UniversalRenderPipelineAsset pipelineAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            // Assign per quality level so switching levels never falls back to built-in.
            int currentLevel = QualitySettings.GetQualityLevel();
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
            QualitySettings.SetQualityLevel(currentLevel, false);

            AssetDatabase.SaveAssets();

            Debug.Log("ProjectBootstrap: URP 2D pipeline assigned at " + PipelinePath);
        }
    }
}
