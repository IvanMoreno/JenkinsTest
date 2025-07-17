using System.IO;
using UnityEditor;
using static UnityEditor.Build.Reporting.BuildResult;

public static class BuildScript
{
    public static void BuildWindows(string buildPath)
    {
        try
        {
            var buildFolder = Path.GetDirectoryName(buildPath);

            if (!Directory.Exists(buildFolder))
                Directory.CreateDirectory(buildFolder);

            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                EditorApplication.Exit(1);
                return;
            }

            var scenePaths = new string[scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            EditorApplication.Exit(report.summary.result == Succeeded ? 0 : 1);
        }
        catch (System.Exception e)
        {
            EditorApplication.Exit(1);
        }
    }
}