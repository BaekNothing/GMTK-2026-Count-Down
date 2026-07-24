using System.Collections;
using UnityEngine;

namespace CountDown
{
    /// <summary>
    /// A self-contained, resource-optional implementation of COUNT DOWN.
    /// Put matching assets under Resources/CountDown to replace the fallbacks.
    /// </summary>
    public sealed class CountDownGame : MonoBehaviour
    {
        private const float ArenaWidth = 12f;
        private const float ArenaDepth = 8f;
        private const float BodyRadius = .48f;
        private const float BodyHeight = 1.7f;
        private const float FighterGroundHeight = .04f;
        private const int MaxHealth = 3;

        private Fighter player;
        private Fighter enemy;
        private Camera gameCamera;
        private AudioSource audioSource;
        private bool finished;
        private float meleeReadyAt;
        private float shake;
        private Texture2D white;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle countStyle;
        private GUIStyle helpStyle;
        private GUIStyle aimStyle;
        private GUIStyle versionStyle;
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
            gameCamera.fieldOfView = 47f;
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
            enemy = CreateFighter("TARGET", false,
                new Vector3(0f, FighterGroundHeight, 2.75f),
                new Color(1f, .24f, .18f), "Enemy");
            player.opponent = enemy;
            enemy.opponent = player;
            player.ResetCount();
            enemy.ResetCount();

            audioSource = NewObject("Audio").AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            meleeReadyAt = Time.time;
            UpdateCamera(true);
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

            for (int i = -5; i <= 5; i++)
                CreateStripe(new Vector3(i, .006f, 0f), new Vector3(.018f, .01f, ArenaDepth));
            for (int i = -3; i <= 3; i++)
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
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = BodyRadius;
            capsule.height = BodyHeight;
            capsule.center = Vector3.up * (BodyHeight * .5f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = resourcePrefix + " Panel";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, .88f, 0f);
            body.transform.localScale = new Vector3(1.08f, 1.72f, .16f);
            body.GetComponent<Renderer>().material = MaterialFor(
                "Materials/" + resourcePrefix, color);
            Destroy(body.GetComponent<Collider>());

