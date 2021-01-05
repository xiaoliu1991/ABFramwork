

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResManager : Singleton<ResManager>
{
    private Dictionary<Def.ModulesType,BaseLoader> mLoader = new Dictionary<Def.ModulesType, BaseLoader>();

    public Object LoadRes(Def.ModulesType type, string name)
    {
        if (!mLoader.ContainsKey(type))
        {
            mLoader.Add(type,GetLoader(type));
        }
        Object obj = mLoader[type].LoadAsset(name);
        if (obj is GameObject)
            return GameObject.Instantiate(obj);
        else
            return obj;
    }

    public Object LoadRes(Def.ModulesType type, string name,Type t)
    {
        if (!mLoader.ContainsKey(type))
        {
            mLoader.Add(type, GetLoader(type));
        }
        Object obj = mLoader[type].LoadAsset(name, t);
        if (obj is GameObject)
            return GameObject.Instantiate(obj);
        else
            return obj;
    }

    public T LoadRes<T>(Def.ModulesType type, string name) where T:Object
    {
        if (!mLoader.ContainsKey(type))
        {
            mLoader.Add(type, GetLoader(type));
        }
        T obj = (T)mLoader[type].LoadAsset<T>(name);
        if (obj is GameObject)
            return GameObject.Instantiate(obj);
        else
            return obj;
    }

    private BaseLoader GetLoader(Def.ModulesType type)
    {
        BaseLoader loader = null;
#if UNITY_EDITOR
        if (Main.Inst.IsABLoad)
        {
            loader = new AssetBundleLoader(type);
        }
        else
        {
            loader = new EditorLoader(type);
        }
#else
        loader = new AssetBundleLoader(type);
#endif
        return loader;
    }

    public void Remove(Def.ModulesType type, string name)
    {
        if (mLoader.ContainsKey(type))
        {
            mLoader[type].RemoveAsset(name);
        }
    }

    public void Dispose()
    {
        foreach (var loader in mLoader)
        {
            loader.Value.Dispose();
        }
        mLoader.Clear();
    }
}