using System;
using System.Reflection;
using CountDown;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CountDownValidation
{
    public static void ValidateRuntimeAssembly()
    {
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var host = new GameObject("Validation Host");
            var game = host.AddComponent<CountDownGame>();

            // AddComponent invokes Awake in this editor context. Keep the
            // reflection fallback for headless versions that defer it.
            if (Find("PLAYER") == null)
            {
                MethodInfo awake = typeof(CountDownGame).GetMethod(
                    "Awake", BindingFlags.Instance | BindingFlags.NonPublic);
                if (awake == null)
                    throw new InvalidOperationException("CountDownGame.Awake was not found.");
                awake.Invoke(game, null);
            }
            Physics.SyncTransforms();

            Require(Find("PLAYER") != null, "Player was not generated.");
            Require(Find("TARGET") != null, "Enemy was not generated.");
            Require(Find("Replaceable Arena Floor") != null, "Arena was not generated.");
            Require(Find("Game Camera") != null, "Game camera was not generated.");
            Require(UnityEngine.Object.FindObjectsByType<LineRenderer>(
                FindObjectsSortMode.None).Length == 2, "Both aiming lasers are required.");
            Require(Find("PLAYER").GetComponent<CapsuleCollider>() != null,
                "Player body hitbox is required.");
            Require(Find("TARGET").GetComponent<CapsuleCollider>() != null,
                "Enemy body hitbox is required.");
            Require(Find("PLAYER").transform.position.y > 0f &&
                Find("TARGET").transform.position.y > 0f,
                "Fighters must remain visibly above the arena floor.");
            Require(typeof(CountDownGame).GetMethod("RestartGame",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Safe in-place restart is required.");
            Require(typeof(CountDownGame).GetMethod("UpdateTouchControls",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Mobile touch controls are required.");
            Require(HasConstant("EnemyTurnSpeed", 63f),
                "Enemy turn speed must be 30% slower than baseline.");
            Require(typeof(CountDownGame).GetField("PlayerTurnSpeed",
                BindingFlags.Static | BindingFlags.NonPublic) == null,
                "Player aiming must be instantaneous with no turn-speed limit.");
            Require(typeof(CountDownGame).GetMethod("PointerArenaPoint",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Mouse aiming must follow the current pointer position.");
            Renderer floorRenderer = Find("Replaceable Arena Floor").GetComponent<Renderer>();
            Require(floorRenderer.material.renderQueue == 1000 &&
                floorRenderer.sortingOrder == -1000,
                "Arena floor must render behind gameplay objects.");
            Renderer railRenderer = Find("Arena Rail").GetComponent<Renderer>();
            Require(railRenderer.material.renderQueue == 1002 &&
                railRenderer.sortingOrder == -998,
                "Arena rails must render behind gameplay objects.");
            Require(typeof(CountDownGame).GetMethod("DrawBuildVersion",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "A safe-area anchored build version overlay is required.");

            Debug.Log("COUNT DOWN runtime validation passed.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static GameObject Find(string name)
    {
        foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            if (candidate.name == name && candidate.scene.IsValid())
                return candidate;
        return null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static bool HasConstant(string name, float expected)
    {
        FieldInfo field = typeof(CountDownGame).GetField(
            name, BindingFlags.Static | BindingFlags.NonPublic);
        return field != null && Mathf.Approximately(
            (float)field.GetRawConstantValue(), expected);
    }
}
