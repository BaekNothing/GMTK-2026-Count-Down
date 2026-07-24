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

        AddControlsToWebPage("Builds/WebGL/index.html");
        Debug.Log(
            $"WebGL build completed: {report.summary.totalSize} bytes");
    }

    private static void AddControlsToWebPage(string indexPath)
    {
        string html = File.ReadAllText(indexPath);
        const string marker = "<!-- COUNT DOWN CONTROLS -->";
        if (html.Contains(marker)) return;

        const string controls = @"
  <!-- COUNT DOWN CONTROLS -->
  <section style=""max-width:960px;margin:18px auto;padding:16px 20px;color:#e5e7eb;background:#111827;font:16px/1.5 sans-serif;border-radius:10px"">
    <strong>HOW TO PLAY</strong><br>
    Desktop: WASD to move · Hold right mouse to aim horizontally · Space to bash · R to restart<br>
    Mobile: Drag the left side to move · Hold and drag the right side to aim · Tap after game over to restart
  </section>
</body>";
        html = html.Replace("</body>", controls);
        File.WriteAllText(indexPath, html);
    }
}
