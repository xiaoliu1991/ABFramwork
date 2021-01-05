using System;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
public class EditorLoader : BaseLoader
{
    private ResourcesConfig mResConfig;

    public EditorLoader(Def.ModulesType type) : base(type)
    {
    }

    public override Object LoadAsset(string name)
    {
        Check();
        return null;
    }

    public override Object LoadAsset(string name, Type t)
    {
        Check();
        string path = mResConfig.GetCfg(name).gamePath;
        return UnityEditor.AssetDatabase.LoadAssetAtPath(path,t);
    }

    public override T LoadAsset<T>(string name)
    {
        Check();
        string path = mResConfig.GetCfg(name).gamePath;
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public override void LoadAssetAsyn<T>(string name, UnityAction<string, T> action)
    {
    }

    public override void RemoveAsset(string name)
    {
    }

    private void Check()
    {
        LoadResConifg();
    }


    private void LoadResConifg()
    {
        if (mResConfig == null)
        {
            string path = string.Format("Assets/{0}/{1}/ResConfig/ResConfig.asset", Paths.GameResRoot, mType);
            mResConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ResourcesConfig>(path);
            mResConfig.FormatDic();
        }
    }


    public override void Dispose()
    {

    }
}
#endif
