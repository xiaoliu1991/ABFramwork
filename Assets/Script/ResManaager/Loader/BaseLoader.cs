

using System;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public abstract class BaseLoader
{
    public Def.ModulesType mType = Def.ModulesType.None;
    public BaseLoader(Def.ModulesType type)
    {
        mType = type;
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    abstract public Object LoadAsset(string name);

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    abstract public Object LoadAsset(string name, Type t);

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    abstract public T LoadAsset<T>(string name) where T : Object;

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="name">资源名</param>
    /// <param name="action">资源回调</param>
    /// <returns></returns>
    abstract public void LoadAssetAsyn<T>(string name, UnityAction<string, T> action) where T : Object;

    /// <summary>
    /// 移除资源
    /// </summary>
    /// <param name="name"></param>
    abstract public void RemoveAsset(string name);

    /// <summary>
    /// 销毁
    /// </summary>
    abstract public void Dispose();
}