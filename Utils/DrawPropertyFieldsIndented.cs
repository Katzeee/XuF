using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xuf.Utils
{
    public static partial class EditorUtils
    {
#if UNITY_EDITOR
        public static void DrawPropertyFieldsIndented(SerializedProperty property, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Draw all children of the m_behaviorArg property using PropertyField
            EditorGUI.indentLevel++;

            // Iterate through all child properties
            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip if this property is not a direct child of m_behaviorArg
                if (!iterator.propertyPath.StartsWith(property.propertyPath + "."))
                    break;

                // Draw the property field
                EditorGUILayout.PropertyField(iterator, true);
            }

            EditorGUI.indentLevel--;
        }
#endif
    }
}