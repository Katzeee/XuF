using UnityEditor;
using UnityEngine;
using Xuf.UI;

[CustomPropertyDrawer(typeof(IntEventArg))]
public class IntEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProp, label);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(FloatEventArg))]
public class FloatEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProp, label);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(StringEventArg))]
public class StringEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProp, label);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(BoolEventArg))]
public class BoolEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProp, label);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(GameObjectEventArg))]
public class GameObjectEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(position, valueProp, label);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
