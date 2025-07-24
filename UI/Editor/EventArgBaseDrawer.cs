using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Xuf.Core;

[CustomPropertyDrawer(typeof(CEventArgBase), true)]
public class EventArgBaseDrawer : PropertyDrawer
{
    // Cache all non-abstract types derived from EventArgBase
    private static Type[] _argTypes;
    private static string[] _typeNames;

    static EventArgBaseDrawer()
    {
        var baseType = typeof(CEventArgBase);
        _argTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType)
            .ToArray();
        _typeNames = _argTypes.Select(t => t.Name).ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout for the whole Event Arg
        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "Event Arg", true);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            // Draw type selection popup on the next line
            var popupRect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);
            var typeProp = property.managedReferenceFullTypename;
            int currentIndex = -1;
            if (!string.IsNullOrEmpty(typeProp))
            {
                var typeName = typeProp.Split(' ').Last();
                currentIndex = Array.FindIndex(_argTypes, t => t.FullName == typeName);
            }
            int newIndex = EditorGUI.Popup(
                popupRect,
                currentIndex < 0 ? 0 : currentIndex,
                _typeNames
            );
            // If type changed, create new instance
            if (newIndex != currentIndex)
            {
                property.managedReferenceValue = Activator.CreateInstance(_argTypes[newIndex]);
            }
            // Draw value field if instance exists, on the next line
            if (property.hasVisibleChildren)
            {
                var valueProp = property.FindPropertyRelative("value");
                if (valueProp != null)
                {
                    var valueRect = new Rect(position.x, position.y + lineHeight * 2, position.width, lineHeight);
                    EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;
        // Three lines: foldout, type, value
        return EditorGUIUtility.singleLineHeight * 3;
    }
}
