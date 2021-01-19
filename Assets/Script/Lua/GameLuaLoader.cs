using System;
using System.IO;
using LuaInterface;
using UnityEngine;

public class GameLuaLoader : LuaFileUtils
{
    public override byte[] ReadFile(string fileName)
    {
        string path = string.Empty;
        if (!beZip)
        {
            path = FindFile(fileName);
            byte[] str = null;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                str = File.ReadAllBytes(path);
            }
            return str;
        }

        byte[] buffer = null;
        //判断传递进来的fileName有没有“.lua”的后缀 没有就加上
        if (!fileName.EndsWith(".lua"))
        {
            fileName += ".lua";
        }

        path = "Lua/" + fileName;
        TextAsset text = Resources.Load(path, typeof(TextAsset)) as TextAsset;
        if (text != null)
        {
            buffer = text.bytes;
            Resources.UnloadAsset(text);
        }

        if (buffer == null)//加载AB包中的lua文件
        {
            var type = GetModulesType(fileName);
            if (type != Def.ModulesType.None)
            {
                string assetName = fileName.Replace("/", "@");
                assetName = assetName.Replace(".lua", "");
                TextAsset luaCode = ResManager.Inst.LoadRes<TextAsset>(type, assetName);
                if (luaCode != null)
                {
                    TextAsset pwd = Resources.Load<TextAsset>("lb_pwd");
                    byte[] bytes = luaCode.bytes;
                    bytes = AES_EnorDecrypt.Decrypt(bytes, Def.AppKey+pwd.text);
                    buffer = bytes;
                    Resources.UnloadAsset(pwd);
                }
            }
        }
        return buffer;
    }


    private Def.ModulesType GetModulesType(string fileName)
    {
        string[] arr = fileName.Split('/');

        foreach (Def.ModulesType t in Enum.GetValues(typeof(Def.ModulesType)))
        {
            if (t.ToString() == arr[0])
            {
                return t;
            }
        }

        return Def.ModulesType.None;
    }
}