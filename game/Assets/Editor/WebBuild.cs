// WebGL build entry point. Run via:
//   Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath game \
//     -executeMethod Reebles2D.Editor.WebBuild.Build -quit

using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Reebles2D.Editor
{
    /// <summary>
    /// Builds the WebGL player into game/Builds/Web for the GitHub Pages deploy.
    /// Compression is disabled because Pages' CDN applies transport compression
    /// itself and cannot emit the Content-Encoding headers Unity's .br files need.
    /// </summary>
    public static class WebBuild
    {
        private const string OutputPath = "Builds/Web";
        private static readonly string[] Scenes = { "Assets/Scenes/Village.unity" };

        public static void Build()
        {
            PlayerSettings.productName = "Reebles 2D";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            BuildReport report = BuildPipeline.BuildPlayer(
                new BuildPlayerOptions
                {
                    scenes = Scenes,
                    locationPathName = OutputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                });

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"WebGL build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }

            Debug.Log($"WebGL build succeeded: {report.summary.totalSize / (1024 * 1024)} MB in {report.summary.totalTime}");
        }
    }
}
