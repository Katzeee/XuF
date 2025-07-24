using UnityEditor;
using UnityEngine;
using Xuf.Core;

[CustomPropertyDrawer(typeof(CIntEventArg))]
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

[CustomPropertyDrawer(typeof(CFloatEventArg))]
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

[CustomPropertyDrawer(typeof(CStringEventArg))]
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

[CustomPropertyDrawer(typeof(CBoolEventArg))]
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

[CustomPropertyDrawer(typeof(CTransformEventArg))]
public class TransformEventArgDrawer : PropertyDrawer
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

[CustomPropertyDrawer(typeof(CTransformIntEventArg))]
public class TransformIntEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float lineHeight = EditorGUIUtility.singleLineHeight;
        var transformProp = property.FindPropertyRelative("transform");
        var intValueProp = property.FindPropertyRelative("intValue");
        var transformRect = new Rect(position.x, position.y, position.width, lineHeight);
        var intRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
        EditorGUI.PropertyField(transformRect, transformProp, new GUIContent("Transform"));
        EditorGUI.PropertyField(intRect, intValueProp, new GUIContent("Int Value"));
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}

[CustomPropertyDrawer(typeof(CTransformFloatEventArg))]
public class TransformFloatEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float lineHeight = EditorGUIUtility.singleLineHeight;
        var transformProp = property.FindPropertyRelative("transform");
        var floatValueProp = property.FindPropertyRelative("floatValue");
        var transformRect = new Rect(position.x, position.y, position.width, lineHeight);
        var floatRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
        EditorGUI.PropertyField(transformRect, transformProp, new GUIContent("Transform"));
        EditorGUI.PropertyField(floatRect, floatValueProp, new GUIContent("Float Value"));
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}

[CustomPropertyDrawer(typeof(CTransformStringEventArg))]
public class TransformStringEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float lineHeight = EditorGUIUtility.singleLineHeight;
        var transformProp = property.FindPropertyRelative("transform");
        var stringValueProp = property.FindPropertyRelative("stringValue");
        var transformRect = new Rect(position.x, position.y, position.width, lineHeight);
        var stringRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
        EditorGUI.PropertyField(transformRect, transformProp, new GUIContent("Transform"));
        EditorGUI.PropertyField(stringRect, stringValueProp, new GUIContent("String Value"));
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}

[CustomPropertyDrawer(typeof(CTransformBoolEventArg))]
public class TransformBoolEventArgDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float lineHeight = EditorGUIUtility.singleLineHeight;
        var transformProp = property.FindPropertyRelative("transform");
        var boolValueProp = property.FindPropertyRelative("boolValue");
        var transformRect = new Rect(position.x, position.y, position.width, lineHeight);
        var boolRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
        EditorGUI.PropertyField(transformRect, transformProp, new GUIContent("Transform"));
        EditorGUI.PropertyField(boolRect, boolValueProp, new GUIContent("Bool Value"));
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}