            var sprite = Resources.Load<Sprite>("CountDown/Sprites/" + resourcePrefix + "Default");
            if (sprite != null)
            {
                body.SetActive(false);
                var spriteObject = NewObject(resourcePrefix + " Sprite");
                spriteObject.transform.SetParent(root.transform);
                spriteObject.transform.localPosition = Vector3.up * .88f;
                var sr = spriteObject.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = Color.white;
                float height = Mathf.Max(.01f, sprite.bounds.size.y);
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

            return new Fighter
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
            UpdateTouchControls();

            if (finished)
            {
                if (Input.GetKeyDown(KeyCode.R))
                    RestartGame();
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                    RestartGame();
                if (Input.GetKeyDown(KeyCode.Escape))
                    Quit();
                UpdateCamera(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Quit();

            float dt = Mathf.Min(Time.deltaTime, .05f);
            UpdatePlayer(dt);
            UpdateEnemy(dt);
            UpdateWeapon(player, dt);
            UpdateWeapon(enemy, dt);
            UpdatePresentation(player, dt);
            UpdatePresentation(enemy, dt);
            UpdateCamera(false);
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.8f);
        }

        private void UpdatePlayer(float dt)
        {
            if (Time.time < player.stunnedUntil)
            {
                player.aiming = false;
                return;
            }

            player.aiming = touchAiming || Input.GetMouseButton(1);
            Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0f,
                Input.GetAxisRaw("Vertical"));
            if (touchMove.sqrMagnitude > move.sqrMagnitude)
                move = new Vector3(touchMove.x, 0f, touchMove.y);
            move = Vector3.ClampMagnitude(move, 1f);
            float speed = player.aiming ? 2f : 5f;
            player.root.position = ClampToArena(player.root.position + move * speed * dt);

            if (!touchAiming)
                mouseAimPosition = Input.mousePosition;

            Vector3 direction;
            if (touchAiming)
            {
                if (touchAim.sqrMagnitude > .01f)
                    aimDirection = touchAim.normalized;
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
            player.gunPivot.rotation = Quaternion.LookRotation(direction, Vector3.up);

            if (Input.GetKeyDown(KeyCode.Space) && Time.time >= meleeReadyAt)
                StartCoroutine(Melee());
        }

        private void RestartGame()
        {
            StopAllCoroutines();
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            player = null;
            enemy = null;
            gameCamera = null;
            audioSource = null;
            finished = false;
            shake = 0f;
            moveTouchId = aimTouchId = -1;
            touchMove = Vector2.zero;
            touchAim = Vector2.zero;
            touchAiming = false;
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

        private void UpdateEnemy(float dt)
        {
            if (Time.time < enemy.stunnedUntil)
            {
                enemy.aiming = false;
                return;
            }

            Vector3 toPlayer = player.root.position - enemy.root.position;
            float distance = toPlayer.magnitude;

            if (distance < 2.5f)
            {
                if (enemy.closeDetectedAt <= 0f) enemy.closeDetectedAt = Time.time;
                if (Time.time - enemy.closeDetectedAt >= .3f)
                    MoveEnemy(-toPlayer.normalized, dt);
            }
            else
            {
                enemy.closeDetectedAt = 0f;
                bool danger = player.count <= 1 && player.aiming && player.hasTarget;
                if (danger && Time.time >= enemy.nextDecisionAt)
                {
                    enemy.evading = Random.value < .7f;
                    enemy.evadeDirection = Random.value < .5f ? -1f : 1f;
                    enemy.nextDecisionAt = Time.time + Random.Range(.45f, .8f);
                }

                if (enemy.evading && Time.time < enemy.nextDecisionAt)
                {
                    Vector3 side = Vector3.Cross(Vector3.up, toPlayer.normalized) * enemy.evadeDirection;
                    MoveEnemy(side, dt);
                }
                else
                {
                    enemy.evading = false;
                    if (distance > 5.2f) MoveEnemy(toPlayer.normalized, dt * .45f);
                    else if (distance < 3.4f) MoveEnemy(-toPlayer.normalized, dt * .4f);
                }
            }

            Vector3 flatTarget = player.root.position + Vector3.up * .85f - enemy.gunPivot.position;
            flatTarget.y = 0f;
            if (flatTarget.sqrMagnitude > .01f)
            {
                Quaternion desired = Quaternion.LookRotation(flatTarget.normalized, Vector3.up);
                enemy.gunPivot.rotation = Quaternion.RotateTowards(
                    enemy.gunPivot.rotation, desired, EnemyTurnSpeed * dt);
            }
            enemy.aiming = true;
        }

        private void MoveEnemy(Vector3 direction, float dt)
        {
            direction.y = 0f;
            enemy.root.position = ClampToArena(enemy.root.position +
                direction.normalized * 4f * dt);
        }

        private void UpdateWeapon(Fighter fighter, float dt)
        {
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(fighter.muzzle.position,
                fighter.muzzle.forward, out hit, 30f);
            Vector3 end = hitSomething ? hit.point :
                fighter.muzzle.position + fighter.muzzle.forward * 30f;
            fighter.hasTarget = hitSomething &&
                hit.collider.transform.IsChildOf(fighter.opponent.root);

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

            bool canCount = fighter.aiming && fighter.hasTarget &&
                Time.time >= fighter.stunnedUntil;
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
                        Fire(fighter);
                        break;
                    }
                }
            }
        }

        private void Fire(Fighter fighter)
        {
            RaycastHit hit;
            bool connected = Physics.Raycast(fighter.muzzle.position,
                fighter.muzzle.forward, out hit, 30f) &&
                hit.collider.transform.IsChildOf(fighter.opponent.root);
            Play("Audio/Gunshot", 115f, .13f, .75f);
            StartCoroutine(MuzzleFlash(fighter));
            if (connected) Damage(fighter.opponent, 1, fighter);
            fighter.ResetCount();
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
            meleeReadyAt = Time.time + 7f;
            player.stunnedUntil = Time.time + .16f;
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

            float distance = Vector3.Distance(player.root.position, enemy.root.position);
            if (distance <= 1.8f && enemy.health > 0)
            {
                enemy.stunnedUntil = Time.time + .7f;
                enemy.aiming = false;
                enemy.ResetCount();
                Damage(enemy, 1, player);
                Play("Audio/MeleeHit", 92f, .1f, .7f);
            }
            else
            {
                player.stunnedUntil = Time.time + .5f;
                Play("Audio/MeleeMiss", 180f, .08f, .24f);
            }

            yield return new WaitForSeconds(.12f);
            player.gunPivot.localEulerAngles = original;
        }

