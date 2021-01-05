using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomImportSettings : AssetPostprocessor
{
    const int MAX_TEXTURE_SIZE = 2048;

    public static bool enabled = true;

//    private static Regex MAP_TEXTURE = new Regex(@"Map/[\d]*/");

//    private static TextureImporterSettings importerSettings = new TextureImporterSettings();

    private static int PIXELTOWORLD = 100;
    /// <summary>
    /// 图片处理
    /// </summary>
    private static Dictionary<string, Action<TextureImporter, string>> handlers = new Dictionary<string, Action<TextureImporter, string>>
    {
        //UI
        {"/Atlas/", SetUITexture},
        {"/Font/",SetFontTexture},
        {"/Effect/",SetEffectTexture },
    };


    void OnPreprocessAudio()
    {
//        AudioImporter audioImporter = assetImporter as AudioImporter;
    }


    void OnPreprocessTexture()
    {
        if (!enabled) return;
        TextureImporter importer = assetImporter as TextureImporter;
        importer.spritePackingTag = "";
        UpdateTextureSetting(importer, assetPath);
    }


    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        foreach (string move in movedAssets)
        {
            //这里重新 import一下
            AssetDatabase.ImportAsset(move);
        }
    }

    /// <summary>
    /// 设置UI图片自动图集名称,与图片格式
    /// </summary>
    static void SetUITexture(TextureImporter importer, string path)
    {
        importer.spritePackingTag = GetAtlasName(importer.assetPath);
        importer.SetTextureSettingsExt(true, TextureImporterType.Sprite, 1, PIXELTOWORLD, false, TextureWrapMode.Clamp, FilterMode.Trilinear, TextureImporterNPOTScale.None);
        importer.SetPlatformSettingsExt("Android", TextureImporterFormat.ETC2_RGBA8, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("iPhone", TextureImporterFormat.ASTC_RGBA_5x5, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("Standalone", TextureImporterFormat.RGBA32, MAX_TEXTURE_SIZE, 50, false);
    }

 
    /// <summary>
    /// 设置UI图片自动图集名称,与图片格式
    /// </summary>
    static void SetEffectTexture(TextureImporter importer, string path)
    {
        importer.spritePackingTag = GetAtlasName(importer.assetPath);
        importer.SetPlatformSettingsExt("Android", TextureImporterFormat.ETC2_RGBA8, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("iPhone", TextureImporterFormat.ASTC_RGBA_5x5, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("Standalone", TextureImporterFormat.RGBA32, MAX_TEXTURE_SIZE, 50, false);
    }

    /// <summary>
    /// 设置UI图片自动图集名称,与图片格式
    /// </summary>
    static void SetFontTexture(TextureImporter importer, string path)
    {
        importer.spritePackingTag = "";
        importer.SetTextureSettingsExt(true, TextureImporterType.Sprite, 1, PIXELTOWORLD, false, TextureWrapMode.Clamp, FilterMode.Trilinear, TextureImporterNPOTScale.None);
        importer.SetPlatformSettingsExt("Android", TextureImporterFormat.ETC2_RGBA8, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("iPhone", TextureImporterFormat.ASTC_RGBA_5x5, MAX_TEXTURE_SIZE, 50, false);
        importer.SetPlatformSettingsExt("Standalone", TextureImporterFormat.RGBA32, MAX_TEXTURE_SIZE, 50, false);
    }

    public static string GetAtlasName(string assetPath)
    {
        int index = assetPath.IndexOf("/Atlas/");
        string atlasName = assetPath.Substring(index + 7);
        index = atlasName.IndexOf("/");
        if (index != -1)
        {
            atlasName = atlasName.Substring(0, index);
        }
        return atlasName;
    }

    /// <summary>
    /// 更新一张图片设置
    /// </summary>
    public static void UpdateTextureSetting(TextureImporter importer, string assetPath)
    {
        foreach (var each in handlers)
        {
            if (importer.assetPath.Contains(each.Key))
            {
                each.Value(importer, each.Key);
                break;
            }
        }
    }
}


public static class ImporterExt
{
    /// <summary>
    /// 更改音效设置
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="platform"></param>
    /// <param name="loadType"></param>
    public static void SetAudioSettingExt(this AudioImporter importer, string platform, AudioClipLoadType loadType, AudioCompressionFormat format, float quality = 1)
    {
        AudioImporterSampleSettings settings = importer.GetOverrideSampleSettings(platform);
        settings.loadType = loadType;
        settings.compressionFormat = format;
        settings.quality = quality;
        importer.SetOverrideSampleSettings(platform, settings);
    }

    /// <summary>
    /// 设置Texture参数
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="type"></param>
    /// <param name="spriteMode"></param>
    /// <param name="spritePixelsPerUnit"></param>
    /// <param name="mipmapEnabled"></param>
    /// <param name="wrapMode"></param>
    /// <param name="filterMode"></param>
    /// <param name="noptScale"></param>
    public static void SetTextureSettingsExt(this TextureImporter importer, bool alphaIsTransparency, TextureImporterType type, int spriteMode, float spritePixelsPerUnit, bool mipmapEnabled, TextureWrapMode wrapMode, FilterMode filterMode, TextureImporterNPOTScale noptScale)
    {
        TextureImporterSettings importerSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(importerSettings);
        importerSettings.npotScale = noptScale;
        importerSettings.spriteMode = spriteMode;
        importerSettings.spritePixelsPerUnit = spritePixelsPerUnit;
        importerSettings.mipmapEnabled = mipmapEnabled;
        importerSettings.wrapMode = wrapMode;
        importerSettings.filterMode = filterMode;
        importerSettings.alphaIsTransparency = alphaIsTransparency;
        importerSettings.textureType = type;
        importer.SetTextureSettings(importerSettings);
    }

    /// <summary>
    /// 设置平台参数
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="platform"></param>
    /// <param name="format"></param>
    /// <param name="maxSize"></param>
    /// <param name="compressionQuality"></param>
    /// <param name="allowsAlphaSplitting"></param>
    /// <param name="overridden"></param>
    public static void SetPlatformSettingsExt(this TextureImporter importer, string platform, TextureImporterFormat format, int maxSize, int compressionQuality, bool allowsAlphaSplitting)
    {
        TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
        settings.name = platform;
        settings.maxTextureSize = maxSize;
        settings.format = format;
        settings.compressionQuality = compressionQuality;
        settings.allowsAlphaSplitting = allowsAlphaSplitting;
        settings.overridden = true;
        importer.SetPlatformTextureSettings(settings);
    }
}
