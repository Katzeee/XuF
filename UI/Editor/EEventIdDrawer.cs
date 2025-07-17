// Scripts/XuF/UI/Editor/EEventIdDrawer.cs
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(EEventId))]
public class EEventIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw field label and get content area
        position = EditorGUI.PrefixLabel(position, label);
        // Adjust dropdown button width
        float dropdownWidth = position.width;
        Rect dropdownRect = new Rect(position.x, position.y, dropdownWidth, position.height);
        // Get all enum values
        var enumType = fieldInfo.FieldType;
        var enumNames = Enum.GetNames(enumType);

        // Build grouped menu
        var menu = new GenericMenu();
        int selectedIndex = -1;
        string selectedPath = Enum.GetName(enumType, property.enumValueIndex);

        for (int i = 0; i < enumNames.Length; i++)
        {
            string enumName = enumNames[i];
            string path = enumName.Replace('_', '/');
            bool isSelected = (enumName == selectedPath);

            if (isSelected) selectedIndex = i;

            menu.AddItem(new GUIContent(path), isSelected, (userData) =>
            {
                property.enumValueIndex = (int) userData;
                property.serializedObject.ApplyModifiedProperties();
            }, i);
        }

        // Show dropdown button
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(selectedPath.Replace('_', '/')), FocusType.Keyboard))
        {
            menu.DropDown(dropdownRect);
        }
    }
}
