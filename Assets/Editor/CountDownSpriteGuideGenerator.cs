using System.IO;
using UnityEditor;
using UnityEngine;

namespace CountDown.Editor
{
    public static class CountDownSpriteGuideGenerator
    {
        private const int CellSize = 128;
        private const int Columns = 4;
        private const string OutputDirectory = "Assets/Resources/CountDown/Sprites";
        private static readonly Color32 GuideWhite = new Color32(255, 255, 255, 255);
        private static readonly Color32 GuideGray = new Color32(153, 153, 153, 255);
        private static readonly Color32 GuideDark = new Color32(68, 68, 68, 255);

        [MenuItem("COUNT DOWN/Regenerate Sprite Guides")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutputDirectory);
            if (!File.Exists(OutputDirectory + "/PlayerSheet.png"))
                GenerateSheet("PlayerSheet", new[]
                {
                    "Idle", "Move", "Death", "Melee", "MeleeRecover"
                }, new Color32(24, 178, 255, 255));
            if (!File.Exists(OutputDirectory + "/EnemySheet.png"))
                GenerateSheet("EnemySheet", new[]
                {
                    "Idle", "Move", "Death"
                }, new Color32(255, 62, 48, 255));
            if (!File.Exists(OutputDirectory + "/FuseSheet.png"))
                GenerateFuseSheet();
            GenerateResourceGuides();
            AssetDatabase.Refresh();
            SliceSheet(OutputDirectory + "/PlayerSheet.png", 5, "Player");
            SliceSheet(OutputDirectory + "/EnemySheet.png", 3, "Enemy");
            SliceFuseSheet(OutputDirectory + "/FuseSheet.png");
            ConfigureGeneratedMocks();
            AssetDatabase.SaveAssets();
            Debug.Log("COUNT DOWN sprite guides generated successfully.");
        }

        public static void RegenerateBackgroundAndUiGuides()
        {
            Directory.CreateDirectory(OutputDirectory);
            GenerateBackgroundAndUiResourceGuides();
            AssetDatabase.Refresh();
            ConfigureSingleSprite(OutputDirectory + "/BackgroundNear.png");
            AssetDatabase.SaveAssets();
            Debug.Log("COUNT DOWN background and UI guides generated successfully.");
        }

        private static void GenerateResourceGuides()
        {
            GenerateBackgroundAndUiResourceGuides();
            GenerateIcon("Gun", MockShape.Gun, new Color32(40, 45, 49, 255));
            GenerateIcon("Stand", MockShape.Disc, new Color32(24, 27, 30, 255));
            GenerateIcon("AimLine", MockShape.Line, new Color32(255, 255, 255, 255));
            GenerateIcon("AimLock", MockShape.Ring, new Color32(112, 255, 190, 230));
            GenerateIcon("Projectile", MockShape.Disc, new Color32(255, 236, 116, 255));
            GenerateIcon("ProjectileTrail", MockShape.Line, new Color32(255, 180, 76, 210));
            GenerateIcon("MuzzleFlash", MockShape.Burst, new Color32(255, 224, 90, 255));
            GenerateIcon("EnemyWarning", MockShape.Warning, new Color32(255, 80, 60, 225));
            GenerateIcon("MeleeRange", MockShape.Ring, new Color32(62, 205, 255, 150));
            GenerateIcon("MeleeCooldown", MockShape.Ring, new Color32(74, 255, 150, 230));
            GenerateIcon("Crosshair", MockShape.Crosshair, new Color32(255, 255, 255, 230));
            GenerateIcon("HealthPip", MockShape.Heart, new Color32(255, 72, 76, 255));
            GenerateIcon("TouchStick", MockShape.Ring, new Color32(255, 255, 255, 110));
            GenerateGuideCard("Move", new Color32(56, 184, 255, 255), 0);
            GenerateGuideCard("Dash", new Color32(255, 184, 58, 255), 1);
            GenerateGuideCard("Melee", new Color32(78, 244, 144, 255), 2);
        }

