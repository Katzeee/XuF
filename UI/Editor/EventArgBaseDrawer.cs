using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Xuf.UI;

[CustomPropertyDrawer(typeof(EventArgBase), true)]
public class EventArgBaseDrawer : PropertyDrawer
{
    // Cache all non-abstract types derived from EventArgBase
    private static Type[] _argTypes;
    private static string[] _typeNames;

    static EventArgBaseDrawer()
    {
        var baseType = typeof(EventArgBase);
        _argTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType)
            .ToArray();
        _typeNames = _argTypes.Select(t => t.Name).ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, label);

        // Get current type
        var typeProp = property.managedReferenceFullTypename;
        int currentIndex = -1;
        if (!string.IsNullOrEmpty(typeProp))
        {
            var typeName = typeProp.Split(' ').Last();
            currentIndex = Array.FindIndex(_argTypes, t => t.FullName == typeName);
        }

        // Draw type selection popup
        int newIndex = EditorGUI.Popup(
            new Rect(position.x, position.y, 120, position.height),
            currentIndex < 0 ? 0 : currentIndex,
            _typeNames
        );

        // If type changed, create new instance
        if (newIndex != currentIndex)
        {
            property.managedReferenceValue = Activator.CreateInstance(_argTypes[newIndex]);
        }

        // Draw value field if instance exists
        if (property.hasVisibleChildren)
        {
            var valueProp = property.FindPropertyRelative("value");
            if (valueProp != null)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x + 130, position.y, position.width - 130, position.height),
                    valueProp,
                    GUIContent.none
                );
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
