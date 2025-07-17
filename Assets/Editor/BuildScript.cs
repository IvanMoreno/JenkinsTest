using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    public static void BuildWindows()
    {
        Debug.Log("=== STARTING BUILD IN JENKINS WORKSPACE ===");
        
        // Parse command line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        var buildParams = ParseBuildParameters(args);
        
        Debug.Log($"Build Version: {buildParams.version}");
        Debug.Log($"Build Configuration: {buildParams.configuration}");
        Debug.Log($"Output Path: {buildParams.outputPath}");
        Debug.Log($"Product Name: {buildParams.productName}");
        Debug.Log($"Company Name: {buildParams.companyName}");
        
        try
        {
            // Set player settings
            PlayerSettings.bundleVersion = buildParams.version;
            PlayerSettings.companyName = buildParams.companyName;
            PlayerSettings.productName = buildParams.productName;
            
            // Build path structure: outputPath/ProductName_Version_Configuration/ProductName.exe
            string buildFolderName = $"{buildParams.productName}_{buildParams.version}_{buildParams.configuration}";
            string buildFileName = $"{buildParams.productName}.exe";
            string buildFolder = Path.Combine(buildParams.outputPath, buildFolderName);
            string fullBuildPath = Path.Combine(buildFolder, buildFileName);
            
            Debug.Log($"Build folder: {buildFolder}");
            Debug.Log($"Full build path: {fullBuildPath}");
            
            // Create build directory
            if (Directory.Exists(buildFolder))
            {
                Directory.Delete(buildFolder, true);
                Debug.Log($"Cleaned existing build directory: {buildFolder}");
            }
            Directory.CreateDirectory(buildFolder);
            Debug.Log($"Created build directory: {buildFolder}");
            
            // Get scenes to build
            var scenes = GetScenesToBuild();
            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes found to build!");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log($"Building {scenes.Length} scene(s):");
            foreach (var scene in scenes)
            {
                Debug.Log($"  - {scene}");
            }
            
            // Configure build options based on configuration
            BuildOptions buildOptions = BuildOptions.None;
            switch (buildParams.configuration.ToLower())
            {
                case "debug":
                    buildOptions |= BuildOptions.Development | BuildOptions.AllowDebugging;
                    Debug.Log("Debug build configuration enabled");
                    break;
                case "development":
                    buildOptions |= BuildOptions.Development;
                    Debug.Log("Development build configuration enabled");
                    break;
                case "release":
                default:
                    Debug.Log("Release build configuration enabled");
                    break;
            }
            
            // Configure build player options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullBuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions
            };
            
            Debug.Log($"Starting build to: {fullBuildPath}");
            Debug.Log($"Target platform: {buildPlayerOptions.target}");
            Debug.Log($"Build options: {buildOptions}");
            
            // Perform the build
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Create build artifacts and info
            CreateBuildArtifacts(buildFolder, buildParams, report, fullBuildPath);
            
            // Check build result
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("✓ BUILD SUCCESSFUL!");
                Debug.Log($"Build output location: {buildFolder}");
                Debug.Log($"Executable: {fullBuildPath}");
                Debug.Log($"Build size: {report.summary.totalSize} bytes");
                Debug.Log($"Build time: {report.summary.totalTime}");
                Debug.Log($"Build warnings: {report.summary.totalWarnings}");
                Debug.Log($"Build errors: {report.summary.totalErrors}");
                
                // List build output files
                Debug.Log("Build output files:");
                string[] buildFiles = Directory.GetFiles(buildFolder, "*", SearchOption.AllDirectories);
                foreach (string file in buildFiles)
                {
                    Debug.Log($"  {Path.GetRelativePath(buildFolder, file)}");
                }
                
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"✗ BUILD FAILED: {report.summary.result}");
                Debug.LogError($"Build errors: {report.summary.totalErrors}");
                Debug.LogError($"Build warnings: {report.summary.totalWarnings}");
                
                LogBuildErrors(report);
                EditorApplication.Exit(1);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Build script exception: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            EditorApplication.Exit(1);
        }
    }
    
    private static BuildParameters ParseBuildParameters(string[] args)
    {
        var parameters = new BuildParameters();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-buildversion":
                    if (i + 1 < args.Length) 
                    {
                        parameters.version = args[i + 1];
                        Debug.Log($"Parsed build version: {parameters.version}");
                    }
                    break;
                case "-buildconfig":
                    if (i + 1 < args.Length) 
                    {
                        parameters.configuration = args[i + 1];
                        Debug.Log($"Parsed build configuration: {parameters.configuration}");
                    }
                    break;
                case "-outputpath":
                    if (i + 1 < args.Length) 
                    {
                        parameters.outputPath = args[i + 1];
                        Debug.Log($"Parsed output path: {parameters.outputPath}");
                    }
                    break;
                case "-companyname":
                    if (i + 1 < args.Length) 
                    {
                        parameters.companyName = args[i + 1];
                        Debug.Log($"Parsed company name: {parameters.companyName}");
                    }
                    break;
                case "-productname":
                    if (i + 1 < args.Length) 
                    {
                        parameters.productName = args[i + 1];
                        Debug.Log($"Parsed product name: {parameters.productName}");
                    }
                    break;
            }
        }
        
        return parameters;
    }
    
    private static string[] GetScenesToBuild()
    {
        // First, try to get scenes from build settings
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length > 0 && scenes.Any(s => s.enabled))
        {
            var enabledScenes = scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            Debug.Log($"Found {enabledScenes.Length} enabled scene(s) in build settings");
            return enabledScenes;
        }
        
        // If no scenes in build settings, auto-discover all scenes
        Debug.Log("No scenes found in build settings, auto-discovering scenes...");
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        if (guids.Length > 0)
        {
            var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                sceneList.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"Auto-discovered scene: {path}");
            }
            
            // Update build settings with discovered scenes
            EditorBuildSettings.scenes = sceneList.ToArray();
            return sceneList.Select(s => s.path).ToArray();
        }
        
        Debug.LogWarning("No scenes found in project!");
        return new string[0];
    }
    
    private static void CreateBuildArtifacts(string buildFolder, BuildParameters parameters, UnityEditor.Build.Reporting.BuildReport report, string executablePath)
    {
        try
        {
            // Create detailed build info file
            string infoPath = Path.Combine(buildFolder, "BUILD_INFO.txt");
            var info = new System.Text.StringBuilder();
            
            info.AppendLine("=====================================");
            info.AppendLine("       BUILD INFORMATION");
            info.AppendLine("=====================================");
            info.AppendLine($"Product Name: {parameters.productName}");
            info.AppendLine($"Version: {parameters.version}");
            info.AppendLine($"Configuration: {parameters.configuration}");
            info.AppendLine($"Company: {parameters.companyName}");
            info.AppendLine($"Build Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine($"Unity Version: {Application.unityVersion}");
            info.AppendLine($"Build Target: {report.summary.platform}");
            info.AppendLine($"Build Result: {report.summary.result}");
            info.AppendLine($"Build Size: {report.summary.totalSize} bytes ({report.summary.totalSize / 1024 / 1024:F2} MB)");
            info.AppendLine($"Build Time: {report.summary.totalTime}");
            info.AppendLine($"Build Errors: {report.summary.totalErrors}");
            info.AppendLine($"Build Warnings: {report.summary.totalWarnings}");
            info.AppendLine($"Executable: {Path.GetFileName(executablePath)}");
            info.AppendLine($"Build Location: {buildFolder}");
            info.AppendLine("=====================================");
            
            // Add scene information
            info.AppendLine("SCENES INCLUDED:");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled);
            foreach (var scene in scenes)
            {
                info.AppendLine($"  - {scene.path}");
            }
            info.AppendLine("=====================================");
            
            File.WriteAllText(infoPath, info.ToString());
            Debug.Log($"Build info written to: {infoPath}");
            
            // Create a batch file to easily run the game
            string batchPath = Path.Combine(buildFolder, "RUN_GAME.bat");
            string batchContent = $"@echo off\n" +
                                 $"echo Starting {parameters.productName} v{parameters.version}...\n" +
                                 $"echo Configuration: {parameters.configuration}\n" +
                                 $"echo.\n" +
                                 $"if exist \"{Path.GetFileName(executablePath)}\" (\n" +
                                 $"    start \"\" \"{Path.GetFileName(executablePath)}\"\n" +
                                 $"    echo Game started successfully!\n" +
                                 $") else (\n" +
                                 $"    echo ERROR: Game executable not found!\n" +
                                 $"    echo Expected: {Path.GetFileName(executablePath)}\n" +
                                 $")\n" +
                                 $"echo.\n" +
                                 $"pause";
            
            File.WriteAllText(batchPath, batchContent);
            Debug.Log($"Run script created: {batchPath}");
            
            // Create a simple readme file
            string readmePath = Path.Combine(buildFolder, "README.txt");
            string readmeContent = $"{parameters.productName} v{parameters.version}\n" +
                                  $"Build Configuration: {parameters.configuration}\n" +
                                  $"Built on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                  $"To run the game:\n" +
                                  $"1. Double-click '{Path.GetFileName(executablePath)}' to start\n" +
                                  $"2. Or use 'RUN_GAME.bat' for a more detailed startup\n\n" +
                                  $"For more information, see BUILD_INFO.txt\n";
            
            File.WriteAllText(readmePath, readmeContent);
            Debug.Log($"README created: {readmePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create build artifacts: {e.Message}");
        }
    }
    
    private static void LogBuildErrors(UnityEditor.Build.Reporting.BuildReport report)
    {
        Debug.LogError("=== BUILD ERRORS AND WARNINGS ===");
        
        foreach (var step in report.steps)
        {
            foreach (var message in step.messages)
            {
                switch (message.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        Debug.LogError($"ERROR: {message.content}");
                        break;
                    case LogType.Warning:
                        Debug.LogWarning($"WARNING: {message.content}");
                        break;
                }
            }
        }
        
        Debug.LogError("=== END BUILD ERRORS ===");
    }
    
    private struct BuildParameters
    {
        public string version;
        public string configuration;
        public string outputPath;
        public string companyName;
        public string productName;
        
        public BuildParameters(string defaultVersion = "1.0.0")
        {
            version = defaultVersion;
            configuration = "Release";
            outputPath = @"C:\temp\builds"; // Default fallback
            companyName = "Your Company";
            productName = "ChristmasLearningProject";
        }
    }
}