        private static void GenerateBackgroundAndUiResourceGuides()
        {
            GenerateTile("FloorTile", GuideWhite, GuideGray, false);
            GenerateTile("WallTile", GuideWhite, GuideGray, true);
            GenerateTile("FloorMark", new Color32(0, 0, 0, 0),
                new Color32(255, 255, 255, 190), false);
            GeneratePanel("GuidePanel", 512, 512, GuideWhite);
            GeneratePanel("HudPanel", 256, 128, GuideWhite);
            GeneratePanel("Button", 192, 96, GuideWhite);
            GeneratePanel("AimVignette", 512, 512,
                new Color32(68, 68, 68, 190), true);
            GenerateBackgroundGuide("BackgroundFar", 1536, 512, false);
            GenerateBackgroundGuide("BackgroundNear", 1024, 256, true);
            GenerateSolid("GuideDim", 16, 16,
                new Color32(0, 0, 0, 184));
        }

        private enum MockShape { Gun, Disc, Line, Ring, Burst, Warning, Crosshair, Heart }

        private static void GenerateTile(string name, Color32 baseColor,
            Color32 detail, bool planks)
        {
            const int size = 256;
            var texture = NewTexture(size, size, baseColor);
            Color32[] pixels = texture.GetPixels32();
            if (planks)
            {
                for (int y = 32; y < size; y += 48)
                {
                    DrawLine(pixels, size, 0, y, size - 1, y, detail, 2);
                    int seam = ((y / 48) % 2 == 0) ? 92 : 164;
                    DrawLine(pixels, size, seam, y - 24,
                        seam, y + 24, detail, 2);
                }
            }
            else
            {
                DrawLine(pixels, size, 4, 4, size - 5, 4, detail, 3);
                DrawLine(pixels, size, size - 5, 4,
                    size - 5, size - 5, detail, 3);
                DrawLine(pixels, size, size - 5, size - 5,
                    4, size - 5, detail, 3);
                DrawLine(pixels, size, 4, size - 5, 4, 4, detail, 3);
                DrawLine(pixels, size, 42, 128, 102, 128, detail, 3);
                DrawLine(pixels, size, 154, 128, 214, 128, detail, 3);
            }
            texture.SetPixels32(pixels);
            Save(texture, OutputDirectory + "/" + name + ".png");
        }

