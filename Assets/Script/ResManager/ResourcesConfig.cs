using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "创建资源配置文件")]
public class ResourcesConfig : ScriptableObject
{
    [SerializeField]
    public SingleResCfg[] configs;

    private Dictionary<string, SingleResCfg> dic;

    public void FormatDic()
    {
        if (dic == null)
        {
            dic = new Dictionary<string, SingleResCfg>();
            foreach (var cfg in configs)
            {
                dic.Add(cfg.name,cfg);
            }
        }
    }

    public SingleResCfg GetCfg(string name)
    {
        SingleResCfg cfg = null;
        dic.TryGetValue(name, out cfg);
        return cfg;
    }
}

[System.Serializable]
public class SingleResCfg
{
    [SerializeField]
    public string name;
    [SerializeField]
    public string extension;
    [SerializeField]
    public string abName;
    [SerializeField]
    public string gamePath;

    public SingleResCfg(string name, string extension, string abName, string gamePath)
    {
        this.name = name;
        this.extension = extension;
        this.abName = abName;
        this.gamePath = gamePath;
    }
}