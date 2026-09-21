using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build Mac Player")]
    public static void BuildMacOS()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        string buildPath = "/Applications/FarmFortune.app";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneOSX;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting macOS build to: " + buildPath);
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build SUCCEEDED! Total size: {summary.totalSize} bytes, Time: {summary.totalTime}");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"Build FAILED! Result: {summary.result}, Total errors: {summary.totalErrors}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
