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

        [MenuItem("COUNT DOWN/Regenerate Sprite Guides")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutputDirectory);
            GenerateSheet("PlayerSheet", new[]
            {
                "Idle", "Move", "Death", "Melee", "MeleeRecover"
            }, new Color32(24, 178, 255, 255));
            GenerateSheet("EnemySheet", new[]
            {
                "Idle", "Move", "Death"
            }, new Color32(255, 62, 48, 255));
            AssetDatabase.Refresh();
            SliceSheet(OutputDirectory + "/PlayerSheet.png", 5, "Player");
            SliceSheet(OutputDirectory + "/EnemySheet.png", 3, "Enemy");
            AssetDatabase.SaveAssets();
            Debug.Log("COUNT DOWN sprite guides generated successfully.");
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

        private static void SetPixel(Color32[] pixels, int width, int x, int y, Color32 color)
        {
            pixels[y * width + x] = color;
        }

        private static void FillRect(Color32[] pixels, int width, int x, int y,
            int rectWidth, int rectHeight, Color32 color)
        {
            for (int py = y; py < y + rectHeight; py++)
            for (int px = x; px < x + rectWidth; px++)
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