        private static void GenerateIcon(string name, MockShape shape, Color32 color)
        {
            const int size = 128;
            var texture = NewTexture(size, size, new Color32(0, 0, 0, 0));
            Color32[] p = texture.GetPixels32();
            DrawGuideGrid(p, size, size, new Color32(color.r, color.g, color.b, 64));
            if (shape == MockShape.Gun)
            {
                FillRect(p, size, 16, 57, 92, 16, color);
                FillRect(p, size, 38, 28, 20, 33, color);
            }
            else if (shape == MockShape.Disc)
                FillCircle(p, size, 64, 64, 26, color);
            else if (shape == MockShape.Line)
                DrawLine(p, size, 8, 64, 120, 64, color, 5);
            else if (shape == MockShape.Ring || shape == MockShape.Crosshair)
            {
                DrawRing(p, size, 64, 64, 39, color, 4);
                if (shape == MockShape.Crosshair)
                {
                    DrawLine(p, size, 8, 64, 44, 64, color, 4);
                    DrawLine(p, size, 84, 64, 120, 64, color, 4);
                    DrawLine(p, size, 64, 8, 64, 44, color, 4);
                    DrawLine(p, size, 64, 84, 64, 120, color, 4);
                }
            }
            else if (shape == MockShape.Burst)
            {
                FillCircle(p, size, 64, 64, 16, color);
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI / 4f;
                    DrawLine(p, size, 64, 64,
                        64 + Mathf.RoundToInt(Mathf.Cos(a) * 48),
                        64 + Mathf.RoundToInt(Mathf.Sin(a) * 48), color, 7);
                }
            }
            else if (shape == MockShape.Warning)
            {
                DrawLine(p, size, 64, 20, 24, 104, color, 7);
                DrawLine(p, size, 24, 104, 104, 104, color, 7);
                DrawLine(p, size, 104, 104, 64, 20, color, 7);
                DrawLine(p, size, 64, 45, 64, 78, color, 7);
                FillCircle(p, size, 64, 91, 4, color);
            }
            else
            {
                FillCircle(p, size, 48, 72, 22, color);
                FillCircle(p, size, 80, 72, 22, color);
                for (int y = 28; y <= 72; y++)
                {
                    int half = Mathf.RoundToInt((y - 28) * .8f);
                    FillRect(p, size, 64 - half, y, half * 2 + 1, 2, color);
                }
            }
            texture.SetPixels32(p);
            Save(texture, OutputDirectory + "/" + name + ".png");
        }

        private static void GeneratePanel(string name, int width, int height,
            Color32 color, bool vignette = false)
        {
            var texture = NewTexture(width, height,
                vignette ? new Color32(0, 0, 0, 0) : color);
            Color32[] p = texture.GetPixels32();
            if (vignette)
            {
                Vector2 center = new Vector2(width, height) * .5f;
                float max = center.magnitude;
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float t = Mathf.InverseLerp(.5f, 1f,
                        Vector2.Distance(new Vector2(x, y), center) / max);
                    p[y * width + x] = new Color32(color.r, color.g, color.b,
                        (byte)(color.a * t));
                }
            }
            else
            {
                DrawLine(p, width, 3, 3, width - 4, 3,
                    GuideGray, 6);
                DrawLine(p, width, 3, height - 4, width - 4, height - 4,
                    GuideGray, 6);
            }
            texture.SetPixels32(p);
            Save(texture, OutputDirectory + "/" + name + ".png");
        }

        private static void GenerateBackgroundGuide(string name, int width,
            int height, bool transparent)
        {
            var texture = NewTexture(width, height, transparent
                ? new Color32(0, 0, 0, 0) : GuideWhite);
            Color32[] p = texture.GetPixels32();
            var guide = new Color32(153, 153, 153,
                transparent ? (byte)120 : (byte)180);
            DrawLine(p, width, 0, height / 3, width - 1, height / 3, guide, 4);
            DrawLine(p, width, 0, height * 2 / 3, width - 1, height * 2 / 3,
                guide, 4);
            DrawLine(p, width, 0, height / 2, width - 1, height / 2,
                new Color32(68, 68, 68,
                    transparent ? (byte)110 : (byte)170), 2);
            texture.SetPixels32(p);
            Save(texture, OutputDirectory + "/" + name + ".png");
        }

        private static void GenerateGuideCard(string name, Color32 accent, int kind)
        {
            const int width = 384, height = 216;
            var texture = NewTexture(width, height, new Color32(12, 18, 23, 255));
            Color32[] p = texture.GetPixels32();
            DrawGuideGrid(p, width, height, new Color32(accent.r, accent.g,
                accent.b, 72));
            DrawRing(p, width, 92, 108, 48, accent, 7);
            int offset = kind * 18;
            DrawLine(p, width, 155, 108 - offset, 320, 108 + offset, accent, 12);
            FillCircle(p, width, 155, 108 - offset, 16, accent);
            FillCircle(p, width, 320, 108 + offset, 16, accent);
            texture.SetPixels32(p);
            Save(texture, "Assets/Resources/CountDown/Guide/" + name + ".png");
        }

        private static Texture2D NewTexture(int width, int height, Color32 color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels32(pixels);
            return texture;
        }

        private static void GenerateSolid(string name, int width, int height,
            Color32 color)
        {
            var texture = NewTexture(width, height, color);
            Save(texture, OutputDirectory + "/" + name + ".png");
        }

        private static void Save(Texture2D texture, string path)
        {
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void DrawRing(Color32[] pixels, int width, int cx, int cy,
            int radius, Color32 color, int thickness)
        {
            int outer = radius * radius;
            int inner = (radius - thickness) * (radius - thickness);
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                int d = x * x + y * y;
                if (d <= outer && d >= inner)
                    SetPixel(pixels, width, cx + x, cy + y, color);
            }
        }

        private static void DrawGuideGrid(Color32[] pixels, int width, int height,
            Color32 color)
        {
            int step = Mathf.Max(16, Mathf.Min(width, height) / 4);
            for (int x = 0; x < width; x += step)
                DrawLine(pixels, width, x, 0, x, height - 1, color, 2);
            for (int y = 0; y < height; y += step)
                DrawLine(pixels, width, 0, y, width - 1, y, color, 2);
            DrawLine(pixels, width, 4, 4, width - 5, 4, color, 4);
            DrawLine(pixels, width, 4, height - 5, width - 5, height - 5, color, 4);
            DrawLine(pixels, width, 4, 4, 4, height - 5, color, 4);
            DrawLine(pixels, width, width - 5, 4, width - 5, height - 5, color, 4);
            DrawLine(pixels, width, width / 2, 0, width / 2, height - 1,
                new Color32(color.r, color.g, color.b, 180), 2);
        }

        private static void ConfigureGeneratedMocks()
        {
            string[] sprites =
            {
                "Gun", "Stand", "AimLine", "AimLock", "Projectile",
                "ProjectileTrail", "MuzzleFlash", "EnemyWarning", "MeleeRange",
                "MeleeCooldown", "Crosshair", "HealthPip", "TouchStick",
                "BackgroundNear"
            };
            for (int i = 0; i < sprites.Length; i++)
                ConfigureSingleSprite(OutputDirectory + "/" + sprites[i] + ".png");
        }

        private static void ConfigureSingleSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static void GenerateFuseSheet()
        {
            const int cell = 64;
            const int columns = 6;
            var texture = new Texture2D(cell * columns, cell, TextureFormat.RGBA32, false);
            var pixels = new Color32[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            Color32[] backgrounds =
            {
                new Color32(122, 82, 48, 34),
                new Color32(255, 70, 35, 34),
                new Color32(255, 155, 20, 34),
                new Color32(255, 225, 55, 34),
                new Color32(255, 70, 55, 34),
                new Color32(255, 190, 30, 34)
            };
            for (int column = 0; column < columns; column++)
            {
                int ox = column * cell;
                FillRect(pixels, texture.width, ox, 0, cell, cell, backgrounds[column]);
                DrawLine(pixels, texture.width, ox + 32, 3, ox + 32, 60,
                    new Color32(255, 255, 255, 90));
                DrawLine(pixels, texture.width, ox + 3, 32, ox + 60, 32,
                    new Color32(255, 255, 255, 90));
                for (int i = 0; i <= column; i++)
                    FillRect(pixels, texture.width, ox + 4 + i * 4, 57, 2, 3,
                        new Color32(255, 255, 255, 235));
            }

            // Segment, then three flame poses, then two alert poses.
            DrawLine(pixels, texture.width, 10, 32, 54, 32,
                new Color32(90, 58, 32, 255), 7);
            for (int frame = 0; frame < 3; frame++)
            {
                int ox = (frame + 1) * cell;
                FillCircle(pixels, texture.width, ox + 32, 28 + frame % 2 * 3,
                    13, new Color32(255, 86, 28, 245));
                FillCircle(pixels, texture.width, ox + 32 + frame - 1, 32,
                    7, new Color32(255, 225, 55, 255));
            }
            for (int frame = 0; frame < 2; frame++)
            {
                int ox = (frame + 4) * cell;
                FillRect(pixels, texture.width, ox + 28, 18, 8, 28,
                    new Color32(255, 235, 70, frame == 0 ? (byte)210 : (byte)255));
                FillCircle(pixels, texture.width, ox + 32, 11, 5,
                    new Color32(255, 235, 70, 255));
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(OutputDirectory + "/FuseSheet.png", texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void GenerateSheet(string fileName, string[] states, Color32 accent)
        {
            int width = CellSize * Columns;
            int height = CellSize * states.Length;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            Color32[] rainbow =
            {
                new Color32(255, 68, 68, 92),
                new Color32(255, 190, 45, 92),
                new Color32(57, 220, 120, 92),
                new Color32(76, 132, 255, 92)
            };

            for (int row = 0; row < states.Length; row++)
            for (int frame = 0; frame < Columns; frame++)
            {
                int cellX = frame * CellSize;
                int cellY = (states.Length - row - 1) * CellSize;
                DrawCell(pixels, width, cellX, cellY, rainbow[frame], accent, row, frame);
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(OutputDirectory + "/" + fileName + ".png", texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void DrawCell(Color32[] pixels, int sheetWidth, int ox, int oy,
            Color32 tint, Color32 accent, int row, int frame)
        {
            for (int y = 0; y < CellSize; y++)
            for (int x = 0; x < CellSize; x++)
            {
                bool border = x < 2 || y < 2 || x >= CellSize - 2 || y >= CellSize - 2;
                bool majorGrid = x == 32 || x == 64 || x == 96 ||
                                 y == 32 || y == 64 || y == 96;
                bool minorGrid = x % 16 == 0 || y % 16 == 0;
                Color32 color = tint;
                color.a = (byte)(border ? 220 : majorGrid ? 145 : minorGrid ? 70 : 30);
                SetPixel(pixels, sheetWidth, ox + x, oy + y, color);
            }

            // Ground/baseline: art feet should meet y=12. Center/pivot is x=64.
            DrawLine(pixels, sheetWidth, ox + 8, oy + 12, ox + 119, oy + 12,
                new Color32(255, 255, 255, 230));
            DrawLine(pixels, sheetWidth, ox + 64, oy + 4, ox + 64, oy + 123,
                new Color32(255, 255, 255, 165));

            // A neutral humanoid footprint shows the safe drawing envelope.
            int sway = frame == 1 ? -2 : frame == 3 ? 2 : 0;
            FillRect(pixels, sheetWidth, ox + 50 + sway, oy + 38, 28, 47,
                new Color32(accent.r, accent.g, accent.b, 105));
            FillCircle(pixels, sheetWidth, ox + 64 + sway, oy + 98, 14,
                new Color32(accent.r, accent.g, accent.b, 125));
            DrawLine(pixels, sheetWidth, ox + 54 + sway, oy + 39,
                ox + 48 - sway, oy + 14, new Color32(accent.r, accent.g, accent.b, 160), 7);
            DrawLine(pixels, sheetWidth, ox + 74 + sway, oy + 39,
                ox + 80 + sway, oy + 14, new Color32(accent.r, accent.g, accent.b, 160), 7);

            // Tiny corner code: row bars on the left, frame bars on the right.
            for (int i = 0; i <= row; i++)
                FillRect(pixels, sheetWidth, ox + 5, oy + 119 - i * 4, 8, 2,
                    new Color32(255, 255, 255, 240));
            for (int i = 0; i <= frame; i++)
                FillRect(pixels, sheetWidth, ox + 115, oy + 119 - i * 4, 8, 2,
                    new Color32(255, 255, 255, 240));
        }

        private static void SliceSheet(string path, int rows, string prefix)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;
            importer.compressionQuality = 55;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 1024;

            var rects = new SpriteMetaData[rows * Columns];
            int index = 0;
            string[] playerStates = { "Idle", "Move", "Death", "Melee", "MeleeRecover" };
            string[] enemyStates = { "Idle", "Move", "Death" };
            string[] states = prefix == "Player" ? playerStates : enemyStates;
            for (int row = 0; row < rows; row++)
            for (int frame = 0; frame < Columns; frame++)
            {
                rects[index++] = new SpriteMetaData
                {
                    name = prefix + "_" + states[row] + "_" + frame,
                    rect = new Rect(frame * CellSize, (rows - row - 1) * CellSize,
                        CellSize, CellSize),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, 12f / CellSize),
                    border = Vector4.zero
                };
            }
            importer.spritesheet = rects;
            importer.SaveAndReimport();
        }

        private static void SliceFuseSheet(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;
            importer.compressionQuality = 55;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            string[] names =
            {
                "Fuse_Segment_0", "Fuse_Flame_0", "Fuse_Flame_1",
                "Fuse_Flame_2", "Fuse_Alert_0", "Fuse_Alert_1"
            };
            var rects = new SpriteMetaData[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                rects[i] = new SpriteMetaData
                {
                    name = names[i],
                    rect = new Rect(i * 64, 0, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(.5f, .5f),
                    border = Vector4.zero
                };
            }
            importer.spritesheet = rects;
            importer.SaveAndReimport();
        }

        private static void SetPixel(Color32[] pixels, int width, int x, int y, Color32 color)
        {
            int height = pixels.Length / width;
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            pixels[y * width + x] = color;
        }

        private static void FillRect(Color32[] pixels, int width, int x, int y,
            int rectWidth, int rectHeight, Color32 color)
        {
            int height = pixels.Length / width;
            int minY = Mathf.Max(0, y);
            int maxY = Mathf.Min(height, y + rectHeight);
            int minX = Mathf.Max(0, x);
            int maxX = Mathf.Min(width, x + rectWidth);
            for (int py = minY; py < maxY; py++)
            for (int px = minX; px < maxX; px++)
                SetPixel(pixels, width, px, py, color);
        }

        private static void FillCircle(Color32[] pixels, int width, int cx, int cy,
            int radius, Color32 color)
        {
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                    SetPixel(pixels, width, cx + x, cy + y, color);
        }

        private static void DrawLine(Color32[] pixels, int width, int x0, int y0,
            int x1, int y1, Color32 color, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                FillRect(pixels, width, x0 - thickness / 2, y0 - thickness / 2,
                    thickness, thickness, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * error;
                if (e2 >= dy) { error += dy; x0 += sx; }
                if (e2 <= dx) { error += dx; y0 += sy; }
            }
        }
    }
}
