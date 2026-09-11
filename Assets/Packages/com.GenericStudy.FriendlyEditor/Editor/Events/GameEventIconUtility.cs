using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Genera la previsualización propia de los assets GameEvent.</summary>
[InitializeOnLoad]
internal static class GameEventIconUtility
{
    private const string IconPath = "Assets/Packages/com.GenericStudy.FriendlyEditor/Editor/Events/GameEventIcon.png";
    private const string GameEventScriptPath = "Assets/Packages/com.GenericStudy.FriendlyEditor/RuneTime/Events/GameEvent.cs";

    private static Texture2D icon;

    static GameEventIconUtility()
    {
        EditorApplication.delayCall += InstallOnGameEventScript;
    }

    internal static Texture2D Icon
    {
        get
        {
            if (icon == null)
            {
                icon = CreateIcon();
            }

            return icon;
        }
    }

    internal static Texture2D CreatePreview(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "Game Event Icon",
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        var clear = new Color(0f, 0f, 0f, 0f);
        var signal = new Color(0.12f, 0.82f, 0.98f, 1f);
        var pixels = new Color[width * height];
        var scale = Mathf.Min(width, height) / 64f;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = (x - (width - 1) * 0.5f) / scale;
                var dy = (y - (height - 1) * 0.5f) / scale;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var center = distance <= 8f;
                var innerWave = Mathf.Abs(distance - 16f) <= 2.25f;
                var outerWave = Mathf.Abs(distance - 25f) <= 2.25f;

                // Las ondas abiertas hacia la derecha comunican que el punto central emite un evento.
                var rightSide = dx >= -2f;
                pixels[y * width + x] = center || (rightSide && (innerWave || outerWave))
                    ? signal
                    : clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static void InstallOnGameEventScript()
    {
        var persistentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (persistentIcon == null)
        {
            var generatedIcon = CreatePreview(64, 64);
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var absoluteIconPath = Path.Combine(projectRoot ?? string.Empty, IconPath);
            File.WriteAllBytes(absoluteIconPath, generatedIcon.EncodeToPNG());
            Object.DestroyImmediate(generatedIcon);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport);

            var textureImporter = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.SaveAndReimport();
            }

            persistentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        }

        if (persistentIcon == null)
        {
            return;
        }

        if ((persistentIcon.hideFlags & HideFlags.HideInHierarchy) == 0)
        {
            persistentIcon.hideFlags |= HideFlags.HideInHierarchy;
            EditorUtility.SetDirty(persistentIcon);
            AssetDatabase.SaveAssets();
        }

        icon = persistentIcon;

        var importer = AssetImporter.GetAtPath(GameEventScriptPath) as MonoImporter;
        if (importer != null && importer.GetIcon() != persistentIcon)
        {
            importer.SetIcon(persistentIcon);
            importer.SaveAndReimport();
        }
    }

    private static Texture2D CreateIcon()
    {
        return CreatePreview(64, 64);
    }
}