        private void Damage(Fighter target, int amount, Fighter source)
        {
            if (finished) return;
            target.health = Mathf.Max(0, target.health - amount);
            target.hitFlash = 1f;
            shake = .24f;
            Play("Audio/Impact", 72f, .09f, .6f);
            if (target.health <= 0)
            {
                finished = true;
                target.dead = true;
                player.aiming = enemy.aiming = false;
                Play(target.isPlayer ? "Audio/Lose" : "Audio/Win",
                    target.isPlayer ? 110f : 440f, .45f, .55f);
            }
        }

        private void UpdatePresentation(Fighter fighter, float dt)
        {
            fighter.pulse = Mathf.MoveTowards(fighter.pulse, 0f, dt * 3f);
            fighter.hitFlash = Mathf.MoveTowards(fighter.hitFlash, 0f, dt * 4f);
            Vector3 lean = fighter.lastPosition == Vector3.zero ? Vector3.zero :
                (fighter.root.position - fighter.lastPosition) / Mathf.Max(dt, .001f);
            fighter.lastPosition = fighter.root.position;

            Quaternion targetRotation = fighter.dead
                ? Quaternion.Euler(78f, 0f, fighter.isPlayer ? -8f : 8f)
                : Quaternion.Euler(0f, 0f, Mathf.Clamp(-lean.x * 2.2f, -8f, 8f));
            fighter.body.localRotation = Quaternion.Slerp(
                fighter.body.localRotation, targetRotation, dt * (fighter.dead ? 4f : 9f));
            if (!fighter.dead)
                fighter.body.localPosition = new Vector3(0f,
                    .88f + Mathf.Sin(Time.time * 7f) * Mathf.Min(lean.magnitude * .006f, .035f), 0f);
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
            Vector3 follow = new Vector3(player.root.position.x * .22f, 7.8f,
                player.root.position.z - 8.8f);
            if (shake > 0f)
                follow += Random.insideUnitSphere * shake;
            gameCamera.transform.position = immediate ? follow :
                Vector3.Lerp(gameCamera.transform.position, follow, Time.deltaTime * 5f);
            Quaternion rotation = Quaternion.LookRotation(
                new Vector3(0f, .75f, .8f) - gameCamera.transform.position, Vector3.up);
            gameCamera.transform.rotation = immediate ? rotation :
                Quaternion.Slerp(gameCamera.transform.rotation, rotation, Time.deltaTime * 5f);
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
            versionStyle = new GUIStyle(helpStyle)
            {
                alignment = TextAnchor.UpperRight,
                normal = { textColor = new Color(.75f, .78f, .82f, .9f) }
            };
        }

        private void OnGUI()
        {
            if (player == null || enemy == null) return;
            GUI.depth = -100;
            InitStyles();
            DrawTopStatus();
            DrawPlayerStatus();
            DrawWorldCount(player);
            DrawWorldCount(enemy);
            DrawCrosshair();
            DrawAimState();
            DrawTouchControls();
            GUI.Label(new Rect(0f, Screen.height - 30f, Screen.width, 24f),
                "WASD MOVE   •   HOLD RMB AIM   •   SPACE BASH   •   ESC QUIT", helpStyle);

            if (finished)
            {
                GUI.color = new Color(0f, 0f, 0f, .72f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), white);
                GUI.color = Color.white;
                string result = player.health > 0 ? "COUNT DOWN" : "YOU ARE DOWN";
                GUI.Label(new Rect(0f, Screen.height * .32f, Screen.width,
                    Screen.height * .16f), result, titleStyle);
                GUI.Label(new Rect(0f, Screen.height * .5f, Screen.width, 40f),
                    Input.touchSupported ? "TAP TO RESTART" : "R TO RESTART", labelStyle);
            }

