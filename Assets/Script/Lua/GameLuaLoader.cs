using System.IO;
using LuaInterface;
using UnityEngine;

public class GameLuaLoader : LuaFileUtils
{
    public override byte[] ReadFile(string fileName)
    {
        if (!beZip)
        {
            string path = FindFile(fileName);
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

        if (buffer == null)//加载Resources下面的lua文件
        {
            string path = "Lua/" + fileName;
            TextAsset text = Resources.Load(path, typeof(TextAsset)) as TextAsset;
            if (text != null)
            {
                buffer = text.bytes;
                Resources.UnloadAsset(text);
            }
        }
        if (buffer == null)//加载AB包中的lua文件
        {
//            AssetBundle bundle = map[fileName];
//            TextAsset luaCode = bundle.LoadAsset<TextAsset>(fileName);
//
//            if (luaCode != null)
//            {
//                buffer = luaCode.bytes;
//                Resources.UnloadAsset(luaCode);
//            }
        }

        return buffer;

    }
}