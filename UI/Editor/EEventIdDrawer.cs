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
        var enumValues = Enum.GetValues(enumType);

        // Build grouped menu
        var menu = new GenericMenu();
        int selectedIndex = -1;

        // Get selected enum name by index (property.enumValueIndex is the index in the enum declaration)
        string selectedPath = null;
        if (property.enumValueIndex >= 0 && property.enumValueIndex < enumNames.Length)
        {
            selectedPath = enumNames[property.enumValueIndex];
        }

        // Handle null selectedPath case
        if (string.IsNullOrEmpty(selectedPath))
        {
            selectedPath = enumNames.Length > 0 ? enumNames[0] : "None";
            property.enumValueIndex = 0;
        }

        for (int i = 0; i < enumNames.Length; i++)
        {
            string enumName = enumNames[i];
            string path = enumName.Replace('_', '/');
            bool isSelected = (enumName == selectedPath);

            if (isSelected) selectedIndex = i;

            // Pass the index i, not the enum value
            menu.AddItem(new GUIContent(path), isSelected, (userData) =>
            {
                property.enumValueIndex = (int) userData;
                property.serializedObject.ApplyModifiedProperties();
            }, i);
        }

        // Show dropdown button
        string displayPath = string.IsNullOrEmpty(selectedPath) ? "None" : selectedPath.Replace('_', '/');
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayPath), FocusType.Keyboard))
        {
            menu.DropDown(dropdownRect);
        }
    }
}
