using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CountDown
{
    /// <summary>
    /// A self-contained, resource-optional implementation of COUNT DOWN.
    /// Put matching assets under Resources/CountDown to replace the fallbacks.
    /// </summary>
    public sealed class CountDownGame : MonoBehaviour
    {
        private const float ArenaWidth = 24f;
        private const float ArenaDepth = 16f;
        private const float BodyRadius = .48f;
        private const float BodyHeight = 1.7f;
        private const float BodyVisualWidth = 1.08f;
        private const float BodyVisualHeight = 1.72f;
        private const float PlayerHitboxScale = .9f;
        private const float EnemyHitboxScale = 1.1f;
        private const float FighterGroundHeight = .04f;
        private const int MaxHealth = 3;
        private const float NormalMoveSpeed = 5f;
        private const float AimMoveSpeed = 2f;
        private const float EmergencyDodgeMultiplier = 2f;
        private const float EmergencyDodgeDuration = .5f;
        private const float ProjectileSpeed = 11.7f;
        private const float PlayerProjectileSpeedMultiplier = 1.1f;
        private const float ProjectileRadius = .16f;
        private const float ProjectileLifetime = 4f;
        private const int PlayerProjectileDamage = 3;
        private const int EnemyProjectileDamage = 1;
        private const float GamepadDeadzone = .2f;
        private const float AimAssistDegrees = 15f;
        private const float FighterSeparation = BodyRadius * 2f + .12f;
        private const float EnemyLineAvoidanceRadius = 1.45f;
        private const float EnemyProjectileDodgeDistance = 3.2f;
        private const float EnemyFireWarningDuration = .5f;
        private const float HitKnockbackDistance = .42f;
        private const float AimAcquireBonus = .2f;
        private const float AimGraceDuration = .5f;
        private const float DefaultCameraFieldOfView = 47f;
        private const float AimingCameraFieldOfView = 42f;
        private const float AimFeedbackSpeed = 5f;
        private const int InitialEnemyCount = 2;
        private const float EnemyBaseMoveSpeed = 2.52f;
        private const float EnemyMoveSpeedPerStage = .315f;
        private const float EnemyMaxMoveSpeed = 5.4f;
        private const int EnemyStageOneMinCount = 10;
        private const int EnemyStageOneMaxCount = 15;
        private const int EnemyMinCountFloor = 3;
        private const int EnemyMaxCountFloor = 7;
        private const float MeleeRange = 1.8f;
        private const float MeleeHitRange = MeleeRange * 1.1f;
        private const float MeleeCooldown = 15f;
        private const float PlayerMeleeRecovery = 1.5f;
        private const float PlayerMovementLockedBrightness = .58f;
        private const float CameraDistanceMultiplier = 1.15f;
        private const float SpriteFramesPerSecond = 4f;
        private const int FuseSegmentCount = EnemyStageOneMaxCount;
        private const float FuseLinkLength = .104f;
        private const float FuseSegmentScale = .56f;
        private const float FuseFlameScale = .48f;
        private const float FuseFlameFramesPerSecond = 6f;
        private const float FuseAlertFramesPerSecond = 5f;
        private const float PreFirePause = .42f;

        private enum InputMode
        {
            Mouse,
            Touch,
            Gamepad
        }

        private Fighter player;
        private readonly List<Fighter> enemies = new List<Fighter>();
        private readonly List<ProjectileThreat> projectileThreats =
            new List<ProjectileThreat>();
        private Camera gameCamera;
        private AudioSource audioSource;
        private bool finished;
        private bool stageTransitioning;
        private bool awaitingStart = true;
        private int stage = 1;
        private float meleeReadyAt;
        private float emergencyDodgeUntil;
        private float shake;
        private float aimFeedback;
        private Texture2D white;
        private Texture2D aimVignette;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle countStyle;
        private GUIStyle helpStyle;
        private GUIStyle aimStyle;
        private GUIStyle guideTitleStyle;
        private GUIStyle guideLabelStyle;
        private GUIStyle guideKeyStyle;
        private Texture2D buildVersionTexture;
        private int moveTouchId = -1;
        private int aimTouchId = -1;
        private Vector2 moveTouchOrigin;
        private Vector2 aimTouchOrigin;
        private Vector2 moveTouchPosition;
        private Vector2 aimTouchPosition;
        private Vector2 touchMove;
        private Vector2 touchAim;
        private Vector2 mouseAimPosition;
        private Vector2 aimDirection = Vector2.up;
        private bool touchAiming;
        private InputMode inputMode = InputMode.Mouse;
        private const float TouchStickRadius = 72f;
        private const float EnemyTurnSpeed = 63f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameExists()
        {
            if (FindFirstObjectByType<CountDownGame>() == null)
                new GameObject("COUNT DOWN Runtime").AddComponent<CountDownGame>();
        }

        private void Awake()
        {
            Application.runInBackground = true;
            white = Texture2D.whiteTexture;
            aimVignette = CreateAimVignetteTexture();
            mouseAimPosition = new Vector2(Screen.width * .5f, Screen.height * .5f);
            BuildWorld();
        }

        private void BuildWorld()
        {
            foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                camera.gameObject.SetActive(false);

            RenderSettings.ambientLight = new Color(.48f, .5f, .54f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.055f, .065f, .08f);
            RenderSettings.fogDensity = .018f;

            gameCamera = NewObject("Game Camera").AddComponent<Camera>();
            gameCamera.tag = "MainCamera";
            gameCamera.fieldOfView = DefaultCameraFieldOfView;
            gameCamera.nearClipPlane = .1f;
            gameCamera.farClipPlane = 80f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(.045f, .052f, .065f);
            gameCamera.gameObject.AddComponent<AudioListener>();

            var sun = NewObject("Arena Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1f, .91f, .78f);
            sun.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            CreateArena();
            player = CreateFighter("PLAYER", true,
                new Vector3(0f, FighterGroundHeight, -2.75f),
                new Color(.12f, .72f, 1f), "Player");
            player.ResetCount();
            StartStage();

            audioSource = NewObject("Audio").AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            meleeReadyAt = Time.time;
            UpdateCamera(true);
        }

        private void StartStage()
        {
            enemies.Clear();
            int enemyCount = InitialEnemyCount + stage - 1;
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 position = EnemySpawnPosition(i, enemyCount);
                string name = i == 0 ? "TARGET" : "TARGET " + (i + 1);
                Fighter spawned = CreateFighter(name, false,
                    position, new Color(1f, .24f, .18f), "Enemy");
                spawned.opponent = player;
                spawned.countMin = Mathf.Max(EnemyMinCountFloor,
                    EnemyStageOneMinCount - (stage - 1));
                spawned.countMax = Mathf.Max(EnemyMaxCountFloor,
                    EnemyStageOneMaxCount - (stage - 1));
                enemies.Add(spawned);
                ResetEnemyCount(spawned);
            }
            player.opponent = null;
            player.hasTarget = false;
            stageTransitioning = false;
        }

        private static Vector3 EnemySpawnPosition(int index, int count)
        {
            int columns = Mathf.Min(7, Mathf.CeilToInt(Mathf.Sqrt(count * 1.5f)));
            int rows = Mathf.CeilToInt(count / (float)columns);
            int column = index % columns;
            int row = index / columns;
            float x = (column - (columns - 1) * .5f) * 2.3f;
            float rowSpacing = Mathf.Min(2.3f, 9f / Mathf.Max(1, rows - 1));
            float z = 2f + (row - (rows - 1) * .5f) * rowSpacing;
            return new Vector3(x, FighterGroundHeight, z);
        }

        private void CreateArena()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Replaceable Arena Floor";
            floor.transform.SetParent(transform);
            floor.transform.position = new Vector3(0f, -.18f, 0f);
            floor.transform.localScale = new Vector3(ArenaWidth, .3f, ArenaDepth);
            Renderer floorRenderer = floor.GetComponent<Renderer>();
            floorRenderer.material = MaterialFor(
                "Materials/Floor", new Color(.13f, .145f, .16f), 1000);
            floorRenderer.sortingOrder = -1000;

            CreateBoundary(new Vector3(0f, .15f, ArenaDepth * .5f + .2f),
                new Vector3(ArenaWidth + .8f, .3f, .18f));
            CreateBoundary(new Vector3(0f, .15f, -ArenaDepth * .5f - .2f),
                new Vector3(ArenaWidth + .8f, .3f, .18f));
            CreateBoundary(new Vector3(ArenaWidth * .5f + .2f, .15f, 0f),
                new Vector3(.18f, .3f, ArenaDepth));
            CreateBoundary(new Vector3(-ArenaWidth * .5f - .2f, .15f, 0f),
                new Vector3(.18f, .3f, ArenaDepth));

            int halfWidth = Mathf.FloorToInt(ArenaWidth * .5f);
            int halfDepth = Mathf.FloorToInt(ArenaDepth * .5f);
            for (int i = -halfWidth + 1; i < halfWidth; i++)
                CreateStripe(new Vector3(i, .006f, 0f), new Vector3(.018f, .01f, ArenaDepth));
            for (int i = -halfDepth + 1; i < halfDepth; i++)
                CreateStripe(new Vector3(0f, .007f, i), new Vector3(ArenaWidth, .01f, .018f));
        }

        private void CreateBoundary(Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Arena Rail";
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.material = MaterialFor(null, new Color(.65f, .58f, .38f), 1002);
            renderer.sortingOrder = -998;
            Destroy(go.GetComponent<Collider>());
        }

        private void CreateStripe(Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Floor Mark";
            go.transform.SetParent(transform);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.material = MaterialFor(null, new Color(.24f, .255f, .27f), 1001);
            renderer.sortingOrder = -999;
            Destroy(go.GetComponent<Collider>());
        }

        private Fighter CreateFighter(string displayName, bool isPlayer, Vector3 position,
            Color color, string resourcePrefix)
        {
            var root = NewObject(displayName);
            root.transform.position = position;
            float hitboxScale = isPlayer
                ? PlayerHitboxScale : EnemyHitboxScale;
            var hitbox = root.AddComponent<BoxCollider>();
            hitbox.size = new Vector3(
                BodyVisualWidth * hitboxScale,
                BodyVisualHeight * hitboxScale,
                BodyVisualWidth * hitboxScale);
            hitbox.center = Vector3.up * (hitbox.size.y * .5f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = resourcePrefix + " Panel";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, .88f, 0f);
            body.transform.localScale = new Vector3(
                BodyVisualWidth, BodyVisualHeight, .16f);
            body.GetComponent<Renderer>().material = MaterialFor(
                "Materials/" + resourcePrefix, color);
            Destroy(body.GetComponent<Collider>());

            Sprite[] sprites = Resources.LoadAll<Sprite>(
                "CountDown/Sprites/" + resourcePrefix + "Sheet");
            if (sprites.Length > 0)
            {
                body.SetActive(false);
                var spriteObject = NewObject(resourcePrefix + " Sprite");
                spriteObject.transform.SetParent(root.transform);
                spriteObject.transform.localPosition = Vector3.zero;
                var sr = spriteObject.AddComponent<SpriteRenderer>();
                sr.sprite = FindSprite(sprites, resourcePrefix + "_Idle_0");
                sr.color = Color.white;
                float height = Mathf.Max(.01f, sr.sprite.bounds.size.y);
                spriteObject.transform.localScale = Vector3.one * (1.72f / height);
                body = spriteObject;
            }

            var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "Target Stand";
            basePlate.transform.SetParent(root.transform);
            basePlate.transform.localPosition = new Vector3(0f, .06f, 0f);
            basePlate.transform.localScale = new Vector3(.72f, .06f, .72f);
            basePlate.GetComponent<Renderer>().material = MaterialFor(null, new Color(.08f, .085f, .09f));
            Destroy(basePlate.GetComponent<Collider>());

            var pivot = NewObject("Gun Pivot").transform;
            pivot.SetParent(root.transform);
            pivot.localPosition = new Vector3(isPlayer ? .52f : -.52f, 1.08f, 0f);

            var gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = "Replaceable Gun";
            gun.transform.SetParent(pivot);
            gun.transform.localPosition = new Vector3(0f, 0f, .38f);
            gun.transform.localScale = new Vector3(.16f, .16f, .78f);
            gun.GetComponent<Renderer>().material = MaterialFor("Materials/Gun", new Color(.11f, .115f, .12f));
            Destroy(gun.GetComponent<Collider>());

            var muzzle = NewObject("Muzzle").transform;
            muzzle.SetParent(pivot);
            muzzle.localPosition = new Vector3(0f, 0f, .82f);

            var laser = NewObject("Laser").AddComponent<LineRenderer>();
            laser.transform.SetParent(root.transform);
            laser.positionCount = 2;
            laser.useWorldSpace = true;
            laser.material = MaterialFor(null, color);
            laser.startWidth = .018f;
            laser.endWidth = .01f;
            laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var fighter = new Fighter
            {
                root = root.transform,
                body = body.transform,
                gunPivot = pivot,
                muzzle = muzzle,
                laser = laser,
                isPlayer = isPlayer,
                accent = color,
                health = MaxHealth,
                displayName = displayName
            };
            fighter.bodyRenderer = body.GetComponent<Renderer>();
            fighter.baseBodyColor = fighter.bodyRenderer.material.color;
            fighter.spriteRenderer = body.GetComponent<SpriteRenderer>();
            if (fighter.spriteRenderer != null)
            {
                fighter.spriteFrames = new Dictionary<string, Sprite>();
                for (int i = 0; i < sprites.Length; i++)
                    fighter.spriteFrames[sprites[i].name] = sprites[i];
                fighter.animationState = "Idle";
                fighter.animationStartedAt = Time.time;
            }
            CreateFuseVisuals(fighter);
            if (isPlayer)
                CreateMeleeRangeIndicator(fighter);
            else
            {
                CreateEnemyFireWarning(fighter);
                CreateAimLockMarker(fighter);
            }
            return fighter;
        }

        private void CreateAimLockMarker(Fighter fighter)
        {
            var marker = NewObject("Aim Lock Target");
            marker.transform.SetParent(fighter.root, true);
            fighter.aimLockMarker = marker.AddComponent<SpriteRenderer>();
            fighter.aimLockMarker.sprite =
                Resources.Load<Sprite>("CountDown/Sprites/AimLock") ??
                CreateAimLockPlaceholderSprite();
            fighter.aimLockMarker.color = new Color(.35f, 1f, .72f, .62f);
            fighter.aimLockMarker.sortingOrder = 32;
            fighter.aimLockMarker.enabled = false;
        }

        private static Sprite CreateAimLockPlaceholderSprite()
        {
            const int size = 64;
            const float radius = 25f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated Aim Lock Placeholder",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 ring = new Color32(255, 255, 255, 220);
            Vector2 center = Vector2.one * ((size - 1) * .5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 delta = new Vector2(x, y) - center;
                    float distance = delta.magnitude;
                    bool circle = Mathf.Abs(distance - radius) <= 1.4f;
                    bool horizontalTick = Mathf.Abs(delta.y) <= 1f &&
                        Mathf.Abs(delta.x) >= radius - 6f &&
                        Mathf.Abs(delta.x) <= radius + 4f;
                    bool verticalTick = Mathf.Abs(delta.x) <= 1f &&
                        Mathf.Abs(delta.y) >= radius - 6f &&
                        Mathf.Abs(delta.y) <= radius + 4f;
                    pixels[y * size + x] =
                        circle || horizontalTick || verticalTick ? ring : clear;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(.5f, .5f), 64f);
        }

        private void CreateFuseVisuals(Fighter fighter)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("CountDown/Sprites/FuseSheet");
            if (sprites.Length == 0) return;

            Sprite segmentSprite = FindSprite(sprites, "Fuse_Segment_0");
            fighter.fuseSegments = new SpriteRenderer[FuseSegmentCount];
            fighter.fusePositions = new Vector3[FuseSegmentCount];
            for (int i = 0; i < FuseSegmentCount; i++)
            {
                var segment = NewObject("Fuse Segment " + (i + 1));
                segment.transform.SetParent(fighter.root, true);
                var renderer = segment.AddComponent<SpriteRenderer>();
                renderer.sprite = segmentSprite;
                renderer.color = Color.Lerp(fighter.accent, new Color(.42f, .25f, .12f), .62f);
                renderer.sortingOrder = 20;
                segment.transform.localScale = Vector3.one * FuseSegmentScale;
                fighter.fuseSegments[i] = renderer;
            }

            var flame = NewObject("Fuse Flame");
            flame.transform.SetParent(fighter.root, true);
            fighter.fuseFlame = flame.AddComponent<SpriteRenderer>();
            fighter.fuseFlameFrames = new[]
            {
                FindSprite(sprites, "Fuse_Flame_0"),
                FindSprite(sprites, "Fuse_Flame_1"),
                FindSprite(sprites, "Fuse_Flame_2")
            };
            fighter.fuseFlame.sprite = fighter.fuseFlameFrames[0];
            fighter.fuseFlame.sortingOrder = 22;
            flame.transform.localScale = Vector3.one * FuseFlameScale;

            var alert = NewObject("Fuse Alert");
            alert.transform.SetParent(fighter.root, true);
            fighter.fuseAlert = alert.AddComponent<SpriteRenderer>();
            fighter.fuseAlertFrames = new[]
            {
                FindSprite(sprites, "Fuse_Alert_0"),
                FindSprite(sprites, "Fuse_Alert_1")
            };
            fighter.fuseAlert.sprite = fighter.fuseAlertFrames[0];
            fighter.fuseAlert.sortingOrder = 23;
            alert.transform.localScale = Vector3.one * .28f;
            fighter.fuseAlert.enabled = false;
        }

        private static Sprite FindSprite(Sprite[] sprites, string name)
        {
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i].name == name)
                    return sprites[i];
            return sprites[0];
        }

        private void CreateMeleeRangeIndicator(Fighter fighter)
        {
            fighter.meleeRangeRing = CreateGroundRing("Melee Range",
                fighter.root, new Color(.2f, .8f, 1f, .28f), .035f);
            fighter.meleeCooldownRing = CreateGroundRing("Melee Cooldown",
                fighter.root, new Color(.25f, 1f, .58f, .9f), .075f);
            SetRingPositions(fighter.meleeRangeRing, MeleeRange, 64, true);
            SetRingPositions(fighter.meleeCooldownRing, MeleeRange, 2, false);
        }

        private LineRenderer CreateGroundRing(string name, Transform parent,
            Color color, float width)
        {
            var ring = NewObject(name).AddComponent<LineRenderer>();
            ring.transform.SetParent(parent);
            ring.transform.localPosition = new Vector3(0f, .025f, 0f);
            ring.useWorldSpace = false;
            ring.material = MaterialFor(null, color, 3000);
            ring.startColor = ring.endColor = color;
            ring.startWidth = ring.endWidth = width;
            ring.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            return ring;
        }

        private static void SetRingPositions(LineRenderer ring, float radius,
            int segmentCount, bool closed, float completion = 1f)
        {
            segmentCount = Mathf.Max(2, segmentCount);
            ring.loop = closed;
            ring.positionCount = segmentCount + (closed ? 0 : 1);
            float denominator = closed ? segmentCount : Mathf.Max(1, segmentCount);
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i / denominator * Mathf.PI * 2f * completion;
                ring.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * radius, 0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        private void CreateEnemyFireWarning(Fighter fighter)
        {
            var warning = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warning.name = "Fire Warning Placeholder";
            warning.transform.SetParent(fighter.muzzle);
            warning.transform.localPosition = new Vector3(0f, .32f, .18f);
            warning.transform.localRotation = Quaternion.identity;
            warning.transform.localScale = Vector3.one * .28f;
            warning.GetComponent<Renderer>().material = MaterialFor(
                null, new Color(1f, .3f, .08f), 3100);
            Destroy(warning.GetComponent<Collider>());
            warning.SetActive(false);
            fighter.fireWarning = warning;
        }

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            return go;
        }

        private Material MaterialFor(string resourcePath, Color fallback, int renderQueue = 2000)
        {
            var loaded = string.IsNullOrEmpty(resourcePath)
                ? null : Resources.Load<Material>("CountDown/" + resourcePath);
            if (loaded != null)
            {
                var instance = new Material(loaded);
                instance.renderQueue = renderQueue;
                return instance;
            }
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            material.color = fallback;
            material.renderQueue = renderQueue;
            return material;
        }

        private void Update()
        {
            UpdateInputMode();
            UpdateTouchControls();

            if (awaitingStart)
            {
                if (StartInputPressed())
                    awaitingStart = false;
                else
                {
                    UpdateCamera(false);
                    return;
                }
            }

            if (finished)
            {
                UpdateAimFeedback(Mathf.Min(Time.deltaTime, .05f));
                if (Input.GetKeyDown(KeyCode.R))
                    RestartGame();
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                    RestartGame();
                if (Input.GetKeyDown(KeyCode.JoystickButton0))
                    RestartGame();
                if (Input.GetKeyDown(KeyCode.Escape))
                    Quit();
                UpdateCamera(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Quit();

            float dt = Mathf.Min(Time.deltaTime, .05f);
            UpdatePlayer(dt);
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead)
                    UpdateEnemy(enemies[i], dt);
            ResolveAllFighterOverlaps();
            UpdateWeapon(player, dt);
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead)
                    UpdateWeapon(enemies[i], dt);
            UpdatePresentation(player, dt);
            for (int i = 0; i < enemies.Count; i++)
                UpdatePresentation(enemies[i], dt);
            UpdateAimFeedback(dt);
            UpdateCamera(false);
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.8f);
        }

        private static bool StartInputPressed()
        {
            if (Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began)
                return true;
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.JoystickButton0);
        }

        private void UpdatePlayer(float dt)
        {
            if (Time.time < player.stunnedUntil)
            {
                player.aiming = false;
                ClearPlayerAimLock();
                return;
            }

            Vector2 gamepadMove = ReadGamepadStick(
                "Gamepad Left X", "Gamepad Left Y");
            Vector2 gamepadAim = ReadGamepadStick(
                "Gamepad Right X", "Gamepad Right Y");
            bool gamepadAiming = inputMode == InputMode.Gamepad &&
                Input.GetKey(KeyCode.JoystickButton7);

            bool wasAiming = player.aiming;
            bool wantsToAim = inputMode == InputMode.Touch ? touchAiming :
                inputMode == InputMode.Gamepad ? gamepadAiming :
                Input.GetMouseButton(1);
            player.aiming = wantsToAim;
            if (wasAiming && !wantsToAim)
            {
                emergencyDodgeUntil = Time.time + EmergencyDodgeDuration;
                ClearPlayerAimLock();
            }

            Vector3 move = inputMode == InputMode.Gamepad
                ? new Vector3(gamepadMove.x, 0f, gamepadMove.y)
                : new Vector3(Input.GetAxisRaw("Horizontal"), 0f,
                    Input.GetAxisRaw("Vertical"));
            if (inputMode == InputMode.Touch)
                move = new Vector3(touchMove.x, 0f, touchMove.y);
            move = Vector3.ClampMagnitude(move, 1f);
            float speed = player.aiming ? AimMoveSpeed : NormalMoveSpeed;
            if (!player.aiming && Time.time < emergencyDodgeUntil)
                speed *= EmergencyDodgeMultiplier;
            MoveFighter(player, move * speed * dt);

            if (inputMode == InputMode.Mouse)
                mouseAimPosition = Input.mousePosition;

            Vector3 direction;
            if (inputMode == InputMode.Touch)
            {
                if (touchAim.sqrMagnitude > .01f)
                    aimDirection = touchAim.normalized;
                direction = new Vector3(aimDirection.x, 0f, aimDirection.y);
            }
            else if (inputMode == InputMode.Gamepad)
            {
                if (gamepadAim.sqrMagnitude > GamepadDeadzone * GamepadDeadzone)
                    aimDirection = gamepadAim.normalized;
                direction = new Vector3(aimDirection.x, 0f, aimDirection.y);
            }
            else
            {
                Vector3 mouseTarget = PointerArenaPoint(Input.mousePosition);
                direction = mouseTarget - player.gunPivot.position;
                direction.y = 0f;
            }
            if (direction.sqrMagnitude < .01f)
                direction = player.gunPivot.forward;
            direction = ApplyAimAssist(player.gunPivot.position, direction);
            player.gunPivot.rotation = Quaternion.LookRotation(direction, Vector3.up);
            TryAutoMelee();
        }

        private void UpdateInputMode()
        {
            if (Input.touchCount > 0)
            {
                inputMode = InputMode.Touch;
                return;
            }

            Vector2 leftStick = ReadGamepadStick(
                "Gamepad Left X", "Gamepad Left Y");
            Vector2 rightStick = ReadGamepadStick(
                "Gamepad Right X", "Gamepad Right Y");
            bool gamepadActive = leftStick.sqrMagnitude >
                    GamepadDeadzone * GamepadDeadzone ||
                rightStick.sqrMagnitude > GamepadDeadzone * GamepadDeadzone ||
                Input.GetKey(KeyCode.JoystickButton7);
            if (gamepadActive)
            {
                inputMode = InputMode.Gamepad;
                return;
            }

            bool mouseOrKeyboardActive =
                Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Mathf.Abs(Input.GetAxisRaw("Mouse X")) > .01f ||
                Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > .01f ||
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > .01f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > .01f;
            if (mouseOrKeyboardActive)
                inputMode = InputMode.Mouse;
        }

        private static Vector2 ReadGamepadStick(string horizontal, string vertical)
        {
            Vector2 value = new Vector2(
                Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical));
            return value.sqrMagnitude < GamepadDeadzone * GamepadDeadzone
                ? Vector2.zero : Vector2.ClampMagnitude(value, 1f);
        }

        private void RestartGame()
        {
            StopAllCoroutines();
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            player = null;
            enemies.Clear();
            projectileThreats.Clear();
            gameCamera = null;
            audioSource = null;
            finished = false;
            stageTransitioning = false;
            awaitingStart = true;
            stage = 1;
            shake = 0f;
            aimFeedback = 0f;
            emergencyDodgeUntil = 0f;
            moveTouchId = aimTouchId = -1;
            touchMove = Vector2.zero;
            touchAim = Vector2.zero;
            touchAiming = false;
            inputMode = InputMode.Mouse;
            aimDirection = Vector2.up;
            mouseAimPosition = new Vector2(Screen.width * .5f, Screen.height * .5f);
            BuildWorld();
        }

        private void UpdateTouchControls()
        {
            touchMove = Vector2.zero;
            touchAim = Vector2.zero;
            touchAiming = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                bool ended = touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled;

                if (touch.phase == TouchPhase.Began)
                {
                    if (touch.position.x < Screen.width * .5f && moveTouchId < 0)
                    {
                        moveTouchId = touch.fingerId;
                        moveTouchOrigin = moveTouchPosition = touch.position;
                    }
                    else if (touch.position.x >= Screen.width * .5f && aimTouchId < 0)
                    {
                        aimTouchId = touch.fingerId;
                        aimTouchOrigin = aimTouchPosition = touch.position;
                    }
                }

                if (touch.fingerId == moveTouchId)
                {
                    if (ended) moveTouchId = -1;
                    else
                    {
                        moveTouchPosition = touch.position;
                        touchMove = Vector2.ClampMagnitude(
                            (moveTouchPosition - moveTouchOrigin) / TouchStickRadius, 1f);
                    }
                }
                if (touch.fingerId == aimTouchId)
                {
                    if (ended) aimTouchId = -1;
                    else
                    {
                        aimTouchPosition = touch.position;
                        touchAim = Vector2.ClampMagnitude(
                            (aimTouchPosition - aimTouchOrigin) / TouchStickRadius, 1f);
                        touchAiming = true;
                    }
                }
            }
        }

        private void UpdateEnemy(Fighter enemy, float dt)
        {
            if (Time.time < enemy.stunnedUntil)
            {
                enemy.aiming = false;
                return;
            }

            Vector3 toPlayer = player.root.position - enemy.root.position;
            float distance = toPlayer.magnitude;
            if (Time.time >= enemy.nextDecisionAt)
            {
                enemy.evadeDirection = Random.value < .5f ? -1f : 1f;
                enemy.nextDecisionAt = Time.time + Random.Range(.65f, 1.15f);
            }

            Vector3 orbit = Vector3.Cross(Vector3.up, toPlayer.normalized) *
                enemy.evadeDirection;
            Vector3 projectileEscape;
            bool projectileClose = TryGetProjectileEscape(
                enemy, out projectileEscape);
            Vector3 tacticalMove = projectileClose
                ? projectileEscape * 1.55f + orbit * .25f
                : orbit * .38f;

            if (distance < 2.5f)
            {
                if (enemy.closeDetectedAt <= 0f) enemy.closeDetectedAt = Time.time;
                if (Time.time - enemy.closeDetectedAt >= .3f)
                    tacticalMove += -toPlayer.normalized * 1.35f;
            }
            else
            {
                enemy.closeDetectedAt = 0f;
                if (distance > 6.2f)
                    tacticalMove += toPlayer.normalized * .42f;
                else if (distance < 3.4f)
                    tacticalMove += -toPlayer.normalized * .65f;
            }

            bool firingSoon = enemy.count == 1 && enemy.aiming &&
                (enemy.hasTarget || Time.time <= enemy.aimGraceUntil);
            if (!firingSoon)
                MoveEnemy(enemy, tacticalMove, dt);

            Vector3 flatTarget = player.root.position + Vector3.up * .85f - enemy.gunPivot.position;
            flatTarget.y = 0f;
            if (flatTarget.sqrMagnitude > .01f)
            {
                Quaternion desired = Quaternion.LookRotation(flatTarget.normalized, Vector3.up);
                enemy.gunPivot.rotation = Quaternion.RotateTowards(
                    enemy.gunPivot.rotation, desired, EnemyTurnSpeed * dt);
            }
            enemy.aiming = true;
            UpdateEnemyFireWarning(enemy);
        }

        private bool TryGetProjectileEscape(Fighter enemy, out Vector3 escape)
        {
            escape = Vector3.zero;
            float bestDistance = EnemyProjectileDodgeDistance;
            for (int i = 0; i < projectileThreats.Count; i++)
            {
                ProjectileThreat threat = projectileThreats[i];
                if (!threat.fromPlayer) continue;
                Vector3 toEnemy = enemy.root.position - threat.position;
                toEnemy.y = 0f;
                float distance = toEnemy.magnitude;
                if (distance >= bestDistance ||
                    Vector3.Dot(toEnemy, threat.direction) <= 0f)
                    continue;
                float pathDistance = Vector3.Cross(
                    threat.direction, toEnemy).magnitude;
                if (pathDistance > EnemyLineAvoidanceRadius) continue;
                Vector3 lateral = toEnemy -
                    threat.direction * Vector3.Dot(toEnemy, threat.direction);
                escape = lateral.sqrMagnitude > .01f
                    ? lateral.normalized
                    : Vector3.Cross(Vector3.up, threat.direction).normalized *
                        enemy.evadeDirection;
                bestDistance = distance;
            }
            return escape.sqrMagnitude > .01f;
        }

        private void UpdateEnemyFireWarning(Fighter enemy)
        {
            if (enemy.fireWarning == null) return;
            bool countingFinalSecond = enemy.count == 1 && enemy.aiming &&
                (enemy.hasTarget || Time.time <= enemy.aimGraceUntil);
            bool visible = countingFinalSecond &&
                enemy.countTimer >= 1f - EnemyFireWarningDuration;
            enemy.fireWarning.SetActive(visible);
            if (visible)
            {
                enemy.fireWarning.transform.localScale = Vector3.one *
                    (.24f + Mathf.PingPong(Time.time * 4f, .1f));
                enemy.fireWarning.transform.LookAt(gameCamera.transform);
            }
        }

        private void MoveEnemy(Fighter enemy, Vector3 direction, float dt)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > .001f)
                MoveFighter(enemy, direction.normalized *
                    EnemyMoveSpeedForStage() * dt);
        }

        private float EnemyMoveSpeedForStage()
        {
            return Mathf.Min(EnemyMaxMoveSpeed,
                EnemyBaseMoveSpeed + (stage - 1) * EnemyMoveSpeedPerStage);
        }

        private Vector3 ApplyAimAssist(Vector3 origin, Vector3 rawDirection)
        {
            rawDirection.y = 0f;
            if (rawDirection.sqrMagnitude < .01f)
                return Vector3.forward;

            Vector3 normalized = rawDirection.normalized;
            float assistDegrees = AimAssistDegrees;

            Fighter best = null;
            float bestAngle = assistDegrees + .001f;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < enemies.Count; i++)
            {
                Fighter candidate = enemies[i];
                if (candidate.dead) continue;
                Vector3 toCandidate = candidate.root.position +
                    Vector3.up * .85f - origin;
                toCandidate.y = 0f;
                float distance = toCandidate.magnitude;
                if (distance < .01f) continue;
                float angle = Vector3.Angle(normalized, toCandidate / distance);
                if (angle > assistDegrees) continue;

                bool moreCentered = angle < bestAngle - .5f;
                bool overlapsCurrent = Mathf.Abs(angle - bestAngle) <= .5f;
                if (best == null || moreCentered ||
                    (overlapsCurrent && distance < bestDistance))
                {
                    best = candidate;
                    bestAngle = angle;
                    bestDistance = distance;
                }
            }

            if (player == null || !player.aiming)
                return DirectionToAimTarget(origin, best, normalized);

            if (player.aimLockTarget == null || player.aimLockTarget.dead)
                player.aimLockTarget = best;
            else
                player.aimLockTarget = SelectAimLockTarget(
                    origin, normalized, player.aimLockTarget);

            return DirectionToAimTarget(
                origin, player.aimLockTarget, normalized);
        }

        private Fighter SelectAimLockTarget(Vector3 origin,
            Vector3 rawDirection, Fighter current)
        {
            Vector3 currentPoint = GetFighterAimPoint(current);
            currentPoint.y = origin.y;
            float lockDistance = Vector3.Distance(origin, currentPoint);
            Vector3 virtualAimPoint = origin + rawDirection * lockDistance;
            Fighter best = current;
            float bestDistance = (currentPoint - virtualAimPoint).sqrMagnitude;

            for (int i = 0; i < enemies.Count; i++)
            {
                Fighter candidate = enemies[i];
                if (candidate.dead || candidate == current) continue;
                Vector3 candidatePoint = GetFighterAimPoint(candidate);
                candidatePoint.y = origin.y;
                float distance =
                    (candidatePoint - virtualAimPoint).sqrMagnitude;
                if (distance + .001f >= bestDistance) continue;
                best = candidate;
                bestDistance = distance;
            }

            return best;
        }

        private static Vector3 DirectionToAimTarget(Vector3 origin,
            Fighter target, Vector3 fallback)
        {
            if (target == null) return fallback;
            Vector3 assisted = GetFighterAimPoint(target) - origin;
            assisted.y = 0f;
            return assisted.sqrMagnitude > .01f
                ? assisted.normalized : fallback;
        }

        private void ClearPlayerAimLock()
        {
            if (player != null)
                player.aimLockTarget = null;
        }

        private void MoveFighter(Fighter fighter, Vector3 delta)
        {
            Vector3 candidate = ClampToArena(fighter.root.position + delta);
            SeparateFromFighter(ref candidate, fighter, player);
            for (int i = 0; i < enemies.Count; i++)
                SeparateFromFighter(ref candidate, fighter, enemies[i]);
            fighter.root.position = ClampToArena(candidate);
        }

        private static void SeparateFromFighter(
            ref Vector3 candidate, Fighter moving, Fighter other)
        {
            if (other == null || other == moving || other.dead) return;
            Vector3 offset = candidate - other.root.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= FighterSeparation) return;
            Vector3 direction = distance > .001f
                ? offset / distance
                : ((moving.root.GetInstanceID() & 1) == 0
                    ? Vector3.right : Vector3.left);
            candidate += direction * (FighterSeparation - distance);
        }

        private void ResolveAllFighterOverlaps()
        {
            ResolvePair(player, null);
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].dead) continue;
                ResolvePair(enemies[i], player);
                for (int j = i + 1; j < enemies.Count; j++)
                    if (!enemies[j].dead)
                        ResolvePair(enemies[i], enemies[j]);
            }
        }

        private void ResolvePair(Fighter first, Fighter second)
        {
            if (first == null || second == null || first.dead || second.dead) return;
            Vector3 offset = first.root.position - second.root.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= FighterSeparation) return;
            Vector3 direction = distance > .001f
                ? offset / distance : Vector3.right;
            float correction = (FighterSeparation - distance) * .5f;
            first.root.position = ClampToArena(first.root.position + direction * correction);
            second.root.position = ClampToArena(second.root.position - direction * correction);
        }

        private void UpdateWeapon(Fighter fighter, float dt)
        {
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(fighter.muzzle.position,
                fighter.muzzle.forward, out hit, 30f);
            Vector3 end = hitSomething ? hit.point :
                fighter.muzzle.position + fighter.muzzle.forward * 30f;
            if (fighter.isPlayer)
                fighter.opponent = hitSomething
                    ? FindAliveEnemy(hit.collider.transform) : null;
            fighter.hasTarget = hitSomething && fighter.opponent != null &&
                !fighter.opponent.dead &&
                hit.collider.transform.IsChildOf(fighter.opponent.root);
            bool acquiredTarget = fighter.aiming && fighter.hasTarget;
            if (acquiredTarget && !fighter.wasActivelyTargeting)
                fighter.countTimer += AimAcquireBonus;
            if (acquiredTarget)
                fighter.aimGraceUntil = Time.time + AimGraceDuration;
            fighter.wasActivelyTargeting = acquiredTarget;

            fighter.laser.SetPosition(0, fighter.muzzle.position);
            fighter.laser.SetPosition(1, end);
            float width = fighter.hasTarget ? .055f : .018f;
            if (fighter.count == 1 && fighter.hasTarget)
                width *= Mathf.Lerp(.35f, 1.8f, Mathf.PingPong(Time.time * 8f, 1f));
            fighter.laser.startWidth = width;
            fighter.laser.endWidth = width * .55f;
            Color beamColor = fighter.hasTarget ? fighter.accent : fighter.accent * .55f;
            fighter.laser.startColor = beamColor;
            fighter.laser.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, .15f);

            if (fighter.firePending)
            {
                if (Time.time >= fighter.pendingFireAt)
                    Fire(fighter);
                return;
            }

            bool canCount = fighter.aiming && Time.time >= fighter.stunnedUntil;
            if (canCount)
            {
                fighter.countTimer += dt;
                while (fighter.countTimer >= 1f)
                {
                    fighter.countTimer -= 1f;
                    fighter.count = Mathf.Max(0, fighter.count - 1);
                    Play(fighter.count == 1 ? "Audio/Warning" : "Audio/Click",
                        fighter.count == 1 ? 660f : 310f, .08f, .18f);
                    fighter.pulse = 1f;
                    if (fighter.count == 0)
                    {
                        fighter.countTimer = 0f;
                        fighter.firePending = true;
                        fighter.pendingFireAt = Time.time + PreFirePause;
                        fighter.pendingFireTarget =
                            fighter.opponent != null && !fighter.opponent.dead
                                ? fighter.opponent : null;
                        fighter.pendingFireDirection =
                            fighter.muzzle.forward.normalized;
                        break;
                    }
                }
            }
        }

        private void Fire(Fighter fighter)
        {
            Vector3 direction;
            if (fighter.pendingFireTarget != null &&
                !fighter.pendingFireTarget.dead)
            {
                direction = (GetFighterAimPoint(fighter.pendingFireTarget) -
                    fighter.muzzle.position).normalized;
            }
            else
            {
                direction = fighter.pendingFireDirection.sqrMagnitude > .01f
                    ? fighter.pendingFireDirection
                    : fighter.muzzle.forward.normalized;
            }
            Play("Audio/Gunshot", 115f, .13f, .75f);
            StartCoroutine(MuzzleFlash(fighter));
            StartCoroutine(FireProjectile(fighter,
                fighter.muzzle.position, direction));
            if (fighter.isPlayer)
                fighter.ResetCount();
            else
                ResetEnemyCount(fighter);
        }

        private void ResetEnemyCount(Fighter enemy)
        {
            float normalizedTotal = 0f;
            int otherCount = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                Fighter other = enemies[i];
                if (other == enemy || other.dead) continue;
                normalizedTotal += Mathf.InverseLerp(
                    other.countMin, other.countMax, other.count);
                otherCount++;
            }

            if (otherCount == 0)
            {
                enemy.ResetCount();
                return;
            }

            float average = normalizedTotal / otherCount;
            float exponent = Mathf.Lerp(.55f, 1.8f, average);
            float weighted = Mathf.Pow(Random.value, exponent);
            enemy.count = Mathf.RoundToInt(Mathf.Lerp(
                enemy.countMin, enemy.countMax, weighted));
            enemy.startingCount = enemy.count;
            enemy.countTimer = 0f;
            enemy.pulse = 1f;
            enemy.firePending = false;
            enemy.pendingFireTarget = null;
            enemy.pendingFireDirection = Vector3.zero;
        }

        private static Vector3 GetFighterAimPoint(Fighter fighter)
        {
            Collider hitbox = fighter.root.GetComponent<Collider>();
            return hitbox != null
                ? hitbox.bounds.center
                : fighter.root.position + Vector3.up * (BodyHeight * .5f);
        }

        private IEnumerator FireProjectile(Fighter source, Vector3 position,
            Vector3 direction)
        {
            var threat = new ProjectileThreat
            {
                position = position,
                direction = direction,
                fromPlayer = source.isPlayer
            };
            projectileThreats.Add(threat);
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = source.isPlayer ? "Player Projectile" : "Enemy Projectile";
            projectile.transform.SetParent(transform);
            projectile.transform.position = position;
            projectile.transform.localScale = Vector3.one * (ProjectileRadius * 2f);
            projectile.GetComponent<Renderer>().material = MaterialFor(
                null, Color.Lerp(source.accent, Color.white, .35f), 3100);
            Destroy(projectile.GetComponent<Collider>());

            var trailObject = NewObject("Projectile Trail");
            trailObject.transform.position = position;
            var trail = trailObject.AddComponent<TrailRenderer>();
            trail.time = .22f;
            trail.startWidth = ProjectileRadius * 1.35f;
            trail.endWidth = 0f;
            trail.material = MaterialFor(null, source.accent, 3099);
            trail.startColor = source.accent;
            trail.endColor = new Color(
                source.accent.r, source.accent.g, source.accent.b, 0f);
            trail.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            float expiresAt = Time.time + ProjectileLifetime;
            while (!finished && !stageTransitioning && Time.time < expiresAt)
            {
                float speed = source.isPlayer
                    ? ProjectileSpeed * PlayerProjectileSpeedMultiplier
                    : ProjectileSpeed;
                float distance = speed * Mathf.Min(Time.deltaTime, .05f);
                RaycastHit[] hits = Physics.SphereCastAll(position,
                    ProjectileRadius, direction, distance,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                bool connected = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    Fighter hitTarget = source.isPlayer
                        ? FindAliveEnemy(hits[i].collider.transform)
                        : hits[i].collider.transform.IsChildOf(player.root)
                            ? player : null;
                    if (hitTarget == null || hitTarget.dead) continue;
                    connected = true;
                    source.opponent = hitTarget;
                    break;
                }

                position += direction * distance;
                threat.position = position;
                projectile.transform.position = position;
                trailObject.transform.position = position;
                if (connected)
                {
                    Damage(source.opponent, source.isPlayer
                        ? PlayerProjectileDamage : EnemyProjectileDamage, source);
                    break;
                }

                if (Mathf.Abs(position.x) > ArenaWidth * .5f + 2f ||
                    Mathf.Abs(position.z) > ArenaDepth * .5f + 2f)
                    break;
                yield return null;
            }

            Destroy(projectile);
            Destroy(trailObject, trail.time);
            projectileThreats.Remove(threat);
        }

        private Fighter FindAliveEnemy(Transform hitTransform)
        {
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead &&
                    hitTransform.IsChildOf(enemies[i].root))
                    return enemies[i];
            return null;
        }

        private IEnumerator MuzzleFlash(Fighter fighter)
        {
            var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "Muzzle Flash";
            flash.transform.SetParent(fighter.muzzle);
            flash.transform.localPosition = Vector3.forward * .12f;
            flash.transform.localScale = Vector3.one * .32f;
            flash.GetComponent<Renderer>().material = MaterialFor(
                "Materials/MuzzleFlash", new Color(1f, .72f, .16f));
            Destroy(flash.GetComponent<Collider>());
            yield return new WaitForSeconds(.07f);
            Destroy(flash);
        }

        private IEnumerator Melee()
        {
            meleeReadyAt = Time.time + MeleeCooldown;
            var targets = new List<Fighter>();
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead && IsInMeleeRange(enemies[i]))
                    targets.Add(enemies[i]);
            player.stunnedUntil = Time.time + PlayerMeleeRecovery;
            player.meleeAttackUntil = Time.time + .26f;
            player.meleeRecoverUntil = player.stunnedUntil;
            Vector3 original = player.gunPivot.localEulerAngles;
            float elapsed = 0f;
            while (elapsed < .14f)
            {
                elapsed += Time.deltaTime;
                player.gunPivot.localRotation = Quaternion.Euler(
                    Mathf.Lerp(0f, -32f, elapsed / .14f), 0f,
                    Mathf.Lerp(0f, -65f, elapsed / .14f));
                yield return null;
            }

            int hitCount = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                Fighter target = targets[i];
                if (target.dead) continue;
                target.stunnedUntil = Time.time + .7f;
                target.aiming = false;
                ResetEnemyCount(target);
                Damage(target, 1, player);
                hitCount++;
            }

            if (hitCount > 0)
            {
                Play("Audio/MeleeHit", 92f, .1f, .7f);
            }
            else
            {
                Play("Audio/MeleeMiss", 180f, .08f, .24f);
            }

            yield return new WaitForSeconds(.12f);
            player.gunPivot.localEulerAngles = original;
        }

        private void TryAutoMelee()
        {
            if (Time.time >= meleeReadyAt && HasEnemyInMeleeRange())
                StartCoroutine(Melee());
        }

        private bool HasEnemyInMeleeRange()
        {
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead &&
                    IsWithinMeleeDistance(enemies[i], MeleeRange))
                    return true;
            return false;
        }

        private bool IsInMeleeRange(Fighter target)
        {
            return IsWithinMeleeDistance(target, MeleeHitRange);
        }

        private bool IsWithinMeleeDistance(Fighter target, float range)
        {
            Vector3 offset = target.root.position - player.root.position;
            offset.y = 0f;
            return offset.magnitude - BodyRadius <= range;
        }

        private Fighter ClosestAliveEnemy()
        {
            Fighter closest = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].dead) continue;
                float distance = (enemies[i].root.position -
                    player.root.position).sqrMagnitude;
                if (distance >= closestDistance) continue;
                closestDistance = distance;
                closest = enemies[i];
            }
            return closest;
        }

        private void Damage(Fighter target, int amount, Fighter source)
        {
            if (finished) return;
            target.health = Mathf.Max(0, target.health - amount);
            target.hitFlash = 1f;
            Vector3 knockback = target.root.position - source.root.position;
            knockback.y = 0f;
            if (knockback.sqrMagnitude < .01f)
                knockback = -source.gunPivot.forward;
            MoveFighter(target, knockback.normalized * HitKnockbackDistance);
            shake = .24f;
            Play("Audio/Impact", 72f, .09f, .6f);
            if (target.health <= 0)
            {
                target.dead = true;
                target.aiming = false;
                target.laser.enabled = false;
                Collider targetCollider = target.root.GetComponent<Collider>();
                if (targetCollider != null) targetCollider.enabled = false;
                if (target.isPlayer)
                {
                    finished = true;
                    for (int i = 0; i < enemies.Count; i++)
                        enemies[i].aiming = false;
                    Play("Audio/Lose", 110f, .45f, .55f);
                }
                else if (!stageTransitioning && AllEnemiesDefeated())
                {
                    stageTransitioning = true;
                    player.aiming = false;
                    StartCoroutine(AdvanceStage());
                }
            }
        }

        private bool AllEnemiesDefeated()
        {
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead)
                    return false;
            return enemies.Count > 0;
        }

        private IEnumerator AdvanceStage()
        {
            Play("Audio/Win", 440f, .45f, .55f);
            yield return new WaitForSeconds(1.15f);
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i].root != null)
                    Destroy(enemies[i].root.gameObject);
            enemies.Clear();
            stage++;
            player.health = MaxHealth;
            player.dead = false;
            player.ResetCount();
            player.root.position = new Vector3(
                0f, FighterGroundHeight, -ArenaDepth * .3f);
            StartStage();
        }

        private void UpdatePresentation(Fighter fighter, float dt)
        {
            fighter.pulse = Mathf.MoveTowards(fighter.pulse, 0f, dt * 3f);
            fighter.hitFlash = Mathf.MoveTowards(fighter.hitFlash, 0f, dt * 4f);
            Vector3 lean = fighter.lastPosition == Vector3.zero ? Vector3.zero :
                (fighter.root.position - fighter.lastPosition) / Mathf.Max(dt, .001f);
            fighter.lastPosition = fighter.root.position;

            Quaternion targetRotation = fighter.dead && fighter.spriteRenderer == null
                ? Quaternion.Euler(78f, 0f, fighter.isPlayer ? -8f : 8f)
                : Quaternion.Euler(0f, 0f, Mathf.Clamp(-lean.x * 2.2f, -8f, 8f));
            fighter.body.localRotation = Quaternion.Slerp(
                fighter.body.localRotation, targetRotation, dt * (fighter.dead ? 4f : 9f));
            if (fighter.bodyRenderer != null)
            {
                float flash = fighter.hitFlash > 0f &&
                    Mathf.PingPong(fighter.hitFlash * 12f, 1f) > .35f ? 1f : 0f;
                Color presentationColor = fighter.baseBodyColor;
                if (fighter.isPlayer && Time.time < fighter.stunnedUntil)
                {
                    presentationColor = new Color(
                        presentationColor.r * PlayerMovementLockedBrightness,
                        presentationColor.g * PlayerMovementLockedBrightness,
                        presentationColor.b * PlayerMovementLockedBrightness,
                        presentationColor.a);
                }
                fighter.bodyRenderer.material.color = Color.Lerp(
                    presentationColor, Color.white, flash);
                fighter.bodyRenderer.enabled = !(fighter.hitFlash > .35f &&
                    fighter.hitFlash < .75f);
            }
            if (!fighter.dead)
                fighter.body.localPosition = new Vector3(0f,
                    (fighter.spriteRenderer == null ? .88f : 0f) +
                    Mathf.Sin(Time.time * 7f) * Mathf.Min(lean.magnitude * .006f, .035f), 0f);
            UpdateSpriteAnimation(fighter, lean.magnitude);
            UpdateFusePresentation(fighter, dt);
            UpdateAimLockMarker(fighter);
            if (fighter.isPlayer)
                UpdateMeleeRangeIndicator(fighter);
        }

        private void UpdateAimLockMarker(Fighter fighter)
        {
            if (fighter.aimLockMarker == null) return;
            bool visible = player != null && player.aiming &&
                player.aimLockTarget == fighter && !fighter.dead;
            fighter.aimLockMarker.enabled = visible;
            if (!visible) return;
            fighter.aimLockMarker.transform.position =
                GetFighterAimPoint(fighter);
            fighter.aimLockMarker.transform.rotation =
                gameCamera.transform.rotation;
            float pulse = 1.08f + Mathf.PingPong(Time.time * .8f, .08f);
            fighter.aimLockMarker.transform.localScale =
                Vector3.one * pulse;
        }

        private void UpdateFusePresentation(Fighter fighter, float dt)
        {
            if (fighter.fuseSegments == null) return;
            bool hidden = fighter.dead;
            float remainingLinks = fighter.startingCount <= 0 || fighter.firePending
                ? 0f
                : Mathf.Clamp(fighter.count - fighter.countTimer,
                    0f, FuseSegmentCount);
            float visibleLinks = hidden ? 0f : remainingLinks;
            int visibleCount = Mathf.Clamp(Mathf.CeilToInt(visibleLinks),
                0, FuseSegmentCount);

            Vector3 cameraRight = gameCamera.transform.right;
            Vector3 cameraUp = gameCamera.transform.up;
            Vector3 defaultDirection =
                (-cameraRight * .96f - cameraUp * .28f).normalized;
            Vector3 anchor = fighter.gunPivot.position -
                fighter.gunPivot.forward * .08f - cameraUp * .02f;
            Vector3 previous = anchor;

            for (int i = 0; i < FuseSegmentCount; i++)
            {
                SpriteRenderer segment = fighter.fuseSegments[i];
                segment.enabled = i < visibleCount;
                if (!segment.enabled) continue;
                float linkAmount = Mathf.Clamp01(visibleLinks - i);

                Vector3 current = fighter.fusePositions[i];
                if (current == Vector3.zero)
                    current = previous + defaultDirection *
                        (FuseLinkLength * linkAmount);
                Vector3 currentDirection = current - previous;
                currentDirection = currentDirection.sqrMagnitude > .0001f
                    ? currentDirection.normalized : defaultDirection;
                Vector3 relaxedDirection = Vector3.Slerp(
                    currentDirection, defaultDirection, Mathf.Clamp01(dt * 3.2f));
                Vector3 target = previous + relaxedDirection *
                    (FuseLinkLength * linkAmount);
                current = Vector3.Lerp(current, target, Mathf.Clamp01(dt * 9f));
                fighter.fusePositions[i] = current;

                Vector3 link = current - previous;
                float angle = Mathf.Atan2(Vector3.Dot(link, cameraUp),
                    Vector3.Dot(link, cameraRight)) * Mathf.Rad2Deg;
                segment.transform.position = (previous + current) * .5f;
                segment.transform.rotation = gameCamera.transform.rotation *
                    Quaternion.Euler(0f, 0f, angle);
                segment.transform.localScale = new Vector3(
                    FuseSegmentScale * linkAmount, FuseSegmentScale,
                    FuseSegmentScale);
                previous = current;
            }

            fighter.fuseFlame.enabled = !hidden && visibleCount > 0;
            if (fighter.fuseFlame.enabled)
            {
                int flameFrame = Mathf.FloorToInt(
                    Time.time * FuseFlameFramesPerSecond) % 3;
                fighter.fuseFlame.sprite = fighter.fuseFlameFrames[flameFrame];
                fighter.fuseFlame.transform.position = previous;
                fighter.fuseFlame.transform.rotation = gameCamera.transform.rotation;
            }

            fighter.fuseAlert.enabled = !hidden && fighter.firePending;
            if (fighter.fuseAlert.enabled)
            {
                int alertFrame = Mathf.FloorToInt(
                    Time.time * FuseAlertFramesPerSecond) % 2;
                fighter.fuseAlert.sprite = fighter.fuseAlertFrames[alertFrame];
                fighter.fuseAlert.transform.position = fighter.muzzle.position +
                    cameraUp * .38f;
                fighter.fuseAlert.transform.rotation = gameCamera.transform.rotation;
                float pulse = 1f + Mathf.PingPong(Time.time * 5f, .22f);
                fighter.fuseAlert.transform.localScale = Vector3.one * (.28f * pulse);
            }
        }

        private void UpdateSpriteAnimation(Fighter fighter, float movementSpeed)
        {
            if (fighter.spriteRenderer == null || fighter.spriteFrames == null) return;

            string state;
            bool loop = true;
            if (fighter.dead)
            {
                state = "Death";
                loop = false;
            }
            else if (fighter.isPlayer && Time.time < fighter.meleeAttackUntil)
                state = "Melee";
            else if (fighter.isPlayer && Time.time < fighter.meleeRecoverUntil)
                state = "MeleeRecover";
            else
                state = movementSpeed > .08f ? "Move" : "Idle";

            if (fighter.animationState != state)
            {
                fighter.animationState = state;
                fighter.animationStartedAt = Time.time;
            }

            int elapsedFrame = Mathf.FloorToInt(
                (Time.time - fighter.animationStartedAt) * SpriteFramesPerSecond);
            int frame = loop ? elapsedFrame % 4 : Mathf.Min(elapsedFrame, 3);
            string prefix = fighter.isPlayer ? "Player" : "Enemy";
            Sprite sprite;
            if (fighter.spriteFrames.TryGetValue(
                prefix + "_" + state + "_" + frame, out sprite))
                fighter.spriteRenderer.sprite = sprite;

            Vector3 forward = fighter.gunPivot.forward;
            if (Mathf.Abs(forward.x) > .08f)
                fighter.spriteRenderer.flipX = forward.x < 0f;
        }

        private void UpdateMeleeRangeIndicator(Fighter fighter)
        {
            if (fighter.meleeCooldownRing == null) return;
            float cooldown = Mathf.Clamp01(
                1f - (meleeReadyAt - Time.time) / MeleeCooldown);
            int segments = Mathf.Max(2, Mathf.CeilToInt(64f * cooldown));
            SetRingPositions(fighter.meleeCooldownRing, MeleeRange,
                segments, cooldown >= .999f, cooldown);
            Color color = cooldown >= .999f
                ? new Color(.25f, 1f, .58f, .95f)
                : new Color(1f, .58f, .16f, .82f);
            fighter.meleeCooldownRing.startColor =
                fighter.meleeCooldownRing.endColor = color;
        }

        private Vector3 ClampToArena(Vector3 value)
        {
            value.x = Mathf.Clamp(value.x, -ArenaWidth * .5f + .65f, ArenaWidth * .5f - .65f);
            value.z = Mathf.Clamp(value.z, -ArenaDepth * .5f + .65f, ArenaDepth * .5f - .65f);
            value.y = FighterGroundHeight;
            return value;
        }

        private Vector3 PointerArenaPoint(Vector3 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, Vector3.up * .9f);
            return plane.Raycast(ray, out float enter)
                ? ray.GetPoint(enter)
                : player.gunPivot.position + player.gunPivot.forward * 10f;
        }

        private void UpdateCamera(bool immediate)
        {
            if (gameCamera == null || player == null) return;
            float targetFieldOfView = Mathf.Lerp(DefaultCameraFieldOfView,
                AimingCameraFieldOfView, aimFeedback);
            gameCamera.fieldOfView = immediate ? targetFieldOfView :
                Mathf.Lerp(gameCamera.fieldOfView, targetFieldOfView,
                    1f - Mathf.Exp(-AimFeedbackSpeed * Time.deltaTime));
            Vector3 follow = new Vector3(player.root.position.x, 7.8f,
                player.root.position.z - 8.8f);
            Vector3 cameraOffset = follow - player.root.position;
            follow = player.root.position +
                cameraOffset * CameraDistanceMultiplier;
            if (shake > 0f)
                follow += Random.insideUnitSphere * shake;
            gameCamera.transform.position = immediate ? follow :
                Vector3.Lerp(gameCamera.transform.position, follow, Time.deltaTime * 5f);
            Quaternion rotation = Quaternion.LookRotation(
                player.root.position + new Vector3(0f, .75f, .8f) -
                gameCamera.transform.position, Vector3.up);
            gameCamera.transform.rotation = immediate ? rotation :
                Quaternion.Slerp(gameCamera.transform.rotation, rotation, Time.deltaTime * 5f);
        }

        private void UpdateAimFeedback(float dt)
        {
            float target = player != null && player.aiming && !finished ? 1f : 0f;
            aimFeedback = Mathf.MoveTowards(aimFeedback, target,
                AimFeedbackSpeed * dt);
        }

        private static Texture2D CreateAimVignetteTexture()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated Aim Vignette",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x + .5f) / size * 2f - 1f;
                float ny = (y + .5f) / size * 2f - 1f;
                float edge = Mathf.Clamp01((Mathf.Sqrt(nx * nx + ny * ny) - .45f) / .55f);
                float alpha = edge * edge * .78f;
                pixels[y * size + x] = new Color(0f, .015f, .025f, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void Play(string resourcePath, float fallbackFrequency, float duration, float volume)
        {
            var clip = Resources.Load<AudioClip>("CountDown/" + resourcePath);
            if (clip == null) clip = Tone(fallbackFrequency, duration);
            audioSource.PlayOneShot(clip, volume);
        }

        private static AudioClip Tone(float frequency, float duration)
        {
            int sampleRate = 22050;
            int length = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)length;
                samples[i] = Mathf.Sin(t * frequency * Mathf.PI * 2f) * envelope * .25f;
            }
            var clip = AudioClip.Create("Generated Tone", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void InitStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * .09f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(15, Mathf.RoundToInt(Screen.height * .023f)),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(.88f, .9f, .92f) }
            };
            countStyle = new GUIStyle(labelStyle) { fontSize = 30 };
            helpStyle = new GUIStyle(labelStyle)
            {
                fontSize = Mathf.Max(12, Mathf.RoundToInt(Screen.height * .018f)),
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(.72f, .75f, .78f) }
            };
            aimStyle = new GUIStyle(labelStyle)
            {
                fontSize = Mathf.Max(13, Mathf.RoundToInt(Screen.height * .021f)),
                normal = { textColor = Color.white }
            };
            guideTitleStyle = new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.Max(17, Mathf.RoundToInt(Screen.height * .025f)),
                normal = { textColor = new Color(.96f, .85f, .38f) }
            };
            guideLabelStyle = new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.Max(13, Mathf.RoundToInt(Screen.height * .018f)),
                wordWrap = true,
                normal = { textColor = new Color(.93f, .95f, .98f) }
            };
            guideKeyStyle = new GUIStyle(guideLabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(11, Mathf.RoundToInt(Screen.height * .016f)),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            if (player == null) return;
            GUI.depth = -100;
            InitStyles();
            DrawAimVignette();
            DrawTopStatus();
            DrawPlayerStatus();
            DrawWorldCount(player);
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead)
                    DrawWorldCount(enemies[i]);
            DrawCrosshair();
            DrawAimState();
            DrawTouchControls();
            DrawStartupGuide();
            GUI.Label(new Rect(0f, Screen.height - 30f, Screen.width, 24f),
                InputHelpText(), helpStyle);

            if (finished)
            {
                GUI.color = new Color(0f, 0f, 0f, .72f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), white);
                GUI.color = Color.white;
                GUI.Label(new Rect(0f, Screen.height * .32f, Screen.width,
                    Screen.height * .16f), "YOU ARE DOWN", titleStyle);
                GUI.Label(new Rect(0f, Screen.height * .5f, Screen.width, 40f),
                    inputMode == InputMode.Touch ? "TAP TO RESTART" :
                    inputMode == InputMode.Gamepad ? "A TO RESTART" :
                    "R TO RESTART", labelStyle);
            }
            else if (stageTransitioning)
            {
                GUI.Label(new Rect(0f, Screen.height * .38f, Screen.width,
                    Screen.height * .14f), "STAGE CLEAR", titleStyle);
            }

            DrawBuildVersion();
        }

        private void DrawStartupGuide()
        {
            if (!awaitingStart || finished || stageTransitioning)
                return;

            Rect safe = Screen.safeArea;
            float scale = Mathf.Clamp(Screen.height / 720f, .72f, 1.25f);
            float width = Mathf.Min(520f * scale, safe.width - 24f);
            float rowHeight = 62f * scale;
            float footerHeight = 54f * scale;
            float height = 52f * scale + rowHeight * 4f + footerHeight;
            Rect panel = new Rect(
                safe.xMin + (safe.width - width) * .5f,
                Screen.height - safe.yMax + Mathf.Max(18f * scale,
                    (safe.height - height) * .5f),
                width, height);

            DrawPanel(panel, new Color(.025f, .03f, .04f, .82f));
            GUI.Label(new Rect(panel.x + 20f * scale, panel.y + 8f * scale,
                panel.width - 40f * scale, 38f * scale),
                GuideText("CONTROLS", "조작 가이드"), guideTitleStyle);

            float y = panel.y + 50f * scale;
            DrawGuideRow(new Rect(panel.x + 14f * scale, y,
                    panel.width - 28f * scale, rowHeight),
                GuideText("Move", "상하좌우 이동"), scale, "WASD", "L");
            y += rowHeight;
            DrawGuideRow(new Rect(panel.x + 14f * scale, y,
                    panel.width - 28f * scale, rowHeight),
                GuideText("Press / hold to aim", "누르고 유지해 조준"),
                scale, "RMB", "TOUCH", "RT");
            y += rowHeight;
            DrawGuideRow(new Rect(panel.x + 14f * scale, y,
                    panel.width - 28f * scale, rowHeight),
                GuideText("Release aim to dash", "조준을 떼면 대시"),
                scale, "UP", "TOUCH", "RT");
            y += rowHeight;
            DrawGuideRow(new Rect(panel.x + 14f * scale, y,
                    panel.width - 28f * scale, rowHeight),
                GuideText("Choose aim direction", "조준 방향 선택"),
                scale, "MOUSE", "TOUCH", "R");
            y += rowHeight;
            string start = inputMode == InputMode.Touch
                ? GuideText("TOUCH TO START", "터치하여 시작")
                : inputMode == InputMode.Gamepad
                    ? GuideText("PRESS A TO START", "A 버튼을 눌러 시작")
                    : GuideText("CLICK TO START", "클릭하여 시작");
            GUI.Label(new Rect(panel.x + 14f * scale, y,
                panel.width - 28f * scale, footerHeight), start, aimStyle);
        }

        private void DrawGuideRow(Rect rect, string description, float scale,
            params string[] icons)
        {
            DrawPanel(rect, new Color(.08f, .095f, .12f, .76f));
            float iconHeight = rect.height - 14f * scale;
            float x = rect.x + 8f * scale;
            float iconWidth = icons.Length > 2 ? 58f * scale : 76f * scale;
            for (int i = 0; i < icons.Length; i++)
            {
                DrawGuideKey(new Rect(x, rect.y + 7f * scale,
                    iconWidth, iconHeight), icons[i]);
                x += iconWidth + 7f * scale;
            }
            GUI.Label(new Rect(x + 7f * scale, rect.y,
                rect.xMax - x - 13f * scale, rect.height),
                description, guideLabelStyle);
        }

        private void DrawGuideKey(Rect rect, string label)
        {
            DrawPanel(rect, new Color(.14f, .17f, .21f, 1f));
            GUI.color = new Color(.3f, .82f, 1f, GUI.color.a);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), white);
            GUI.color = new Color(1f, 1f, 1f, GUI.color.a);
            GUI.Label(rect, label, guideKeyStyle);
        }

        private static string GuideText(string english, string korean)
        {
            return Application.systemLanguage == SystemLanguage.Korean
                ? korean : english;
        }

        private void DrawAimVignette()
        {
            if (aimFeedback <= .001f || aimVignette == null) return;
            GUI.color = new Color(1f, 1f, 1f, aimFeedback);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                aimVignette, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
        }

        private void DrawBuildVersion()
        {
            if (buildVersionTexture == null)
                buildVersionTexture = CreateBuildVersionTexture(
                    "BUILD " + Application.version);

            Rect safe = Screen.safeArea;
            float pixelSize = Mathf.Clamp(Mathf.Round(Screen.height / 360f), 2f, 4f);
            float textWidth = buildVersionTexture.width * pixelSize;
            float textHeight = buildVersionTexture.height * pixelSize;
            float width = textWidth + 16f;
            float height = textHeight + 16f;
            float x = Mathf.Max(8f, safe.xMax - width - 12f);
            float y = Mathf.Max(8f, Screen.height - safe.yMax + 12f);
            Rect panel = new Rect(x, y, width, height);
            DrawPanel(panel, new Color(.02f, .025f, .03f, .9f));
            GUI.color = new Color(.82f, .86f, .92f);
            GUI.DrawTexture(new Rect(panel.x + 8f, panel.y + 8f,
                textWidth, textHeight), buildVersionTexture, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
        }

        private static Texture2D CreateBuildVersionTexture(string text)
        {
            const int glyphWidth = 3;
            const int glyphHeight = 5;
            const int advance = 4;
            int width = Mathf.Max(1, text.Length * advance - 1);
            var texture = new Texture2D(width, glyphHeight, TextureFormat.RGBA32, false)
            {
                name = "Embedded Build Version",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[width * glyphHeight];

            for (int characterIndex = 0; characterIndex < text.Length; characterIndex++)
            {
                string glyph = VersionGlyph(char.ToUpperInvariant(text[characterIndex]));
                for (int row = 0; row < glyphHeight; row++)
                for (int column = 0; column < glyphWidth; column++)
                {
                    if (glyph[row * glyphWidth + column] != '1') continue;
                    int x = characterIndex * advance + column;
                    int y = glyphHeight - row - 1;
                    pixels[y * width + x] = new Color32(255, 255, 255, 255);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static string VersionGlyph(char character)
        {
            switch (character)
            {
                case 'B': return "110101110101110";
                case 'U': return "101101101101111";
                case 'I': return "111010010010111";
                case 'L': return "100100100100111";
                case 'D': return "110101101101110";
                case '0': return "111101101101111";
                case '1': return "010110010010111";
                case '2': return "111001111100111";
                case '3': return "111001111001111";
                case '4': return "101101111001001";
                case '5': return "111100111001111";
                case '6': return "111100111101111";
                case '7': return "111001001001001";
                case '8': return "111101111101111";
                case '9': return "111101111001111";
                case '.': return "000000000000010";
                case '-': return "000000111000000";
                case ' ': return "000000000000000";
                default: return "111101101101111";
            }
        }

        private void DrawAimState()
        {
            Rect badge = new Rect((Screen.width - 220f) * .5f, 82f, 220f, 34f);
            DrawPanel(badge, player.aiming
                ? new Color(.08f, .72f, .38f, .92f)
                : new Color(.1f, .11f, .12f, .72f));
            string mode = inputMode == InputMode.Touch ? "TOUCH" :
                inputMode == InputMode.Gamepad ? "GAMEPAD" : "CLICK";
            GUI.Label(badge, mode + " • " +
                (player.aiming ? "AIMING" : "MOVE MODE"), aimStyle);
        }

        private void DrawTouchControls()
        {
            if (inputMode != InputMode.Touch) return;
            DrawStick(moveTouchId >= 0 ? moveTouchOrigin : new Vector2(92f, 100f),
                moveTouchId >= 0 ? moveTouchPosition : new Vector2(92f, 100f), "MOVE");
            DrawStick(aimTouchId >= 0 ? aimTouchOrigin :
                    new Vector2(Screen.width - 92f, 100f),
                aimTouchId >= 0 ? aimTouchPosition :
                    new Vector2(Screen.width - 92f, 100f), "AIM");
        }

        private string InputHelpText()
        {
            if (inputMode == InputMode.Touch)
                return "LEFT TOUCH MOVE   •   RIGHT TOUCH AIM";
            if (inputMode == InputMode.Gamepad)
                return "LEFT STICK MOVE   •   RIGHT STICK AIM   •   RT HOLD AIM   •   AUTO BASH";
            return "WASD MOVE   •   HOLD RMB AIM   •   AUTO BASH   •   ESC QUIT";
        }

        private void DrawStick(Vector2 origin, Vector2 position, string label)
        {
            origin.y = Screen.height - origin.y;
            position.y = Screen.height - position.y;
            position = origin + Vector2.ClampMagnitude(position - origin, TouchStickRadius);
            GUI.color = new Color(1f, 1f, 1f, .16f);
            GUI.DrawTexture(new Rect(origin.x - TouchStickRadius, origin.y - TouchStickRadius,
                TouchStickRadius * 2f, TouchStickRadius * 2f), white);
            GUI.color = new Color(1f, 1f, 1f, .42f);
            GUI.DrawTexture(new Rect(position.x - 28f, position.y - 28f, 56f, 56f), white);
            GUI.color = Color.white;
            GUI.Label(new Rect(origin.x - 55f, origin.y + TouchStickRadius + 4f, 110f, 24f),
                label, helpStyle);
        }

        private void DrawTopStatus()
        {
            float width = Mathf.Min(400f, Screen.width * .42f);
            Rect panel = new Rect((Screen.width - width) * .5f, 16f, width, 58f);
            DrawPanel(panel, new Color(.05f, .06f, .07f, .82f));
            int alive = 0;
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].dead) alive++;
            GUI.Label(new Rect(panel.x, panel.y + 3f, width, 20f),
                "STAGE " + stage + "   •   ENEMIES " + alive, helpStyle);
            Fighter target = player.opponent != null && !player.opponent.dead
                ? player.opponent : ClosestAliveEnemy();
            if (target != null)
                DrawHearts(new Rect(panel.x + 16f, panel.y + 29f,
                    width - 32f, 18f), target.health, false);
        }

        private void DrawPlayerStatus()
        {
            Rect panel = new Rect(20f, Screen.height - 126f, 260f, 74f);
            DrawPanel(panel, new Color(.05f, .06f, .07f, .82f));
            GUI.Label(new Rect(panel.x + 10f, panel.y + 4f, 90f, 20f), "PLAYER", helpStyle);
            DrawHearts(new Rect(panel.x + 108f, panel.y + 8f, 135f, 16f),
                player.health, true);
            float cooldown = Mathf.Clamp01(
                1f - (meleeReadyAt - Time.time) / MeleeCooldown);
            Rect bar = new Rect(panel.x + 14f, panel.y + 43f, panel.width - 28f, 12f);
            GUI.color = new Color(.18f, .2f, .22f);
            GUI.DrawTexture(bar, white);
            GUI.color = cooldown >= 1f ? new Color(.3f, 1f, .62f) : new Color(.95f, .65f, .18f);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * cooldown, bar.height), white);
            GUI.color = Color.white;
        }

        private void DrawWorldCount(Fighter fighter)
        {
            float height = fighter.isPlayer ? 2.2f : 2.48f;
            Vector3 point = gameCamera.WorldToScreenPoint(
                fighter.root.position + Vector3.up * height);
            if (point.z <= 0f) return;
            float size = fighter.count >= 3 ? 34f : fighter.count == 2 ? 42f :
                fighter.count == 1 ? 54f : 64f;
            size += fighter.pulse * 10f;
            countStyle.fontSize = Mathf.RoundToInt(size);
            countStyle.normal.textColor = fighter.count == 1 ? new Color(1f, .72f, .12f) :
                fighter.count == 0 ? Color.white : fighter.accent;
            GUI.Label(new Rect(point.x - 60f, Screen.height - point.y - 34f, 120f, 70f),
                fighter.count.ToString(), countStyle);
            if (!fighter.isPlayer)
                DrawWorldEnemyHealth(fighter);
        }

        private void DrawWorldEnemyHealth(Fighter fighter)
        {
            Vector3 point = gameCamera.WorldToScreenPoint(
                fighter.root.position + Vector3.up * 2.02f);
            if (point.z <= 0f) return;

            const float cellWidth = 18f;
            const float cellHeight = 7f;
            const float gap = 3f;
            const float padding = 4f;
            float width = cellWidth * MaxHealth + gap * (MaxHealth - 1);
            Rect panel = new Rect(point.x - width * .5f - padding,
                Screen.height - point.y - cellHeight * .5f - padding,
                width + padding * 2f, cellHeight + padding * 2f);
            DrawPanel(panel, new Color(.025f, .03f, .04f, .78f));
            for (int i = 0; i < MaxHealth; i++)
            {
                GUI.color = i < fighter.health
                    ? new Color(1f, .25f, .18f)
                    : new Color(.17f, .18f, .2f);
                GUI.DrawTexture(new Rect(panel.x + padding + i * (cellWidth + gap),
                    panel.y + padding, cellWidth, cellHeight), white);
            }
            GUI.color = Color.white;
        }

        private void DrawCrosshair()
        {
            if (inputMode == InputMode.Gamepad) return;
            Vector2 mouse;
            if (inputMode == InputMode.Touch)
                mouse = new Vector2(aimTouchPosition.x, Screen.height - aimTouchPosition.y);
            else
                mouse = new Vector2(mouseAimPosition.x, Screen.height - mouseAimPosition.y);
            Color color = player.hasTarget && player.aiming
                ? new Color(.3f, 1f, .62f) : new Color(1f, 1f, 1f, .75f);
            GUI.color = color;
            GUI.DrawTexture(new Rect(mouse.x - 13f, mouse.y - 1f, 9f, 2f), white);
            GUI.DrawTexture(new Rect(mouse.x + 4f, mouse.y - 1f, 9f, 2f), white);
            GUI.DrawTexture(new Rect(mouse.x - 1f, mouse.y - 13f, 2f, 9f), white);
            GUI.DrawTexture(new Rect(mouse.x - 1f, mouse.y + 4f, 2f, 9f), white);
            GUI.color = Color.white;
        }

        private void DrawHearts(Rect rect, int health, bool playerSide)
        {
            float gap = 7f;
            float cell = (rect.width - gap * 2f) / 3f;
            for (int i = 0; i < 3; i++)
            {
                GUI.color = i < health
                    ? (playerSide ? new Color(.12f, .72f, 1f) : new Color(1f, .25f, .18f))
                    : new Color(.18f, .19f, .2f);
                GUI.DrawTexture(new Rect(rect.x + i * (cell + gap), rect.y, cell, rect.height), white);
            }
            GUI.color = Color.white;
        }

        private void DrawPanel(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, white);
            GUI.color = Color.white;
        }

        private sealed class Fighter
        {
            public Transform root;
            public Transform body;
            public Transform gunPivot;
            public Transform muzzle;
            public LineRenderer laser;
            public LineRenderer meleeRangeRing;
            public LineRenderer meleeCooldownRing;
            public Renderer bodyRenderer;
            public SpriteRenderer spriteRenderer;
            public SpriteRenderer aimLockMarker;
            public Dictionary<string, Sprite> spriteFrames;
            public Color baseBodyColor;
            public GameObject fireWarning;
            public Fighter opponent;
            public Fighter aimLockTarget;
            public bool isPlayer;
            public string displayName;
            public Color accent;
            public int health;
            public int count;
            public float countTimer;
            public bool aiming;
            public bool hasTarget;
            public bool wasActivelyTargeting;
            public bool dead;
            public float stunnedUntil;
            public float aimGraceUntil;
            public float nextDecisionAt;
            public float evadeDirection;
            public bool evading;
            public float closeDetectedAt;
            public float pulse;
            public float hitFlash;
            public string animationState;
            public float animationStartedAt;
            public float meleeAttackUntil;
            public float meleeRecoverUntil;
            public SpriteRenderer[] fuseSegments;
            public Vector3[] fusePositions;
            public SpriteRenderer fuseFlame;
            public Sprite[] fuseFlameFrames;
            public SpriteRenderer fuseAlert;
            public Sprite[] fuseAlertFrames;
            public int startingCount;
            public bool firePending;
            public float pendingFireAt;
            public Fighter pendingFireTarget;
            public Vector3 pendingFireDirection;
            public Vector3 lastPosition;
            public int countMin = 3;
            public int countMax = 7;

            public void ResetCount()
            {
                count = Random.Range(countMin, countMax + 1);
                startingCount = count;
                countTimer = 0f;
                pulse = 1f;
                firePending = false;
                pendingFireTarget = null;
                pendingFireDirection = Vector3.zero;
            }
        }

        private sealed class ProjectileThreat
        {
            public Vector3 position;
            public Vector3 direction;
            public bool fromPlayer;
        }
    }
}
