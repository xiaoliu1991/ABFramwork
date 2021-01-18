using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(XTip))]
public class XTipDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        XTip rename = (XTip)attribute;
        label.text = rename.label;
        EditorGUI.PropertyField(position, property, label);
    }
}