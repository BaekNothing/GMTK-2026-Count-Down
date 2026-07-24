using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WebGLProjectSetup
{
    private const string SceneDirectory = "Assets/Scenes";
    private const string ScenePath = SceneDirectory + "/Main.unity";

    public static void Configure()
    {
        Directory.CreateDirectory(SceneDirectory);

        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.DefaultGameObjects,
                NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.WebGL,
                BuildTarget.WebGL))
        {
            throw new System.InvalidOperationException(
                "Failed to switch the active build target to WebGL.");
        }

        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        Debug.Log("WebGL project setup completed.");
    }

    public static void Build()
    {
        Configure();
        Directory.CreateDirectory("Builds/WebGL");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.InvalidOperationException(
                $"WebGL build failed: {report.summary.result}");
        }

        Debug.Log(
            $"WebGL build completed: {report.summary.totalSize} bytes");
    }
}
