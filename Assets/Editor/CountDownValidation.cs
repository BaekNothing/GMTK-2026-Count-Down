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
            Require(typeof(CountDownGame).GetMethod("UpdateInputMode",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Mouse, touch, and gamepad input must switch automatically.");
            Require(typeof(CountDownGame).GetMethod("ReadGamepadStick",
                BindingFlags.Static | BindingFlags.NonPublic) != null,
                "Dedicated gamepad stick input is required.");
            Require(typeof(CountDownGame).GetMethod("StartStage",
                BindingFlags.Instance | BindingFlags.NonPublic) != null &&
                typeof(CountDownGame).GetMethod("AdvanceStage",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Clearing a stage must spawn one additional enemy.");
            Require(typeof(CountDownGame).GetField("enemies",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Stage combat must support multiple enemies.");
            Require(HasInputAxis("Gamepad Left X") &&
                HasInputAxis("Gamepad Left Y") &&
                HasInputAxis("Gamepad Right X") &&
                HasInputAxis("Gamepad Right Y"),
                "Both gamepad sticks must have dedicated legacy input axes.");
            Require(HasConstant("EnemyTurnSpeed", 63f),
                "Enemy turn speed must be 30% slower than baseline.");
            Require(HasConstant("ArenaWidth", 24f) &&
                HasConstant("ArenaDepth", 16f),
                "The playable arena must be twice as wide and twice as deep.");
            Require(HasConstant("EmergencyDodgeMultiplier", 2f) &&
                HasConstant("EmergencyDodgeDuration", .5f),
                "Releasing aim must grant a 2x emergency dodge for 0.5 seconds.");
            Require(HasConstant("ProjectileSpeed", 9f) &&
                HasConstant("ProjectileRadius", .16f),
                "Countdown weapons must fire visible, dodgeable projectiles.");
            Require(typeof(CountDownGame).GetMethod("FireProjectile",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Hitscan damage must be replaced by projectile travel.");
            Require(typeof(CountDownGame).GetField("PlayerTurnSpeed",
                BindingFlags.Static | BindingFlags.NonPublic) == null,
                "Player aiming must be instantaneous with no turn-speed limit.");
            Require(typeof(CountDownGame).GetMethod("PointerArenaPoint",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Mouse aiming must follow the current pointer position.");
            Require(HasConstant("AimAssistDegrees", 10f) &&
                typeof(CountDownGame).GetMethod("ApplyAimAssist",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Player aim must snap to the nearest overlapping target within 10 degrees.");
            Require(HasConstant("EnemyLineAvoidanceRadius", 1.45f) &&
                typeof(CountDownGame).GetMethod("MoveEnemy",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Enemies must continuously move to escape the player's firing line.");
            Require(HasConstant("FighterSeparation", 1.08f) &&
                typeof(CountDownGame).GetMethod("ResolveAllFighterOverlaps",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Living fighters must maintain a non-overlapping separation.");
            Transform playerRoot = Find("PLAYER").transform;
            Transform targetRoot = Find("TARGET").transform;
            Transform playerGun = playerRoot.Find("Gun Pivot");
            Vector3 targetDirection = targetRoot.position +
                Vector3.up * .85f - playerGun.position;
            targetDirection.y = 0f;
            Vector3 rawAssistedDirection =
                Quaternion.Euler(0f, 8f, 0f) * targetDirection.normalized;
            MethodInfo aimAssist = typeof(CountDownGame).GetMethod(
                "ApplyAimAssist", BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 assistedDirection = (Vector3)aimAssist.Invoke(
                game, new object[] { playerGun.position, rawAssistedDirection });
            Require(Vector3.Angle(assistedDirection, targetDirection) < .1f,
                "An enemy inside the 10-degree cone must receive exact center aim.");
            targetRoot.position = playerRoot.position;
            MethodInfo resolveOverlaps = typeof(CountDownGame).GetMethod(
                "ResolveAllFighterOverlaps",
                BindingFlags.Instance | BindingFlags.NonPublic);
            resolveOverlaps.Invoke(game, null);
            Vector3 separation = targetRoot.position - playerRoot.position;
            separation.y = 0f;
            Require(separation.magnitude >= 1.079f,
                "Overlap resolution must physically separate living fighters.");
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
            Require(typeof(CountDownGame).GetMethod("CreateBuildVersionTexture",
                BindingFlags.Static | BindingFlags.NonPublic) != null,
                "Build version text must use an embedded WebGL-safe bitmap font.");
            Require(typeof(CountDownGame).GetField("versionStyle",
                BindingFlags.Instance | BindingFlags.NonPublic) == null,
                "Build version visibility must not depend on the runtime GUI font.");
            Require(!PlayerSettings.SplashScreen.show &&
                !PlayerSettings.SplashScreen.showUnityLogo,
                "The Unity startup splash and logo must be disabled.");
            Require(typeof(WebGLProjectSetup).GetMethod(
                "RemoveUnityBrandingFromWebPage",
                BindingFlags.Static | BindingFlags.NonPublic) != null,
                "The WebGL loading and footer Unity logos must be removed.");

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

    private static bool HasInputAxis(string name)
    {
        UnityEngine.Object[] assets =
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
        if (assets.Length == 0) return false;
        var inputManager = new SerializedObject(assets[0]);
        SerializedProperty axes = inputManager.FindProperty("m_Axes");
        for (int i = 0; i < axes.arraySize; i++)
        {
            SerializedProperty axis = axes.GetArrayElementAtIndex(i);
            if (axis.FindPropertyRelative("m_Name").stringValue == name)
                return true;
        }
        return false;
    }
}
