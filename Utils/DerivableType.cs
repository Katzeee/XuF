using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
				if (string.IsNullOrEmpty(m_assemblyQualifiedTypeName))
				{
					return null;
				}

				// Resolve without forcing AppDomain to load missing assemblies
				var resolved = ResolveTypeSafely(m_assemblyQualifiedTypeName);
				if (resolved == null)
				{
					Xuf.Core.LogUtils.Warning($"Could not find type: {m_assemblyQualifiedTypeName}");
				}
				return resolved;
            }
        }

        // Constructors
        public DerivableType() { }

        // Implicit conversion from SerializableType to Type
        public static implicit operator Type(DerivableType<T> serializableType)
        {
            return serializableType?.Type;
        }

#if UNITY_EDITOR
		private static List<Type> GetDerivedTypes()
        {
			// Use Unity's TypeCache for fast, cached type discovery in Editor
			var types = TypeCache.GetTypesDerivedFrom<T>()
				.Where(type => !type.IsAbstract && type != typeof(T))
				.OrderBy(t => t.Name)
				.ToList();
			return types;
        }

        public static void DrawTypeDropdown(string label, SerializedProperty typeProperty, Action<Type> onTypeChanged = null)
        {
            SerializedProperty typeNameProp = typeProperty.FindPropertyRelative("m_assemblyQualifiedTypeName");
            string currentTypeName = typeNameProp.stringValue;
            List<Type> availableTypes = GetDerivedTypes();

			// Resolve current type from the stored name without triggering assembly loads
			Type currentType = null;
			if (!string.IsNullOrEmpty(currentTypeName))
			{
				string currentFullName = ExtractFullName(currentTypeName);
				currentType = availableTypes.FirstOrDefault(t =>
					t.AssemblyQualifiedName == currentTypeName || t.FullName == currentFullName);
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
#endif

		private static string ExtractFullName(string storedName)
		{
			int commaIndex = storedName.IndexOf(',');
			return (commaIndex >= 0 ? storedName.Substring(0, commaIndex) : storedName).Trim();
		}

		private static Type ResolveTypeSafely(string storedName)
		{
			string fullName = ExtractFullName(storedName);

			// First try exact match among already loaded assemblies without loading new ones
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(fullName, false);
				if (type != null)
				{
					return type;
				}
			}

			// As a last resort, try Type.GetType but do NOT throw; this may still return null
			try
			{
				return Type.GetType(storedName, false);
			}
			catch
			{
				return null;
			}
		}
    }
}