            DrawBuildVersion();
        }

        private void DrawBuildVersion()
        {
            Rect safe = Screen.safeArea;
            float width = Mathf.Min(210f, Mathf.Max(130f, safe.width * .32f));
            float x = Mathf.Max(8f, safe.xMax - width - 12f);
            float y = Mathf.Max(8f, Screen.height - safe.yMax + 12f);
            Rect panel = new Rect(x, y, width, 30f);
            DrawPanel(panel, new Color(.02f, .025f, .03f, .9f));
            GUI.Label(new Rect(panel.x + 8f, panel.y + 5f,
                panel.width - 16f, 20f), "BUILD " + Application.version, versionStyle);
        }

        private void DrawAimState()
        {
            Rect badge = new Rect((Screen.width - 150f) * .5f, 82f, 150f, 34f);
            DrawPanel(badge, player.aiming
                ? new Color(.08f, .72f, .38f, .92f)
                : new Color(.1f, .11f, .12f, .72f));
            GUI.Label(badge, player.aiming ? "AIMING" : "MOVE MODE", aimStyle);
        }

        private void DrawTouchControls()
        {
            if (!Input.touchSupported && Input.touchCount == 0) return;
            DrawStick(moveTouchId >= 0 ? moveTouchOrigin : new Vector2(92f, 100f),
                moveTouchId >= 0 ? moveTouchPosition : new Vector2(92f, 100f), "MOVE");
            DrawStick(aimTouchId >= 0 ? aimTouchOrigin :
                    new Vector2(Screen.width - 92f, 100f),
                aimTouchId >= 0 ? aimTouchPosition :
                    new Vector2(Screen.width - 92f, 100f), "AIM");
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
            GUI.Label(new Rect(panel.x, panel.y + 3f, width, 20f), "TARGET", helpStyle);
            DrawHearts(new Rect(panel.x + 16f, panel.y + 29f, width - 32f, 18f),
                enemy.health, false);
        }

        private void DrawPlayerStatus()
        {
            Rect panel = new Rect(20f, Screen.height - 126f, 260f, 74f);
            DrawPanel(panel, new Color(.05f, .06f, .07f, .82f));
            GUI.Label(new Rect(panel.x + 10f, panel.y + 4f, 90f, 20f), "PLAYER", helpStyle);
            DrawHearts(new Rect(panel.x + 108f, panel.y + 8f, 135f, 16f),
                player.health, true);
            float cooldown = Mathf.Clamp01(1f - (meleeReadyAt - Time.time) / 7f);
            Rect bar = new Rect(panel.x + 14f, panel.y + 43f, panel.width - 28f, 12f);
            GUI.color = new Color(.18f, .2f, .22f);
            GUI.DrawTexture(bar, white);
            GUI.color = cooldown >= 1f ? new Color(.3f, 1f, .62f) : new Color(.95f, .65f, .18f);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * cooldown, bar.height), white);
            GUI.color = Color.white;
        }

        private void DrawWorldCount(Fighter fighter)
        {
            Vector3 point = gameCamera.WorldToScreenPoint(fighter.root.position + Vector3.up * 2.2f);
            if (point.z <= 0f) return;
            float size = fighter.count >= 3 ? 34f : fighter.count == 2 ? 42f :
                fighter.count == 1 ? 54f : 64f;
            size += fighter.pulse * 10f;
            countStyle.fontSize = Mathf.RoundToInt(size);
            countStyle.normal.textColor = fighter.count == 1 ? new Color(1f, .72f, .12f) :
                fighter.count == 0 ? Color.white : fighter.accent;
            GUI.Label(new Rect(point.x - 60f, Screen.height - point.y - 34f, 120f, 70f),
                fighter.count.ToString(), countStyle);
        }

        private void DrawCrosshair()
        {
            Vector2 mouse;
            if (touchAiming)
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
            public Fighter opponent;
            public bool isPlayer;
            public string displayName;
            public Color accent;
            public int health;
            public int count;
            public float countTimer;
            public bool aiming;
            public bool hasTarget;
            public bool dead;
            public float stunnedUntil;
            public float nextDecisionAt;
            public float evadeDirection;
            public bool evading;
            public float closeDetectedAt;
            public float pulse;
            public float hitFlash;
            public Vector3 lastPosition;

            public void ResetCount()
            {
                count = Random.Range(3, 8);
                countTimer = 0f;
                pulse = 1f;
            }
        }
    }
}
