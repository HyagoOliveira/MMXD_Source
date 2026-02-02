using UnityEditor;
using UnityEngine;
using System.IO;
using System.Drawing;

namespace MMXP.Editor
{
    /// <summary>
    /// Provides utility methods for extracting individual sprites from Unity sprite assets 
    /// via the Unity Editor context menu.
    /// </summary>
    /// <remarks>
    /// This class is intended for use within the Unity Editor and adds a context menu item for
    /// sprite extraction. 
    /// It supports extracting sprites from supported asset types and saves them as separate PNG
    /// files. 
    /// Extraction is not available for certain file types, such as PSD files.
    /// </remarks>
    public static class SpriteExtractor
    {
        private const string MENU_PATH = "CONTEXT/Sprite/Extract Sprite";

        [MenuItem(MENU_PATH)]
        private static void CreateMenuItem(MenuCommand command) => ExtractSprite((Sprite)command.context);

        [MenuItem(MENU_PATH, isValidateFunction: true)]
        private static bool ValidateMenuItem() => Selection.objects.Length > 0 && Selection.objects[0] is Sprite;

        private static void ExtractSprite(Sprite originalSprite)
        {
            var path = AssetDatabase.GetAssetPath(originalSprite);
            var extension = Path.GetExtension(path).ToLowerInvariant();
            var isInvalidFile = extension.Equals(".psd");

            if (isInvalidFile)
            {
                Debug.LogError($"Cannot extract sprite from file type: {extension}");
                return;
            }

            var folder = Path.GetDirectoryName(path);
            var newFileName = originalSprite.name + ".png";
            var newAssetPath = Path.Combine(folder, newFileName);
            var spriteRect = originalSprite.rect;
            var width = Mathf.RoundToInt(spriteRect.width);
            var height = Mathf.RoundToInt(spriteRect.height);

            using var source = new Bitmap(path);
            using var sprite = new Bitmap(width, height);

            var rect = spriteRect.ToRectangle(source.Height);
            var clonedSprite = source.Clone(rect, source.PixelFormat);

            sprite.MakeTransparent(System.Drawing.Color.White);

            using var g = System.Drawing.Graphics.FromImage(sprite);

            g.DrawImageUnscaled(clonedSprite, 0, 0);

            sprite.Save(newAssetPath);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(newAssetPath);

            var importer = AssetImporter.GetAtPath(newAssetPath) as TextureImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = originalSprite.texture.filterMode;
            importer.spritePivot = originalSprite.pivot;
            importer.spriteBorder = originalSprite.border;
            importer.spritePixelsPerUnit = originalSprite.pixelsPerUnit;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"Sprite '{originalSprite.name}' extracted into: {newAssetPath}");

            var newAsset = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(Texture2D));
            if (newAsset != null)
            {
                Selection.activeObject = newAsset;
                EditorGUIUtility.PingObject(newAsset);
            }
        }

        private static Rectangle ToRectangle(this Rect rect, int textureHeight)
        {
            var y = ConvertToBitmapY(textureHeight, (int)rect.height, (int)rect.y);
            return new((int)rect.x, y, (int)rect.width, (int)rect.height);
        }

        private static int ConvertToBitmapY(int textureHeight, int spriteHeight, int y) => textureHeight - (y + spriteHeight);
    }
}