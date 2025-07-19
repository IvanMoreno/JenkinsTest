using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public static class SimpleBuildScript
{
    public static void Build()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string version = GetArg(args, "-version") ?? "1.0.0";
        string config = GetArg(args, "-config") ?? "Release";
        
        PlayerSettings.bundleVersion = version;
        PlayerSettings.productName = "MyGame";
        
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build", $"MyGame_{version}.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
        
        string[] scenes = GetScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found!");
            EditorApplication.Exit(1);
            return;
        }
        
        BuildOptions options = config == "Debug" ? BuildOptions.Development : BuildOptions.None;
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = options
        };
        
        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded!");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("Build failed!");
            EditorApplication.Exit(1);
        }
    }
    
    private static string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
    
    private static string[] GetScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length > 0)
            return scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
    }
}