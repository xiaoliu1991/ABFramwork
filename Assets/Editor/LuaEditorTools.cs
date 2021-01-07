using System.IO;
using UnityEditor;
using UnityEngine;

public class LuaEditorTools
{
    [MenuItem("Assets/创建Lua")]
    public static void CreatLuaFile()
    {
        string path = Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(Selection.activeObject)+"/A.lua";
        File.WriteAllText(path,"");
        AssetDatabase.Refresh();
    }
}