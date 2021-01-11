using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameBundle
{
    private string mName;
    private AssetBundle mBundle;
    private int mRefCount;

    private Dictionary<string, GameBundle> mDependens = new Dictionary<string, GameBundle>();
    private List<string> mClearAbs = new List<string>();

    public GameBundle(string name)
    {
        mName = name;
    }

    public GameBundle(string name, AssetBundle bundle)
    {
        mName = name;
        mBundle = bundle;
        AddRef();
    }

    public void AddRef()
    {
        mRefCount++;
    }

    public void SetDependens(List<GameBundle> bundles)
    {
        for (int i = 0; i < bundles.Count; i++)
        {
            GameBundle bundle = bundles[i];
            mDependens.Add(bundle.mName, bundle);
        }
    }

    public Object LoadRes(string name)
    {
        return mBundle.LoadAsset(name);
    }

    public T LoadRes<T>(string name) where T:Object
    {
        return (T)mBundle.LoadAsset<T>(name);
    }

    public Object LoadRes(string name,Type t)
    {
        return mBundle.LoadAsset(name, t);
    }

    public void UnLoad()
    {
        mRefCount--;
        foreach (var bundle in mDependens)
        {
            bundle.Value.UnLoad();
            if (bundle.Value.mRefCount <= 0)
            {
                mClearAbs.Add(bundle.Key);
            }
        }
        mDependens.Clear();
        if (mRefCount <= 0)
        {
            mClearAbs.Add(mName);
            mBundle.Unload(true);
        }
    }

    public List<string> GetClearBundle()
    {
        return mClearAbs;
    }
}