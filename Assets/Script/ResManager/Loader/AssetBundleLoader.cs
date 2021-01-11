using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class AssetBundleLoader : BaseLoader
{
    private Dictionary<string, GameBundle> mBundleDic = new Dictionary<string, GameBundle>();

    //主包
    private AssetBundle mManifestAB;
    private AssetBundleManifest mManifest;
    private ResourcesConfig mResConfig;

    public AssetBundleLoader(Def.ModulesType type) : base(type)
    {
        
    }

    public override Object LoadAsset(string name)
    {
        Check();
        return LoadAB(name).LoadRes(name);
    }

    public override Object LoadAsset(string name, Type t)
    {
        Check();
        return LoadAB(name).LoadRes(name,t);
    }

    public override T LoadAsset<T>(string name)
    {
        Check();
        return LoadAB(name).LoadRes<T>(name);
    }

    public override void LoadAssetAsyn<T>(string name, UnityAction<string, T> action)
    {
        Check();

    }

    public override void RemoveAsset(string resName)
    {
        string abName = mResConfig.GetCfg(resName).abName;
        if (mBundleDic.ContainsKey(abName))
        {
            mBundleDic[abName].UnLoad();
            List<string> list = mBundleDic[abName].GetClearBundle();
            foreach (var name in list)
            {
                mBundleDic.Remove(name);
            }
        }
    }


    /// <summary>
    /// 加载ab包
    /// </summary>
    private GameBundle LoadAB(string resName)
    {
        string abName = mResConfig.GetCfg(resName).abName;
        GameBundle resBundle = null;
        if (mBundleDic.ContainsKey(abName))
        {
            resBundle = mBundleDic[abName];
        }
        else
        {
            //获取依赖包的相关信息
            string[] strs = mManifest.GetAllDependencies(abName);
            List<GameBundle> refList = new List<GameBundle>();
            for (int i = 0; i < strs.Length; i++)
            {
                if (!mBundleDic.ContainsKey(strs[i]))
                {
                    AssetBundle ab = LoadABFile(strs[i]);
                    var bundle = new GameBundle(strs[i], ab);
                    refList.Add(bundle);
                    mBundleDic.Add(strs[i], bundle);
                }
                else
                {
                    var bundle = mBundleDic[strs[i]];
                    bundle.AddRef();
                    refList.Add(bundle);
                }
            }

            //加载主包来获取资源
            if (!mBundleDic.ContainsKey(abName))
            {
                AssetBundle ab = LoadABFile(abName);
                resBundle = new GameBundle(abName, ab);
                resBundle.SetDependens(refList);
                mBundleDic.Add(abName, resBundle);
            }
        }
        return resBundle;
    }

    private AssetBundle LoadABFile(string path)
    {
        string loadPath = string.Empty;
        AssetBundle ab = null;
        if (!Main.Inst.LoadLocalAsset)
        {
#if UNITY_EDITOR
            loadPath = Paths.PersistentDataPath + Paths.PlatformName + "/" + path;
#else
            loadPath = Paths.PersistentDataPath + path;
#endif
            Debug.LogError("[LoadABFile]: " + loadPath);
            ab = AssetBundle.LoadFromFile(loadPath);
        }
        else
        {
            loadPath = Paths.StreamPath + path;
            if (File.Exists(loadPath))
            {
                ab = AssetBundle.LoadFromFile(loadPath);
            }
            else
            {
#if UNITY_EDITOR
                loadPath = Paths.PersistentDataPath + Paths.PlatformName + "/" + path;
#else
                loadPath = Paths.PersistentDataPath + path;
#endif
                ab = AssetBundle.LoadFromFile(loadPath);
            }
        }
        return ab;
    }

    public override void Dispose()
    {
        foreach (var bundle in mBundleDic)
        {
            bundle.Value.UnLoad();
        }
        mBundleDic.Clear();
    }


    private void Check()
    {
        LoadResConifg();
        LoadManifest();
    }

    private void LoadResConifg()
    {
        if (mResConfig == null)
        {
            string path = string.Format("{0}/resconfig.{1}", mType, Def.AssetBundleSuffix);
            AssetBundle res = LoadABFile(path);
            mResConfig = res.LoadAsset<ResourcesConfig>("resconfig");
            mResConfig.FormatDic();
            res.Unload(true);
        }
    }

    private void LoadManifest()
    {
        if (mManifestAB == null)
        {
            mManifestAB = LoadABFile(Paths.PlatformName);
            mManifest = mManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }
}