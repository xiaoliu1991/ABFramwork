using System;
using UnityEngine;

//显示中文
[AttributeUsage(AttributeTargets.Field)]
public class XTip : PropertyAttribute
{
    public string label;        //要显示的字符
    public XTip(string label)
    {
        this.label = label;
    }
}
