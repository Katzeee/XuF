using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Xuf.Utils
{
    [Serializable]
    public class DerivableType<T>
    {
        [SerializeField]
        private string m_assemblyQualifiedTypeName;

        // Property to get/set the type
        public Type Type
        {
            get
            {
                Type type = null;
                // Load the type if it's not loaded yet
                if (type == null && !string.IsNullOrEmpty(m_assemblyQualifiedTypeName))
                {
                    type = Type.GetType(m_assemblyQualifiedTypeName);

                    // If not found, try to find the type in all loaded assemblies
                    if (type == null)
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            type = assembly.GetType(m_assemblyQualifiedTypeName);
                            if (type != null)
                                break;
                        }
                    }

                    if (type == null)
                    {
                        Xuf.Core.LogUtils.Warning($"Could not find type: {m_assemblyQualifiedTypeName}");
                    }
                }
                return type;
            }
        }

        // Constructors
        public DerivableType() { }

        // Implicit conversion from SerializableType to Type
        public static implicit operator Type(DerivableType<T> serializableType)
        {
            return serializableType?.Type;
        }

        private static List<Type> GetDerivedTypes()
        {
            var derivedTypes = new List<Type>();

            // Get all types that derive from the base type
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Find all types that derive from the base type
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(T).IsAssignableFrom(type) && !type.IsAbstract && type != typeof(T))
                        {
                            derivedTypes.Add(type);
                        }

                    }
                }
                catch (Exception ex)
                {
                    // Ignore reflection exceptions
                    Xuf.Core.LogUtils.Warning($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            // Sort types by name for better display
            return derivedTypes.OrderBy(t => t.Name).ToList();
        }

        public static void DrawTypeDropdown(string label, SerializedProperty typeProperty, Action<Type> onTypeChanged = null)
        {
            SerializedProperty typeNameProp = typeProperty.FindPropertyRelative("m_assemblyQualifiedTypeName");
            string currentTypeName = typeNameProp.stringValue;
            List<Type> availableTypes = GetDerivedTypes();

            // Get the current type from the serialized name
            Type currentType = null;
            if (!string.IsNullOrEmpty(currentTypeName))
            {
                currentType = Type.GetType(currentTypeName);
            }

            // Create a rect for the property field
            Rect position = EditorGUILayout.GetControlRect();

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent(label));

            // Create a display name for the current selection
            string displayName = currentType != null ? currentType.Name : "[No Type Selected]";

            // Draw the dropdown
            if (EditorGUI.DropdownButton(position, new GUIContent(displayName), FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                // Add a "None" option
                menu.AddItem(new GUIContent("None"), currentType == null, () =>
                {
                    typeNameProp.stringValue = "";
                    onTypeChanged?.Invoke(null);
                    typeProperty.serializedObject.ApplyModifiedProperties();
                });

                // Add each available type to the menu
                foreach (var type in availableTypes)
                {
                    menu.AddItem(new GUIContent(type.Name), currentType == type, () =>
                    {
                        typeNameProp.stringValue = type.AssemblyQualifiedName;
                        onTypeChanged?.Invoke(type);
                        typeProperty.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(position);
            }
        }
    }
}