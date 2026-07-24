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
            Require(Find("TARGET 2") != null,
                "The first stage must begin with two enemies.");
            Require(CountObjects("Laser") == 3,
                "The player and both initial enemies require aiming lasers.");
            Require(CountObjects("Melee Range") == 1 &&
                CountObjects("Melee Cooldown") == 1,
                "The player requires melee range and cooldown ground rings.");
            Require(CountObjects("Fire Warning Placeholder") == 2,
                "Each initial enemy requires a muzzle warning placeholder.");
            BoxCollider playerHitbox =
                Find("PLAYER").GetComponent<BoxCollider>();
            BoxCollider enemyHitbox =
                Find("TARGET").GetComponent<BoxCollider>();
            Require(playerHitbox != null,
                "Player requires an invisible box hitbox.");
            Require(enemyHitbox != null,
                "Enemies require invisible box hitboxes.");
            Require(Mathf.Approximately(playerHitbox.size.x, .972f) &&
                Mathf.Approximately(playerHitbox.size.y, 1.548f) &&
                Mathf.Approximately(playerHitbox.size.z, .972f),
                "The player hitbox must be 10 percent smaller than the visible body.");
            Require(Mathf.Approximately(enemyHitbox.size.x, 1.188f) &&
                Mathf.Approximately(enemyHitbox.size.y, 1.892f) &&
                Mathf.Approximately(enemyHitbox.size.z, 1.188f),
                "Enemy hitboxes must be 10 percent larger than their visible bodies.");
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
            Require(HasConstant("PlayerHitboxScale", .9f) &&
                HasConstant("EnemyHitboxScale", 1.1f),
                "Player and enemy box hitbox scales must remain asymmetric.");
            Require(HasConstant("ArenaWidth", 24f) &&
                HasConstant("ArenaDepth", 16f),
                "The playable arena must be twice as wide and twice as deep.");
            Require(HasConstant("EmergencyDodgeMultiplier", 2f) &&
                HasConstant("EmergencyDodgeDuration", .5f),
                "Releasing aim must grant a 2x emergency dodge for 0.5 seconds.");
            Require(HasConstant("ProjectileSpeed", 11.7f) &&
                HasConstant("ProjectileRadius", .16f) &&
                HasConstant("PlayerProjectileDamage", 3) &&
                HasConstant("EnemyProjectileDamage", 1),
                "Player projectiles must deal 3 damage while enemy projectiles deal 1.");
            Require(typeof(CountDownGame).GetMethod("FireProjectile",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Hitscan damage must be replaced by projectile travel.");
            Require(typeof(CountDownGame).GetField("PlayerTurnSpeed",
                BindingFlags.Static | BindingFlags.NonPublic) == null,
                "Player aiming must be instantaneous with no turn-speed limit.");
            Require(typeof(CountDownGame).GetMethod("PointerArenaPoint",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Mouse aiming must follow the current pointer position.");
            Require(HasConstant("AimAssistDegrees", 20f) &&
                HasConstant("FinalAimAssistDegrees", 120f) &&
                HasConstant("FinalAimAssistDuration", .3f) &&
                typeof(CountDownGame).GetMethod("ApplyAimAssist",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Player aim must expand from 20 to 120 degrees for the final 0.3 seconds.");
            Require(HasConstant("EnemyLineAvoidanceRadius", 1.45f) &&
                HasConstant("EnemyProjectileDodgeDistance", 3.2f) &&
                typeof(CountDownGame).GetMethod("TryGetProjectileEscape",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Enemies must dodge only when a player projectile gets close.");
            Require(HasConstant("FighterSeparation", 1.08f) &&
                typeof(CountDownGame).GetMethod("ResolveAllFighterOverlaps",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Living fighters must maintain a non-overlapping separation.");
            Require(HasConstant("AimAcquireBonus", .2f) &&
                HasConstant("AimGraceDuration", .5f),
                "Acquiring aim must grant 0.2 seconds and retain progress for 0.5 seconds.");
            Require(HasConstant("DefaultCameraFieldOfView", 47f) &&
                HasConstant("AimingCameraFieldOfView", 42f) &&
                HasConstant("AimFeedbackSpeed", 5f),
                "Aiming must smoothly zoom the camera from 47 to 42 degrees.");
            Require(typeof(CountDownGame).GetMethod("DrawAimVignette",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Aiming must display a dark edge vignette.");
            Require(HasConstant("InitialEnemyCount", 2),
                "Stage one must begin with two enemies.");
            Require(HasConstant("EnemyBaseMoveSpeed", 2.8f) &&
                HasConstant("EnemyMoveSpeedPerStage", .35f) &&
                HasConstant("EnemyStageOneMinCount", 10) &&
                HasConstant("EnemyStageOneMaxCount", 15) &&
                HasConstant("EnemyMinCountFloor", 3) &&
                HasConstant("EnemyMaxCountFloor", 7),
                "Enemy movement speed must rise and starting counts must fall with stage.");
            Require(HasConstant("MeleeRange", 1.8f) &&
                HasConstant("MeleeHitRange", 1.98f) &&
                HasConstant("MeleeCooldown", 15f) &&
                HasConstant("PlayerMeleeRecovery", 1.5f) &&
                HasConstant("PlayerMovementLockedBrightness", .58f) &&
                HasConstant("HitKnockbackDistance", .42f) &&
                typeof(CountDownGame).GetMethod("HasEnemyInMeleeRange",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null &&
                typeof(CountDownGame).GetMethod("CreateMeleeRangeIndicator",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null &&
                typeof(CountDownGame).GetMethod("TryAutoMelee",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Melee recovery must immobilize and visibly dim the player for 1.5 seconds.");
            Require(HasConstant("CameraDistanceMultiplier", 1.15f),
                "The gameplay camera must pull back by 15 percent.");
            Require(typeof(CountDownGame).GetMethod("ResetEnemyCount",
                BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Enemy countdown rolls must balance against other living enemies.");
            Require(HasConstant("EnemyFireWarningDuration", .5f) &&
                typeof(CountDownGame).GetMethod("UpdateEnemyFireWarning",
                    BindingFlags.Instance | BindingFlags.NonPublic) != null,
                "Enemies must stop for their final second and warn 0.5 seconds before firing.");
            Require(HasConstant("SpriteFramesPerSecond", 4f) &&
                HasConstant("FuseFlameFramesPerSecond", 6f) &&
                HasConstant("FuseAlertFramesPerSecond", 5f),
                "Character and fuse sprite animations must play at half speed.");
            Require(HasConstant("FuseSegmentScale", .56f) &&
                HasConstant("FuseFlameScale", .48f),
                "The fuse wick and flame effects must render at double size.");
            Require(HasConstant("FuseSegmentCount", 15),
                "Fuse length must support one wick segment per starting count.");
            Transform playerRoot = Find("PLAYER").transform;
            Transform targetRoot = Find("TARGET").transform;
            Transform playerGun = playerRoot.Find("Gun Pivot");
            Vector3 targetDirection = targetRoot.position +
                Vector3.up * .85f - playerGun.position;
            targetDirection.y = 0f;
            Vector3 rawAssistedDirection =
                Quaternion.Euler(0f, -18f, 0f) * targetDirection.normalized;
            MethodInfo aimAssist = typeof(CountDownGame).GetMethod(
                "ApplyAimAssist", BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 assistedDirection = (Vector3)aimAssist.Invoke(
                game, new object[] { playerGun.position, rawAssistedDirection });
            Require(Vector3.Angle(assistedDirection, targetDirection) < .1f,
                "An enemy inside the 20-degree cone must receive exact center aim.");
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

    private static int CountObjects(string name)
    {
        int count = 0;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            if (candidate.name == name && candidate.scene.IsValid())
                count++;
        return count;
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

    private static bool HasConstant(string name, int expected)
    {
        FieldInfo field = typeof(CountDownGame).GetField(
            name, BindingFlags.Static | BindingFlags.NonPublic);
        return field != null && (int)field.GetRawConstantValue() == expected;
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
