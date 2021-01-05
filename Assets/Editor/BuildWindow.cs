using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BuildWindow : EditorWindow {

    /// <summary>
    /// 块压缩
    /// </summary>
    public static BuildAssetBundleOptions LZ4Options
    {
        get
        {
            return BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
        }
    }

    /// <summary>
    /// 默认LZMA压缩
    /// </summary>
    public static BuildAssetBundleOptions LZMAOptions
    {
        get
        {
            return BuildAssetBundleOptions.DeterministicAssetBundle;
        }
    }

    private bool IsCopyToStream = false;
    [MenuItem("Build/BuildWindow")]
    static void Init()
    {
        BuildWindow window = (BuildWindow)EditorWindow.GetWindow(typeof(BuildWindow));
        window.Show();
    }

    public void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("刷新资源配置", GUILayout.Height(40)))
        {
            RefreshResConfig();
        }

        GUILayout.Space(5);
        IsCopyToStream = GUILayout.Toggle(IsCopyToStream, "是否拷贝到流目录");
        //清理标记
        if (GUILayout.Button("打包", GUILayout.Height(40)))
        {
            string version = DateTime.Now.ToString("yyMMddHHmmss");
            ClearMarks();
            MarkAssetLabels();
            RefreshResConfig();
            string exportPath = GetExportPath(version);
            BuildPipeline.BuildAssetBundles(exportPath, LZ4Options,EditorUserBuildSettings.activeBuildTarget);
            ClearManifest(exportPath);
            CreateFileList(exportPath, version);
            if (IsCopyToStream)MoveBundle(exportPath);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("拷贝热更到服务器", GUILayout.Height(40)))
        {
            FileUtils.CopyDirectory(Paths.BuildABPath, "F:/tomcat9/webapps/Resource/");
        }
        GUILayout.EndVertical();
    }


    public static void ClearMarks()
    {
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        if (names.Length < 1)
            return;
        int startIndex = 0;
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[startIndex];
            EditorUtility.DisplayProgressBar("清理标记中", name, (float)(startIndex + 1) / (float)names.Length);
            AssetDatabase.RemoveAssetBundleName(name, true);
            startIndex++;
            if (startIndex >= names.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                break;
            }
        }
    }

    public static void MarkAssetLabels()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + Paths.GameResRoot);
        foreach (DirectoryInfo childDir in dir.GetDirectories())
        {
            string buildConfigPath = string.Format("Assets/"+ Paths.GameResRoot + "/{0}/BuildConfigs.asset", childDir.Name);
            ABBuildConfigs configs = AssetDatabase.LoadAssetAtPath<ABBuildConfigs>(buildConfigPath);

            ABMarkConfig[] paths = configs.OneFloderOneBundle;
            foreach (var p in paths)
            {
                AssetImporter ai = AssetImporter.GetAtPath(p.AssetPath);
                ai.SetAssetBundleNameAndVariant(p.GamePath, Def.AssetBundleSuffix);
            }

            paths = configs.SubFloderOneBundle;
            foreach (var p in paths)
            {
                DirectoryInfo assetDir = new DirectoryInfo(p.AssetPath);
                foreach (DirectoryInfo subDir in assetDir.GetDirectories())
                {
                    AssetImporter ai = AssetImporter.GetAtPath(p.AssetPath + "/" + subDir.Name);
                    ai.SetAssetBundleNameAndVariant(p.GamePath + "/" + subDir.Name, Def.AssetBundleSuffix);
                }
            }

            paths = configs.OneAssetOneBundle;
            foreach (var p in paths)
            {
                AssetImporter ai = AssetImporter.GetAtPath(p.AssetPath);
                ai.SetAssetBundleNameAndVariant(p.GamePath, Def.AssetBundleSuffix);
            }
        }
    }

    public static void RefreshResConfig()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + Paths.GameResRoot);
        foreach (DirectoryInfo childDir in dir.GetDirectories())
        {
            List<SingleResCfg> configList = new List<SingleResCfg>();
            string buildConfigPath = string.Format("Assets/" + Paths.GameResRoot + "/{0}/BuildConfigs.asset", childDir.Name);
            ABBuildConfigs configs = AssetDatabase.LoadAssetAtPath<ABBuildConfigs>(buildConfigPath);
            ABMarkConfig[] paths = configs.OneFloderOneBundle;
            foreach (var p in paths)
            {
                string abName = (p.GamePath + "." + Def.AssetBundleSuffix).ToLower();
                DirectoryInfo childRootDir = new DirectoryInfo(p.AssetPath);
                FileInfo[] files = childRootDir.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    if (file.Extension.Equals(".meta") || file.Extension.Equals(".DS_Store")) continue;
                    configList.Add(new SingleResCfg(file.Name.Replace(file.Extension, ""), file.Extension, abName, "Assets/" + file.FullName.Replace("\\","/").Replace(Application.dataPath+"/","")));
                }
            }

            paths = configs.SubFloderOneBundle;
            foreach (var p in paths)
            {
                DirectoryInfo assetDir = new DirectoryInfo(p.AssetPath);
                foreach (DirectoryInfo subDir in assetDir.GetDirectories())
                {
                    string abName = (p.GamePath + "/" + subDir.Name + "." + Def.AssetBundleSuffix).ToLower();
                    FileInfo[] files = subDir.GetFiles("*", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i];
                        if (file.Extension.Equals(".meta") || file.Extension.Equals(".DS_Store")) continue;
                        configList.Add(new SingleResCfg(file.Name.Replace(file.Extension, ""), file.Extension, abName, "Assets/" + file.FullName.Replace("\\", "/").Replace(Application.dataPath + "/", "")));
                    }
                }
            }

            paths = configs.OneAssetOneBundle;
            foreach (var p in paths)
            {
                string abName = (p.GamePath + "." + Def.AssetBundleSuffix).ToLower();
                if (Directory.Exists(Application.dataPath+"/"+p.GamePath))
                {
                    DirectoryInfo assetDir = new DirectoryInfo(p.AssetPath);
                    FileInfo[] files = assetDir.GetFiles("*", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i];
                        if (file.Extension.Equals(".meta") || file.Extension.Equals(".DS_Store")) continue;
                        configList.Add(new SingleResCfg(file.Name.Replace(file.Extension, ""), file.Extension, abName, "Assets/" + file.FullName.Replace("\\", "/").Replace(Application.dataPath + "/", "")));
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(p.AssetPath);
                    configList.Add(new SingleResCfg(file.Name.Replace(file.Extension, ""), file.Extension, abName, "Assets/"+file.FullName.Replace("\\", "/").Replace(Application.dataPath + "/", "")));
                }
            }

            string path = childDir.Name + "/ResConfig/ResConfig";
            ResourcesConfig asset = Resources.Load<ResourcesConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ResourcesConfig>();
                path = "Assets/" + Paths.GameResRoot + "/" + childDir.Name+"/ResConfig/ResConfig.asset";
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.configs = configList.ToArray();
            EditorUtility.SetDirty(asset);
        }

    }

    /// <summary>
    /// 清理多余的Manifest
    /// </summary>
    public static void ClearManifest(string path)
    {
        string fileName = Path.GetFileName(path);
        string[] files = Directory.GetFiles(path, "*.manifest", SearchOption.AllDirectories);
        foreach (var each in files)
        {
            if (!each.EndsWith(fileName+ ".manifest"))
            {
                File.Delete(each);
            }
        }
    }

    public static void CreateFileList(string exportPath,string version)
    {
        ///----------------------创建文件列表-----------------------
        string filePath = exportPath + "/files.txt";
        if (File.Exists(filePath)) File.Delete(filePath);
        string versionFilePath = Application.dataPath + "/../BuildABs" + "/version.txt";
        if (File.Exists(versionFilePath)) File.Delete(versionFilePath);
        StringBuilder sb = new StringBuilder();
        DirectoryInfo dir = new DirectoryInfo(exportPath);
        FileInfo[] files = dir.GetFiles("*",SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i];
            if (file.Extension.Equals(".meta") || file.Extension.Equals(".DS_Store"))
            {
                continue;
            }

            string crc = FileToCRC32.GetFileCRC32(file.FullName);
            string value = file.FullName.Replace(dir.FullName+"\\", string.Empty);
            sb.AppendLine(value + "|" + crc + "|" + file.Length);
        }
        File.WriteAllText(filePath, sb.ToString());
        File.WriteAllText(versionFilePath, version);
    }

    public static void MoveBundle(string exportPath)
    {
        string path = Paths.StreamPath;
        if (Directory.Exists(path))
        {
            Directory.Delete(path,true);
        }
        FileUtils.CopyDirectory(exportPath, path);

        path = Application.streamingAssetsPath + "/version.txt";
        File.Copy(Paths.BuildABPath+ "version.txt", path,true);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取导出路径
    /// </summary>
    /// <returns>The export path.</returns>
    public static string GetExportPath(string version)
    {
        string floder = "Other";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                floder = "Android";
                break;
            case BuildTarget.iOS:
                floder = "IOS";
                break;
            case BuildTarget.StandaloneWindows:
                floder = "Windows";
                break;
            case BuildTarget.StandaloneWindows64:
                floder = "Windows";
                break;
        }

        floder = Paths.BuildABPath + version + "/" + floder;
        if (Directory.Exists(floder))Directory.Delete(floder,true);
        Directory.CreateDirectory(floder);
        return floder;
    }
}
